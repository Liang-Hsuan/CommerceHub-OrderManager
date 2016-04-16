using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Order_Manager.channel.brightpearl;
using Order_Manager.channel.sears;
using Order_Manager.supportingClasses;
using Order_Manager.supportingClasses.Address;
using Order_Manager.supportingClasses.Shipment;
using Order_Manager.channel.shop.ca;
using Order_Manager.channel.giantTiger;

namespace Order_Manager.mainForms
{
    public partial class DetailPage : Form
    {
        // field for storing order details
        private readonly SearsValues searsValues;
        private readonly ShopCaValues shopCaValues;
        private readonly GiantTigerValues giantTigerValues;
        private readonly string CHANNEL;

        // field for brightpearl connection
        private readonly BPconnect bp = new BPconnect();

        // supporting field for keeping track cancelled items and time for loading prompt
        private Dictionary<int, string> cancelList;
        private int timeLeft = 4;   // default set to 4

        // supporting field for storing address
        private string addressOld;

        #region Constructor
        /* first constructor that show sears order */
        public DetailPage(SearsValues value)
        {
            InitializeComponent();

            searsValues = value;
            ShowResult(value);

            // set flag to sears
            CHANNEL = "Sears";
        }

        /* second constructor that show shop.ca order */
        public DetailPage(ShopCaValues value)
        {
            InitializeComponent();

            shopCaValues = value;
            ShowResult(value);

            // set flag to shop.ca
            CHANNEL = "Shop.ca";
        }

        /* third constructor that show giant tiger order */
        public DetailPage(GiantTigerValues value)
        {
            InitializeComponent();

            giantTigerValues = value;
            ShowResult(value);

            // set flag to giant tiger
            CHANNEL = "Giant Tiger";
        }
        #endregion

        /* print packing slip button clicks that print the packing slip for the order item(s) */
        private void printPackingSlipButton_Click(object sender, EventArgs e)
        {
            // get all cancel index and print the packing slip that are not cancelled
            int[] cancelIndex = GetCancelIndex();

            switch (CHANNEL)
            {
                case "Sears":
                    // ths case if the order is from sears
                    try
                    {
                        SearsPackingSlip.CreatePackingSlip(searsValues, cancelIndex, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "Shop.ca":
                    // the case if the order is from shop.ca
                    try
                    {
                        ShopCaPackingSlip.CreatePackingSlip(shopCaValues, cancelIndex, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "Giant Tiger":
                    // the case if the order is from giant tiger
                    try
                    {
                        GiantTigerPackingSlip.CreatePackingSlip(giantTigerValues, cancelIndex, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
            }
        }

        /* the event for verify button click that show the result of the address validity */
        private void verifyButton_Click(object sender, EventArgs e)
        {
            // set wait cursor
            Cursor.Current = Cursors.WaitCursor;

            bool flag = true;
            switch (CHANNEL)
            {
                case "Sears":
                    flag = AddressValidation.Validate(searsValues.ShipTo);
                    break;
                case "Shop.ca":
                    flag = AddressValidation.Validate(shopCaValues.ShipTo);
                    break;
                case "Giant Tiger":
                    flag = AddressValidation.Validate(giantTigerValues.ShipTo);
                    break;
            }

            if (flag)
            {
                verifyTextbox.Text = "Address Verified";
                verifyTextbox.ForeColor = Color.FromArgb(100, 168, 17);
            }
            else
            {
                verifyTextbox.Text = "Address Not Valid";
                verifyTextbox.ForeColor = Color.FromArgb(254, 126, 116);
            }

            verifyTextbox.Visible = true;

            // set default cursor after complete
            Cursor.Current = Cursors.Default;
        }

        #region ListView Events
        /* the event for select all checkbox check change that select all checkbox all deselect all */
        private void selectAllCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (selectAllCheckbox.Checked)
            {
                for (int i = 0; i < listview.Items.Count; i++)
                    listview.Items[i].Checked = true;
            }
            else
            {
                for (int i = 0; i < listview.Items.Count; i++)
                    listview.Items[i].Checked = false;
            }
        }

        /* the event for mark as cancel button click that mark the checked item to cancelled shipment */
        private void markCancelButton_Click(object sender, EventArgs e)
        {
            int count = 0;

            #region Listview and Buttons
            foreach (ListViewItem item in listview.Items)
            {
                if (item.Checked)
                {
                    item.SubItems[5].Text = "Cancelled";
                    count++;
                }
                else
                {
                    item.SubItems[5].Text = "";
                    item.SubItems[6].Text = "";
                }

            }

            if (count > 0)
            {
                listview.Columns[6].Text = "Reason";
                reasonCombobox.Visible = true;
                setReasonButton.Visible = true;
            }
            else
            {
                listview.Columns[6].Text = "";
                reasonCombobox.Visible = false;
                setReasonButton.Visible = false;
            }

            // check the number of the cancel items compare to those are not so that change the enability of the print button and create label button
            if (count >= listview.Items.Count)
            {
                printPackingSlipButton.Enabled = false;
                createLabelButton.Enabled = false;
            }
            else
            {
                printPackingSlipButton.Enabled = true;
                createLabelButton.Enabled = true;
            }
            #endregion

            #region Package Detail
            // initialize field for sku detail -> [0] weight, [1] length, [2] width, [3] height
            decimal[] skuDetail = { 0, 0, 0, 0 };

            // change the details of package
            switch (CHANNEL)
            {
                case "Sears":
                    for (int i = 0; i < searsValues.LineCount; i++)
                    {
                        if (listview.Items[i].SubItems[5].Text != "") continue;
                        decimal[] detailList = Package.GetSkuDetail(searsValues.TrxVendorSku[i]);

                        if (detailList == null) continue;
                        for (int j = 0; j < 4; j++)
                            skuDetail[j] += detailList[j];
                    }
                    break;
                case "Shop.ca":
                    for (int i = 0; i < shopCaValues.OrderItemId.Count; i++)
                    {
                        if (listview.Items[i].SubItems[5].Text != "") continue;
                        decimal[] detailList = Package.GetSkuDetail(shopCaValues.Sku[i]);

                        if (detailList == null) continue;
                        for (int j = 0; j < 4; j++)
                            skuDetail[j] += detailList[j];
                    }
                    break;
                case "Giant Tiger":
                    for (int i = 0; i < giantTigerValues.VendorSku.Count; i++)
                    {
                        if (listview.Items[i].SubItems[5].Text != "") continue;
                        decimal[] detailList = Package.GetSkuDetail(giantTigerValues.VendorSku[i]);

                        if (detailList == null) continue;
                        for (int j = 0; j < 4; j++)
                            skuDetail[j] += detailList[j];
                    }
                    break;
            }

            // show result to shipping info
            weightKgUpdown.Value = skuDetail[0] / 1000;
            weightLbUpdown.Value = skuDetail[0] / 453.592m;
            lengthUpdown.Value = skuDetail[1];
            widthUpdown.Value = skuDetail[2];
            heightUpdown.Value = skuDetail[3];
            #endregion
        }

        /* when clicked set the reason of cancelling to the checked items */
        private void setReasonButton_Click(object sender, EventArgs e)
        {
            if (reasonCombobox.SelectedIndex == 0) return;
            foreach (ListViewItem item in listview.CheckedItems)
                item.SubItems[6].Text = reasonCombobox.SelectedItem.ToString();
        }

        /* prevent user from changing header size */
        private void listview_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listview.Columns[e.ColumnIndex].Width;
        }
        #endregion

        #region Shipment and Package
        /* create label button clicks that create shipment and retrieve the label and tracking number */
        private void createLabelButton_Click(object sender, EventArgs e)
        {
            // ask for user confirmaiton
            ConfirmPanel confirm = new ConfirmPanel("Are you sure you want to ship the package ?");
            confirm.ShowDialog(this);

            if (confirm.DialogResult != DialogResult.OK) return;

            trackingNumberTextbox.Text = "Shipping";
            shipmentConfirmButton.Enabled = false;
            createLabelButton.Enabled = false;

            // start timer
            timerShip.Start();

            // initialize field for shipment package
            switch (CHANNEL)
            {
                case "Sears":
                    searsValues.Package = new Package(weightKgUpdown.Value, lengthUpdown.Value, widthUpdown.Value, heightUpdown.Value, serviceCombobox.SelectedItem.ToString(), serviceCombobox.SelectedItem.ToString(), null, null, null, null);
                    break;
                case "Shop.ca":
                    shopCaValues.Package = new Package(weightKgUpdown.Value, lengthUpdown.Value, widthUpdown.Value, heightUpdown.Value, serviceCombobox.SelectedItem.ToString(), serviceCombobox.SelectedItem.ToString(), null, null, null, null);
                    break;
                case "Giant Tiger":
                    giantTigerValues.Package = new Package(weightKgUpdown.Value, lengthUpdown.Value, widthUpdown.Value, heightUpdown.Value, serviceCombobox.SelectedItem.ToString(), serviceCombobox.SelectedItem.ToString(), null, null, null, null);
                    break;
            }

            if (!backgroundWorkerShip.IsBusy)
                backgroundWorkerShip.RunWorkerAsync();
        }
        private void backgroundWorkerShip_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            switch (CHANNEL)
            {
                case "Sears":
                    #region UPS
                    // initialize field for shipment
                    Ups ups = new Ups();

                    // post shipment confirm and get the digest string from response
                    string[] digest = ups.PostShipmentConfirm(searsValues);

                    // error checking
                    if (ups.Error)
                    {
                        MessageBox.Show(ups.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Result = true;
                        return;
                    }

                    // post shipment accept and get tracking number and image 
                    string[] acceptResult = ups.PostShipmentAccept(digest[1]);

                    // error checking
                    if (ups.Error)
                    {
                        MessageBox.Show(ups.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Result = true;
                        return;
                    }

                    // retrieve identification number and tracking number
                    searsValues.Package.IdentificationNumber = digest[0];
                    searsValues.Package.TrackingNumber = acceptResult[0];

                    // update database set the order's tracking number and identification number
                    new Sears().PostShip(searsValues.Package.TrackingNumber, searsValues.Package.IdentificationNumber, searsValues.TransactionId);

                    // get the shipment label and show it
                    ups.ExportLabel(acceptResult[1], searsValues.TransactionId, true);
                    break;
                    #endregion
                case "Shop.ca":
                    #region Canada Post
                    {
                        // initialize field for shipment
                        CanadaPost canadaPost = new CanadaPost();

                        // create shipment for canada post
                        string[] result = canadaPost.CreateShipment(shopCaValues.ShipTo, shopCaValues.Package);

                        // error checking
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Result = true;
                            return;
                        }

                        // retrieve tracking number, refund link and label link
                        shopCaValues.Package.TrackingNumber = result[0];
                        shopCaValues.Package.SelfLink = result[1];
                        shopCaValues.Package.LabelLink = result[2];

                        // update database set the order's tracking number refund link and label link
                        new ShopCa().PostShip(shopCaValues.Package.TrackingNumber, shopCaValues.Package.SelfLink, shopCaValues.Package.LabelLink, shopCaValues.OrderId);

                        // get artifect
                        Thread.Sleep(5000);
                        byte[] artifect = canadaPost.GetArtifact(result[2]);

                        // error checking
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Result = true;
                            return;
                        }

                        // get the shipment label and show it
                        canadaPost.ExportLabel(artifect, shopCaValues.OrderId, true, true);
                    }
                    break;
                #endregion
                case "Giant Tiger":
                    #region Canada Post
                    {
                        // initialize field for shipment
                        CanadaPost canadaPost = new CanadaPost();

                        // create shipment for canada post
                        string[] result = canadaPost.CreateShipment(giantTigerValues.ShipTo, giantTigerValues.Package);

                        // error checking
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Result = true;
                            return;
                        }

                        // retrieve tracking number, refund link and label link
                        giantTigerValues.Package.TrackingNumber = result[0];
                        giantTigerValues.Package.SelfLink = result[1];
                        giantTigerValues.Package.LabelLink = result[2];

                        // update database set the order's tracking number refund link and label link
                        new GiantTiger().PostShip(giantTigerValues.Package.TrackingNumber, giantTigerValues.Package.SelfLink, giantTigerValues.Package.LabelLink, giantTigerValues.PoNumber);

                        // get artifect
                        Thread.Sleep(5000);
                        byte[] artifect = canadaPost.GetArtifact(result[2]);

                        // error checking
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Result = true;
                            return;
                        }

                        // get the shipment label and show it
                        canadaPost.ExportLabel(artifect, giantTigerValues.PoNumber, true, true);
                    }
                    break;
                    #endregion
            }

            // set bool flag to false
            e.Result = false;
        }

        private void backgroundWorkerShip_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // stop the timer and show the tracking number
            timerShip.Stop();
            switch (CHANNEL)
            {
                case "Sears":
                    trackingNumberTextbox.Text = searsValues.Package.TrackingNumber;
                    break;
                case "Shop.ca":
                    trackingNumberTextbox.Text = shopCaValues.Package.TrackingNumber;
                    break;
                case "Giant Tiger":
                    trackingNumberTextbox.Text = giantTigerValues.Package.TrackingNumber;
                    break;
            }

            // if error occur enable button else diable it and show shipment cancel button
            if ((bool) e.Result)
            {
                trackingNumberTextbox.Text = "Error";
                createLabelButton.Enabled = true;
            }
            else
                voidShipmentButton.Visible = true;

            shipmentConfirmButton.Enabled = true;
        }

        /* shipment loading promopt */
        private void timerShip_Tick(object sender, EventArgs e)
        {
            timeLeft--;

            if (timeLeft <= 0)
            {
                trackingNumberTextbox.Text = "Shipping";
                timeLeft = 4;
            }
            else
                trackingNumberTextbox.Text += '.';
        }
        #endregion

        /* void shipment button that void the current shipment for the order */
        private void voidShipmentButton_Click(object sender, EventArgs e)
        {
            switch (CHANNEL)
            {
                case "Sears":
                    #region UPS
                    // post void shipment request and get the response
                    Ups ups = new Ups();
                    ups.PostShipmentVoid(searsValues.Package.IdentificationNumber);

                    // the case if is bad request
                    if (ups.Error)
                    {
                        MessageBox.Show(ups.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // mark transaction as not shipped
                    new Sears().PostVoid(new[] { searsValues.TransactionId });

                    // set tracking and identification to nothing
                    searsValues.Package.IdentificationNumber = "";
                    searsValues.Package.TrackingNumber = "";

                    break;
                    #endregion
                case "Shop.ca":
                    #region Canada Post
                    {
                        // post void shipment request and get the response
                        CanadaPost canadaPost = new CanadaPost();
                        canadaPost.DeleteShipment(shopCaValues.Package.SelfLink);

                        // the cas if is bad request
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // mar order as not shipped
                        new ShopCa().PostVoid(new[] { shopCaValues.OrderId });

                        // set tracking, self link, label link to nothing
                        shopCaValues.Package.TrackingNumber = "";
                        shopCaValues.Package.SelfLink = "";
                        shopCaValues.Package.LabelLink = "";
                    }
                    break;
                #endregion
                case "Giant Tiger":
                    #region Canada Post
                    {
                        // post void shipment request and get the response
                        CanadaPost canadaPost = new CanadaPost();
                        canadaPost.DeleteShipment(giantTigerValues.Package.SelfLink);

                        // the cas if is bad request
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // mar order as not shipped
                        new GiantTiger().PostVoid(new[] { giantTigerValues.PoNumber });

                        // set tracking, self link, label link to nothing
                        giantTigerValues.Package.TrackingNumber = "";
                        giantTigerValues.Package.SelfLink = "";
                        giantTigerValues.Package.LabelLink = "";
                    }
                    break;
                    #endregion
            }

            // mark cancel as invisible, set tracking number text to not shipped, and enable create label button
            voidShipmentButton.Visible = false;
            trackingNumberTextbox.Text = "Not Shipped";
            createLabelButton.Enabled = true;        
        }

        #region Shipment Confirm
        /* shipment confirm button clicks that send the confirm xml to sears */
        private void shipmentConfirmButton_Click(object sender, EventArgs e)
        {
            // get user's confirmation
            ConfirmPanel confirm = new ConfirmPanel("Are you sure you want to ship this order ?");
            confirm.ShowDialog(this);

            // get user confirmation
            if (confirm.DialogResult != DialogResult.OK) return;

            // start timer
            timerConfirm.Start();

            // generate cancel list
            cancelList = new Dictionary<int, string>();
            for (int i = 0; i < listview.Items.Count; i++)
            {
                if (listview.Items[i].SubItems[5].Text != "Cancelled") continue;
                string reason = listview.Items[i].SubItems[6].Text;

                // the case if the user has not provide the reason for cancelling a item
                if (reason == "")
                {
                    MessageBox.Show("Please provide the reason of cancellation", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cancelList.Add(i, reason);
            }

            // error check for non shipped items
            if (CHANNEL == "Sears" && cancelList.Count < searsValues.LineCount && searsValues.Package.TrackingNumber == "")
            {
                MessageBox.Show("There are items that are not shipped", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (CHANNEL == "Shop.ca" && cancelList.Count < shopCaValues.OrderItemId.Count && shopCaValues.Package.TrackingNumber == "")
            {
                MessageBox.Show("There are items that are not shipped", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (CHANNEL == "Giant Tiger" && cancelList.Count < giantTigerValues.VendorSku.Count && giantTigerValues.Package.TrackingNumber == "")
            {
                MessageBox.Show("There are items that are not shipped", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            progressbar.Visible = true;

            // call background worker
            if (!backgroundWorkerConfirm.IsBusy)
                backgroundWorkerConfirm.RunWorkerAsync();
        }
        private void backgroundWorkerConfirm_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Simulate(1, 40);

            switch (CHANNEL)
            {
                case "Sears":
                    // sears case
                    // export xml file
                    new Sears().GenerateXml(searsValues, cancelList);

                    Simulate(40, 70);

                    // post order to brightpearl
                    bp.PostOrder(searsValues, new List<int>(cancelList.Keys).ToArray());

                    break;
                case "Shop.ca":
                    // shop.ca case
                    // export txt file
                    new ShopCa().GenerateTxt(shopCaValues, cancelList);

                    Simulate(40, 70);

                    // post order to brightpearl
                    bp.PostOrder(shopCaValues, new List<int>(cancelList.Keys).ToArray());

                    break;
                case "Giant Tiger":
                    // giant tiger case
                    // export csv file
                    new GiantTiger().GenerateCsv(giantTigerValues, cancelList);

                    Simulate(40, 70);

                    // post order to brightpearl
                    bp.PostOrder(giantTigerValues, new List<int>(cancelList.Keys).ToArray());

                    break;
            }

            Simulate(70, 100);
        }
        private void backgroundWorkerConfirm_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressbar.Value = e.ProgressPercentage;
        }
        private void backgroundWorkerConfirm_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // final work
            progressbar.Visible = false;
            shipmentConfirmButton.Text = "Completed";
            shipmentConfirmButton.BackColor = Color.Transparent;
            shipmentConfirmButton.Enabled = false;
            timerConfirm.Stop();
        }

        /* shipment confirm promopt */
        private void timerConfirm_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = bp.Status;
        }
        #endregion

        #region Supporting Region
        /* a method that show the information of the given SearsValues object */
        private void ShowResult(SearsValues value)
        {
            // title bar set up
            logoPicturebox.Image = Image.FromFile(@"..\..\image\sears.png");
            topOrderNumberTextbox.Text = value.TransactionId;

            #region Order Summary
            // date
            orderDateTextbox.Text = value.CustOrderDate.ToString("MM/dd/yyyy");
            paidDateTextbox.Text = value.CustOrderDate.ToString("MM/dd/yyyy");
            shipByDateTextbox.Text = value.ExpectedShipDate[0].ToString("MM/dd/yyyy");

            // unit price
            double price = value.UnitPrice.Sum();
            unitPriceTotalTextbox.Text = price.ToString(CultureInfo.InvariantCulture);

            // GST and HST
            price = value.GstHstExtended.Sum() + value.GstHstTotal.Sum();
            gsthstTextbox.Text = price.ToString(CultureInfo.InvariantCulture);

            // PST
            price = value.PstExtended.Sum() + value.PstTotal.Sum();
            pstTextbox.Text = price.ToString(CultureInfo.InvariantCulture);

            // other fee
            price = value.LineHandling.Sum();
            otherFeeTextbox.Text = price.ToString(CultureInfo.InvariantCulture);

            // total 
            totalOrderTextbox.Text = value.TrxBalanceDue.ToString(CultureInfo.InvariantCulture);
            #endregion

            #region Buyer / Recipient Info
            // sold to 
            soldToTextbox.Text = value.Recipient.Name;
            soldToPhoneTextbox.Text = value.Recipient.DayPhone;

            // ship to
            shipToNameTextbox.Text = value.ShipTo.Name;
            shipToAddress1Textbox.Text = value.ShipTo.Address1;
            shipToAddress2Textbox.Text = value.ShipTo.Address2;
            shipToCombineTextbox.Text = value.ShipTo.City + ", " + value.ShipTo.State + ", " + value.ShipTo.PostalCode;
            shipToPhoneTextbox.Text = value.ShipTo.DayPhone;
            #endregion

            #region Listview and Shipping Info
            // adding items to service combobox
            serviceCombobox.Items.Clear();
            serviceCombobox.Items.Add("UPS Standard");
            serviceCombobox.Items.Add("UPS Express");
            serviceCombobox.Items.Add("UPS 3 Day Select");
            serviceCombobox.Items.Add("UPS Worldwide Express");
            serviceCombobox.SelectedIndex = 0;

            // adding items to reason combobox
            reasonCombobox.Items.Clear();
            reasonCombobox.Items.Add("Select Reason");
            reasonCombobox.Items.Add("Incorrect Ship To Address");
            reasonCombobox.Items.Add("Incorrect SKU");
            reasonCombobox.Items.Add("Cancelled at Merchant's Request");
            reasonCombobox.Items.Add("Cannot fulfill the order in time");
            reasonCombobox.Items.Add("Cannot Ship as Ordered");
            reasonCombobox.Items.Add("Invalid Item Cost");
            reasonCombobox.Items.Add("Merchant detected fraud");
            reasonCombobox.Items.Add("Order missing information");
            reasonCombobox.Items.Add("Out of Stock");
            reasonCombobox.Items.Add("Product Has Been Discontinued");
            reasonCombobox.Items.Add("Other");
            reasonCombobox.SelectedIndex = 0;

            // ups details
            switch (value.ServiceLevel)
            {
                case "UPSN_3D":
                    serviceCombobox.SelectedIndex = 2;
                    break;
                case "UPSN_IX":
                    serviceCombobox.SelectedIndex = 3;
                    break;
                case "UPS":
                    serviceCombobox.SelectedIndex = 1;
                    break;
                default:
                    serviceCombobox.SelectedIndex = 0;
                    break;
            }

            // initialize field for sku detail -> [0] weight, [1] length, [2] width, [3] height
            decimal[] skuDetail = { 0, 0, 0, 0 };

            // adding list to listview and getting sku detail
            for (int i = 0; i < value.LineCount; i++)
            {
                // add item to list
                ListViewItem item = new ListViewItem(value.MerchantLineNumber[i].ToString());

                item.SubItems.Add(value.Description[i] + "  SKU: " + value.TrxVendorSku[i]);
                item.SubItems.Add("$ " + value.UnitPrice[i]);
                item.SubItems.Add(value.TrxQty[i].ToString());
                item.SubItems.Add("$ " + value.LineBalanceDue[i]);
                item.SubItems.Add("");
                item.SubItems.Add("");

                listview.Items.Add(item);

                // generate sku detail
                decimal[] detailList = Package.GetSkuDetail(value.TrxVendorSku[i]);

                // the case if bad sku
                if (detailList == null)
                    item.BackColor = Color.FromArgb(254, 126, 116);
                else
                {
                    for (int j = 0; j < 4; j++)
                        skuDetail[j] += detailList[j];
                }
            }

            // show result to shipping info
            weightKgUpdown.Value = skuDetail[0] / 1000;
            weightLbUpdown.Value = skuDetail[0] / 453.592m;
            lengthUpdown.Value = skuDetail[1];
            widthUpdown.Value = skuDetail[2];
            heightUpdown.Value = skuDetail[3];

            // shipment status -> the case if the order has already shipped
            if (value.Package.TrackingNumber == "") return;

            createLabelButton.Enabled = false;
            trackingNumberTextbox.Text = value.Package.TrackingNumber;
            voidShipmentButton.Visible = true;
            #endregion
        }

        /* a method that show the information of the given ShopCaValues object */
        private void ShowResult(ShopCaValues value)
        {
            // title bar set up
            logoPicturebox.Image = Image.FromFile(@"..\..\image\shopca.png");
            topOrderNumberTextbox.Text = value.OrderId;

            #region Order Summary
            // date
            orderDateTextbox.Text = value.OrderCreateDate.ToString("MM/dd/yyyy");
            paidDateTextbox.Text = value.OrderCreateDate.ToString("MM/dd/yyyy");
            shipByDateTextbox.Text = DateTime.Today.ToString("MM/dd/yyyy");

            // unit price
            unitPriceTotalTextbox.Text = value.TotalPrice.ToString(CultureInfo.InvariantCulture);

            // GST and HST
            gsthstTextbox.Text = value.TotalTax.ToString(CultureInfo.InvariantCulture);

            // PST
            pstTextbox.Text = "0.00";

            // other fee
            otherFeeTextbox.Text = (value.ItemShippingCost.Sum() - value.ItemDiscount.Sum()).ToString(CultureInfo.InvariantCulture);

            // total 
            totalOrderTextbox.Text = value.GrandTotal.ToString(CultureInfo.InvariantCulture);
            #endregion

            #region Buyer / Recipient Info
            // sold to 
            soldToTextbox.Text = value.BillTo.Name;
            soldToPhoneTextbox.Text = value.BillTo.DayPhone;

            // ship to
            shipToNameTextbox.Text = value.ShipTo.Name;
            shipToAddress1Textbox.Text = value.ShipTo.Address1;
            shipToAddress2Textbox.Text = value.ShipTo.Address2;
            shipToCombineTextbox.Text = value.ShipTo.City + ", " + value.ShipTo.State + ", " + value.ShipTo.PostalCode;
            shipToPhoneTextbox.Text = value.ShipTo.DayPhone;
            #endregion

            #region Listview and Shipping Info
            // adding items to service combobox
            serviceCombobox.Items.Clear();
            serviceCombobox.Items.Add("Expedited Parcel");
            serviceCombobox.Items.Add("Regular Parcel");
            serviceCombobox.Items.Add("Xpresspost");
            serviceCombobox.Items.Add("Priority");
            serviceCombobox.SelectedIndex = 0;

            // adding items to reason combobox
            reasonCombobox.Items.Clear();
            reasonCombobox.Items.Add("Select Reason");
            reasonCombobox.Items.Add("Carrier cannot deliver");
            reasonCombobox.Items.Add("Discontinued");
            reasonCombobox.Items.Add("Incomplete Address");
            reasonCombobox.Items.Add("Out of delivery area");
            reasonCombobox.Items.Add("Out of Stock");
            reasonCombobox.Items.Add("Other");
            reasonCombobox.SelectedIndex = 0;

            // initialize field for sku detail -> [0] weight, [1] length, [2] width, [3] height
            decimal[] skuDetail = { 0, 0, 0, 0 };

            // adding list to listview and getting sku detail
            for (int i = 0; i < value.OrderItemId.Count; i++)
            {
                // add item to list
                ListViewItem item = new ListViewItem(value.OrderItemId[i]);

                item.SubItems.Add(value.Title[i] + "  SKU: " + value.Sku[i]);
                item.SubItems.Add("$ " + value.ItemPrice[i]);
                item.SubItems.Add(value.Quantity[i].ToString());
                item.SubItems.Add("$ " + value.ExtendedItemPrice[i]);
                item.SubItems.Add("");
                item.SubItems.Add("");

                listview.Items.Add(item);

                // generate sku detail
                decimal[] detailList = Package.GetSkuDetail(value.Sku[i]);

                // the case if bad sku
                if (detailList == null)
                    item.BackColor = Color.FromArgb(254, 126, 116);
                else
                {
                    for (int j = 0; j < 4; j++)
                        skuDetail[j] += detailList[j];
                }
            }

            // show result to shipping info
            weightKgUpdown.Value = skuDetail[0] / 1000;
            weightLbUpdown.Value = skuDetail[0] / 453.592m;
            lengthUpdown.Value = skuDetail[1];
            widthUpdown.Value = skuDetail[2];
            heightUpdown.Value = skuDetail[3];

            // shipment status -> the case if the order has already shipped
            if (value.Package.TrackingNumber == "") return;

            createLabelButton.Enabled = false;
            trackingNumberTextbox.Text = value.Package.TrackingNumber;
            voidShipmentButton.Visible = true;
            #endregion
        }

        /* a method that show the information of the given GiantTigerValues object */
        private void ShowResult(GiantTigerValues value)
        {
            // title bar set up
            logoPicturebox.Image = Image.FromFile(@"..\..\image\giantTiger.png");
            topOrderNumberTextbox.Text = value.PoNumber;

            #region Order Summary
            // date
            orderDateTextbox.Text = value.OrderDate.ToString("MM/dd/yyyy");
            paidDateTextbox.Text = value.OrderDate.ToString("MM/dd/yyyy");
            shipByDateTextbox.Text = DateTime.Today.ToString("MM/dd/yyyy");

            // unit price
            unitPriceTotalTextbox.Text = value.UnitCost.Sum().ToString();

            // GST and HST
            gsthstTextbox.Text = "0.00";

            // PST
            pstTextbox.Text = "0.00";

            // other fee
            otherFeeTextbox.Text = "0.00";

            // total 
            totalOrderTextbox.Text = value.UnitCost.Sum().ToString(CultureInfo.InvariantCulture);
            #endregion

            #region Buyer / Recipient Info
            // sold to 
            soldToTextbox.Text = value.ShipTo.Name;
            soldToPhoneTextbox.Text = value.ShipTo.DayPhone;

            // ship to
            shipToNameTextbox.Text = value.ShipTo.Name;
            shipToAddress1Textbox.Text = value.ShipTo.Address1;
            shipToAddress2Textbox.Text = value.ShipTo.Address2;
            shipToCombineTextbox.Text = value.ShipTo.City + ", " + value.ShipTo.State + ", " + value.ShipTo.PostalCode;
            shipToPhoneTextbox.Text = value.ShipTo.DayPhone;
            #endregion

            #region Listview and Shipping Info
            // adding items to service combobox
            serviceCombobox.Items.Clear();
            serviceCombobox.Items.Add("Expedited Parcel");
            serviceCombobox.Items.Add("Regular Parcel");
            serviceCombobox.Items.Add("Xpresspost");
            serviceCombobox.Items.Add("Priority");
            serviceCombobox.SelectedIndex = 0;

            // adding items to reason combobox
            reasonCombobox.Items.Clear();
            reasonCombobox.Items.Add("Select Reason");
            reasonCombobox.Items.Add("Out of Stock");
            reasonCombobox.Items.Add("Customer Request");
            reasonCombobox.Items.Add("Incorrect product setup");
            reasonCombobox.SelectedIndex = 0;

            // initialize field for sku detail -> [0] weight, [1] length, [2] width, [3] height
            decimal[] skuDetail = { 0, 0, 0, 0 };

            // adding list to listview and getting sku detail
            for (int i = 0; i < value.VendorSku.Count; i++)
            {
                // add item to list
                ListViewItem item = new ListViewItem(value.ClientItemId[i]);

                item.SubItems.Add("Host SKU: " + value.HostSku[i] + "  - Ashlin SKU: " + value.VendorSku[i]);
                item.SubItems.Add("$ " + value.UnitCost[i]);
                item.SubItems.Add(value.Quantity[i].ToString());
                item.SubItems.Add("$ " + value.UnitCost[i]);
                item.SubItems.Add("");
                item.SubItems.Add("");

                listview.Items.Add(item);

                // generate sku detail
                decimal[] detailList = Package.GetSkuDetail(value.VendorSku[i]);

                // the case if bad sku
                if (detailList == null)
                    item.BackColor = Color.FromArgb(254, 126, 116);
                else
                {
                    for (int j = 0; j < 4; j++)
                        skuDetail[j] += detailList[j];
                }
            }

            // show result to shipping info
            weightKgUpdown.Value = skuDetail[0] / 1000;
            weightLbUpdown.Value = skuDetail[0] / 453.592m;
            lengthUpdown.Value = skuDetail[1];
            widthUpdown.Value = skuDetail[2];
            heightUpdown.Value = skuDetail[3];

            // shipment status -> the case if the order has already shipped
            if (value.Package.TrackingNumber == "") return;

            createLabelButton.Enabled = false;
            trackingNumberTextbox.Text = value.Package.TrackingNumber;
            voidShipmentButton.Visible = true;
            #endregion
        }

        /* a method that get the current cancel items' idexes */
        private int[] GetCancelIndex()
        {
            return (from ListViewItem item in listview.Items where item.SubItems[5].Text == "Cancelled" select item.Index).ToArray();
        }

        /* a method that report to progress bar value from the start to end */
        private void Simulate(int start, int end)
        {
            // simulate progress 1% ~ 30%
            for (int i = start; i <= end; i++)
            {
                Thread.Sleep(30);
                backgroundWorkerConfirm.ReportProgress(i);
            }
        }
        #endregion

        #region ShipToCombineTextbox Events
        /* key press event for ship to combine textbox that does not allow comma character */
        private void shipToCombineTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (ch != ',')
                e.Handled = false;
        }

        /* got focus event for ship to combine textbox that store old address value */
        private void shipToCombineTextbox_Enter(object sender, EventArgs e)
        {
            addressOld = shipToCombineTextbox.Text;
        }

        /* text change event for ship to combine textbox that check if the user has delete any comma */
        private void shipToCombineTextbox_TextChanged(object sender, EventArgs e)
        {
            if (addressOld == null) return;

            // find number of comma in old address text
            int[] count = { 0, 0 };
            foreach (char c in addressOld.Where(c => c == ','))
                count[0]++;

            // find number of comma in new address text
            foreach (char c in shipToCombineTextbox.Text.Where(c => c == ','))
                count[1]++;

            if (count[0] != count[1])
                shipToCombineTextbox.Text = addressOld;
            else
            {
                string addressNew = shipToCombineTextbox.Text;
                switch (CHANNEL)
                {
                    case "Sears":
                        searsValues.ShipTo.City = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        searsValues.ShipTo.State = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        searsValues.ShipTo.PostalCode = addressNew.Substring(0);
                        break;
                    case "Shop.ca":
                        shopCaValues.ShipTo.City = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        shopCaValues.ShipTo.State = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        shopCaValues.ShipTo.PostalCode = addressNew.Substring(0);
                        break;
                    case "Giant Tiger":
                        giantTigerValues.ShipTo.City = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        giantTigerValues.ShipTo.State = addressNew.Substring(0, addressNew.IndexOf(','));
                        addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                        giantTigerValues.ShipTo.PostalCode = addressNew.Substring(0);
                        break;
                }
            }
        }
        #endregion

        #region Numeric Up Down
        /* weight kg numeric updown value change that change the value of pound numeric updown as well */
        private void weightKgUpdown_ValueChanged(object sender, EventArgs e)
        {
            weightLbUpdown.Value = weightKgUpdown.Value * 2.20462m;
        }

        /* weight lb numeric updown value change that change the value of kilgram numeric updown as well */
        private void weightLbUpdown_ValueChanged(object sender, EventArgs e)
        {
            weightKgUpdown.Value = weightLbUpdown.Value / 2.20462m;
        }
        #endregion
    }
}
