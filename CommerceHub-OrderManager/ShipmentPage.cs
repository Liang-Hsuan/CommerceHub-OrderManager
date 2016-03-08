using CommerceHub_OrderManager.channel.sears;
using CommerceHub_OrderManager.supportingClasses;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CommerceHub_OrderManager
{
    /*
     * An application module that can manage the shipped order
     */
    public partial class ShipmentPage : Form
    {
        // field for commerce hub order
        private Sears sears = new Sears();

        // field for UPS connection
        UPS ups = new UPS();

        // field for storing data
        private struct Order
        {
            public string source;
            public string transactionId;
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

            // get shipped items from channels
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
            List<Order> cancelList = new List<Order>();
            List<string> list = new List<string>();

            // adding cancel order list and post shipment void
            foreach (ListViewItem item in listview.CheckedItems)
            {
                Order order = new Order();

                order.source = item.SubItems[0].Text;
                order.transactionId = item.SubItems[1].Text;
                ups.postShipmentVoid(item.SubItems[2].Text);

                cancelList.Add(order);
            }

            // sears cancellation
            foreach (Order cancelledOrder in cancelList)
            {
                if (cancelledOrder.source == "Sears")
                    list.Add(cancelledOrder.transactionId);
            }
            sears.PostCancel(list.ToArray());

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
