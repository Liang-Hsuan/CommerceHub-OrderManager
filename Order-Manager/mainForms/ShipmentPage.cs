using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Order_Manager.channel.sears;
using Order_Manager.channel.shop.ca;
using Order_Manager.supportingClasses.Shipment;

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

        // field for UPS connection
        private readonly UPS ups = new UPS();

        // field for storing data
        private struct Order
        {
            public string source;
            public string transactionId;
            public string shipmentIdentificationNumber;
        }

        /* constructor that initialize graphic components and data in listview */
        public ShipmentPage()
        {
            InitializeComponent();

            showResult();
        }

        /* a method that show today's shipped order to the listview */
        private void showResult()
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

                item.SubItems.Add(value.TransactionID);
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
                item.SubItems.Add(value.Package.RefundLink);

                listview.Items.Add(item);
            }
            #endregion
        }

        #region Top Buttons
        /* cancel shipment button click that mark the selected order to cancelled in database */
        private void cancelShipmentButton_Click(object sender, EventArgs e)
        {
            // error check
            if (listview.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select the shipment you want to cancel", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // local fields for storing data
            List<string> list = new List<string>();

            // adding cancel order list and post shipment void
            List<Order> cancelList = (from ListViewItem item in listview.CheckedItems
                select new Order
                {
                    source = item.SubItems[0].Text,
                    transactionId = item.SubItems[1].Text,
                    shipmentIdentificationNumber = item.SubItems[2].Text
                }).ToList();

            // sears cancellation
            foreach (Order cancelledOrder in cancelList.Where(cancelledOrder => cancelledOrder.source == "Sears"))
            {
                list.Add(cancelledOrder.transactionId);
                ups.postShipmentVoid(cancelledOrder.shipmentIdentificationNumber);
                if (ups.Error)
                {
                    MessageBox.Show(ups.ErrorMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            sears.PostVoid(list.ToArray());

            // show new result
            showResult();
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
