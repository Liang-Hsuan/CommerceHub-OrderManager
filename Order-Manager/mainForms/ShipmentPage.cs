using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Order_Manager.channel.sears;
using Order_Manager.channel.shop.ca;
using Order_Manager.supportingClasses.Shipment;
using Order_Manager.supportingClasses;
using System.Threading;
using System.Globalization;

namespace Order_Manager.mainForms
{
    /*
     * An application module that can manage the shipped order
     */
    public partial class ShipmentPage : Form
    {
        // field for online shopping channels' order
        private readonly Sears sears = new Sears();
        private readonly  ShopCa shopCa = new ShopCa();

        // field for carrier connection
        private readonly Ups ups = new Ups();
        private readonly CanadaPost canadaPost = new CanadaPost();

        // field for storing data
        private struct Order
        {
            public string Source;
            public string TransactionId;
            public string ShipmentIdentificationNumber;
        }

        /* constructor that initialize graphic components and data in listview */
        public ShipmentPage()
        {
            InitializeComponent();

            ShowResult();
        }

        /* a method that show today's shipped order to the listview */
        private void ShowResult()
        {
            // first clear the listview
            listview.Items.Clear();

            #region Sears
            // get shipped items from sears
            SearsValues[] searsValue = sears.GetAllShippedOrder();

            // show shipped item to list view
            foreach (SearsValues value in searsValue)
            {
                ListViewItem item = new ListViewItem("Sears");

                item.SubItems.Add(value.TransactionId);
                item.SubItems.Add(value.Package.TrackingNumber);
                item.SubItems.Add(value.Package.IdentificationNumber);

                listview.Items.Add(item);
            }
            #endregion

            #region Shop.ca
            // get shipped items from shop.ca
            ShopCaValues[] shopCaValue = shopCa.GetAllShippedOrder();

            // shor shipped item to list view 
            foreach (ShopCaValues value in shopCaValue)
            {
                ListViewItem item = new ListViewItem("Shop.ca");

                item.SubItems.Add(value.OrderId);
                item.SubItems.Add(value.Package.TrackingNumber);
                item.SubItems.Add(value.Package.SelfLink);

                listview.Items.Add(item);
            }
            #endregion
        }

        #region Top Buttons
        /* end of day event */
        private void endOfDayButton_Click(object sender, EventArgs e)
        {
            // check all the items that can be end of day
            foreach (ListViewItem item in listview.Items)
            {
                if (item.SubItems[0].Text == "Shop.ca")
                    item.Checked = true;
            }

            // get confirmation from the user
            ConfirmPanel confirm = new ConfirmPanel("Are you sure you want to do the end of day for Canada Post?");
            confirm.ShowDialog(this);

            // the case if user not conifrm or there is no shipment to end of day -> return
            if (confirm.DialogResult != DialogResult.OK || listview.CheckedItems.Count < 1) return;

            #region Real Work
            // set end of day button to disabled and cursor to wait state
            endOfDayButton.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            // get all the manifest links
            string groupId = DateTime.Today.ToString("yyyyMMdd");
            string[] list = canadaPost.TransmitShipments(groupId);

            // error check 1
            if (canadaPost.Error)
            {
                MessageBox.Show(canadaPost.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                endOfDayButton.Enabled = true;
                return;
            }

            Thread.Sleep(5000);

            // confirm each manifest and get artifact link
            List<string> manifestList = new List<string>();
            foreach (string manifest in list)
            {
                manifestList.Add(canadaPost.GetManifest(manifest));

                // error check 2
                if (!canadaPost.Error) continue;

                MessageBox.Show(canadaPost.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                endOfDayButton.Enabled = true;
                return;
            }

            Thread.Sleep(5000);

            // get artifact and save as pdf
            for (int i = 0; i < manifestList.Count; i++)
            {
                byte[] binary = canadaPost.GetArtifact(manifestList[i]);
                canadaPost.ExportLabel(binary, groupId + '_' + (i + 1), false, false);
            }

            // set end of day to true in database
            shopCa.PostShip(true, DateTime.ParseExact(groupId, "yyyyMMdd", CultureInfo.InvariantCulture));

            // show the new result
            MessageBox.Show("Manifests have been successfully exported to\n" + canadaPost.SavePathManifestShopCa, "Congratulation");
            ShowResult();

            // set end of day button to enabled and cursor to default state
            endOfDayButton.Enabled = true;
            Cursor.Current = Cursors.Default;
            #endregion
        }

        /* cancel shipment button click that mark the selected order to cancelled in database */
        private void cancelShipmentButton_Click(object sender, EventArgs e)
        {
            // error check
            if (listview.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select the shipment you want to cancel", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // local fields for storing data -> [0] sears, [1] shop.ca
            List<string>[] list = { new List<string>(), new List<string>() };
            bool[] channel = { false, false };

            // adding cancel order list and post shipment void
            List<Order> cancelList = (from ListViewItem item in listview.CheckedItems
                select new Order
                {
                    Source = item.SubItems[0].Text,
                    TransactionId = item.SubItems[1].Text,
                    ShipmentIdentificationNumber = item.SubItems[3].Text
                }).ToList();

            // cancellation to carriers
            foreach (Order cancelledOrder in cancelList)
            {
                switch (cancelledOrder.Source)
                {
                    case "Sears":
                        list[0].Add(cancelledOrder.TransactionId);
                        ups.PostShipmentVoid(cancelledOrder.ShipmentIdentificationNumber);
                        if (ups.Error)
                        {
                            MessageBox.Show(ups.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        channel[0] = true;
                        break;
                    case "Shop.ca":
                        list[1].Add(cancelledOrder.TransactionId);
                        canadaPost.DeleteShipment(cancelledOrder.ShipmentIdentificationNumber);
                        if (canadaPost.Error)
                        {
                            MessageBox.Show(canadaPost.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        channel[1] = true;
                        break;
                }
            }

            // cancellation to database
            if (channel[0])
                sears.PostVoid(list[0].ToArray());
            if (channel[1])
                shopCa.PostVoid(list[1].ToArray());

            // show new result
            ShowResult();
        }

        /* beck botton clicks that clost the form */
        private void backButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region List View Events
        /* select all checkbox event that can check and uncheck all the items in list view */
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

        /* prevent user from changing header size */
        private void listview_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listview.Columns[e.ColumnIndex].Width;
        }
        #endregion
    }
}
