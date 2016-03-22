using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CommerceHub_OrderManager.channel.brightpearl;
using CommerceHub_OrderManager.channel.sears;
using CommerceHub_OrderManager.supportingClasses;

namespace CommerceHub_OrderManager.mainForms
{
    public partial class DetailPage : Form
    {
        // field for storing order details
        private readonly SearsValues value;

        // field for brightpearl connection
        private readonly BPconnect bp = new BPconnect();

        // supporting field for keeping track cancelled items and time for loading prompt
        private Dictionary<int, string> cancelList;
        private int timeLeft = 4;   // default set to 4

        // supporting field for storing address
        private string addressOld;

        /* constructor that initializes graphic compents and order fields */
        public DetailPage(SearsValues value)
        {
            InitializeComponent();

            this.value = value;
            showResult(value);
        }

        /* print packing slip button clicks that print the packing slip for the order item(s) */
        private void printPackingSlipButton_Click(object sender, EventArgs e)
        {
            // get all cancel index and print the packing slip that are not cancelled
            int[] cancelIndex = getCancelIndex();
            SearsPackingSlip packingSlip = new SearsPackingSlip();
            packingSlip.createPackingSlip(value, cancelIndex, true);
            if (packingSlip.Error)
                MessageBox.Show("Error occurs during exporting packing slip:\nPlease check that the file is not opened during exporting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /* the event for verify button click that show the result of the address validity */
        private void verifyButton_Click(object sender, EventArgs e)
        {
            bool flag = new AddressValidation().validate(value.ShipTo);

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
            for (int i = 0; i < value.LineCount; i++)
            {
                if (listview.Items[i].SubItems[5].Text != "") continue;
                decimal[] detailList = getSkuDetail(value.TrxVendorSKU[i]);

                if (detailList != null)
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

            if (confirm.DialogResult == DialogResult.OK)
            {
                trackingNumberTextbox.Text = "Shipping";
                shipmentConfirmButton.Enabled = false;

                // start timer
                timerShip.Start();

                // initialize field for shipment package
                value.Package = new Package(weightKgUpdown.Value, lengthUpdown.Value, widthUpdown.Value, heightUpdown.Value, serviceCombobox.SelectedItem.ToString(), serviceCombobox.SelectedItem.ToString(), null, null);

                if (!backgroundWorkerShip.IsBusy)
                    backgroundWorkerShip.RunWorkerAsync();
            }
        }
        private void backgroundWorkerShip_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // initialize field for shipment
            UPS ups = new UPS();

            // post shipment confirm and get the digest string from response
            string[] digest = ups.postShipmentConfirm(value, value.Package);

            // error checking
            if (digest == null)
            {
                MessageBox.Show("Error occur while requesting shipment, please try again.", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Result = true;
                return;
            }
            if (digest[0].Contains("Error:"))
            {
                MessageBox.Show(digest[0], "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Result = true;
                return;
            }

            // post shipment accept and get tracking number and image 
            string[] acceptResult = ups.postShipmentAccept(digest[1]);

            // error checking
            if (acceptResult == null)
            {
                MessageBox.Show("Error occur while requesting shipment, please try again.", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Result = true;
                return;
            }

            // retrieve identification number and tracking number
            value.Package.IdentificationNumber = digest[0];
            value.Package.TrackingNumber = acceptResult[0];

            // update database set the order's tracking number and identification number
            new Sears().PostShip(value.Package.TrackingNumber, value.Package.IdentificationNumber, value.TransactionID);

            // get the shipment label and show it
            ups.exportLabel(acceptResult[1], value.TransactionID, true);

            // set bool flag to false
            e.Result = false;
        }
        private void backgroundWorkerShip_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // stop the timer and show the tracking number
            timerShip.Stop();
            trackingNumberTextbox.Text = value.Package.TrackingNumber;

            // if error occur enable button else diable it and show shipment cancel button
            if ((bool) e.Result)
                createLabelButton.Enabled = true;
            else
            {
                createLabelButton.Enabled = false;
                voidShipmentButton.Visible = true;
            }

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
                timerShip.Start();
            }
            else
                trackingNumberTextbox.Text += ".";
        }
        #endregion

        /* void shipment button that void the current shipment for the order */
        private void voidShipmentButton_Click(object sender, EventArgs e)
        {
            // post void shipment request and get the response
            string voidResult = new UPS().postShipmentVoid(value.Package.IdentificationNumber);

            // the case if is bad request
            if (voidResult.Contains("Error:"))
            {
                MessageBox.Show(voidResult, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // mark transaction as not shipped
            new Sears().PostVoid(new[] { value.TransactionID });

            // mark cancel as invisible, set tracking number text to not shipped, and enable create label button
            voidShipmentButton.Visible = false;
            trackingNumberTextbox.Text = "Not Shipped";
            createLabelButton.Enabled = true;

            // set tracking and identification to nothing
            value.Package.IdentificationNumber = "";
            value.Package.TrackingNumber = "";
        }

        #region Shipment Confirm
        /* shipment confirm button clicks that send the confirm xml to sears */
        private void shipmentConfirmButton_Click(object sender, EventArgs e)
        {
            // get user's confirmation
            ConfirmPanel confirm = new ConfirmPanel("Are you sure you want to ship this order ?");
            confirm.ShowDialog(this);

            if (confirm.DialogResult == DialogResult.OK)
            {
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

                progressbar.Visible = true;

                // call background worker
                if (!backgroundWorkerConfirm.IsBusy)
                    backgroundWorkerConfirm.RunWorkerAsync();
            }
        }
        private void backgroundWorkerConfirm_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            simulate(1, 40);

            // export xml file
            new Sears().GenerateXML(value, cancelList);

            simulate(40, 70);

            // post order to brightpearl
            bp.postOrder(value, new List<int>(cancelList.Keys).ToArray());

            simulate(70, 100);
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
        private void showResult(SearsValues value)
        {
            topOrderNumberTextbox.Text = value.TransactionID;

            #region Order Summary
            // date
            orderDateTextbox.Text = value.CustOrderDate.ToString("MM/dd/yyyy");
            paidDateTextbox.Text = value.CustOrderDate.ToString("MM/dd/yyyy");
            shipByDateTextbox.Text = value.ExpectedShipDate[0].ToString("MM/dd/yyyy");

            // unit price
            double price = value.UnitPrice.Sum();
            unitPriceTotalTextbox.Text = price.ToString();

            // GST and HST
            price = value.GST_HST_Extended.Sum() + value.GST_HST_Total.Sum();
            gsthstTextbox.Text = price.ToString();

            // PST
            price = value.PST_Extended.Sum() + value.PST_Total.Sum();
            pstTextbox.Text = price.ToString();

            // other fee
            price = value.LineHandling.Sum();
            otherFeeTextbox.Text = price.ToString();

            // total 
            totalOrderTextbox.Text = value.TrxBalanceDue.ToString();
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

                item.SubItems.Add(value.Description[i] + "  SKU: " + value.TrxVendorSKU[i]);
                item.SubItems.Add("$ " + value.UnitPrice[i]);
                item.SubItems.Add(value.TrxQty[i].ToString());
                item.SubItems.Add("$ " + value.LineBalanceDue[i]);
                item.SubItems.Add("");
                item.SubItems.Add("");

                listview.Items.Add(item);

                // generate sku detail
                decimal[] detailList = getSkuDetail(value.TrxVendorSKU[i]);

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
            #endregion
        }

        /* a method that get the detail of the given sku */
        private static decimal[] getSkuDetail(string sku)
        {
            // local supporting fields
            decimal[] list = new decimal[4];

            // [0] weight, [1] length, [2] width, [3] height
            using (SqlConnection conneciton = new SqlConnection(Properties.Settings.Default.Designcs))
            {
                SqlCommand command = new SqlCommand("SELECT Weight_grams, Shippable_Depth_cm, Shippable_Width_cm, Shippable_Height_cm " +
                                                    "FROM master_Design_Attributes design JOIN master_SKU_Attributes sku ON (design.Design_Service_Code = sku.Design_Service_Code) " + 
                                                    "WHERE SKU_Ashlin = \'" + sku + "\';", conneciton);
                conneciton.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // check if there is result
                if (!reader.HasRows)
                    return null;

                for (int i = 0; i < 4; i++)
                    list[i] = Convert.ToDecimal(reader.GetValue(i));
            }

            return list;
        }

        /* a method that get the current cancel items' idexes */
        private int[] getCancelIndex()
        {
            return (from ListViewItem item in listview.Items where item.SubItems[5].Text == "Cancelled" select item.Index).ToArray();
        }

        /* a method that report to progress bar value from the start to end */
        private void simulate(int start, int end)
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
                value.ShipTo.City = addressNew.Substring(0, addressNew.IndexOf(','));
                addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                value.ShipTo.State = addressNew.Substring(0, addressNew.IndexOf(','));
                addressNew = addressNew.Substring(addressNew.IndexOf(',') + 1);
                value.ShipTo.PostalCode = addressNew.Substring(0);
            }
        }
        #endregion
    }
}
