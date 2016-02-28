using CommerceHub_OrderManager.channel.sears;
using System;
using System.Windows.Forms;

namespace CommerceHub_OrderManager
{
    public partial class Main : Form
    {
        // field for keeping track of invoice number
        private static int iterator;

        // field for commerce hub order
        private Sears sears = new Sears();

        public Main()
        {
            InitializeComponent();

            // show new orders from sears
            showSearsResult();
        }

        /* save data when the form is closing */
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Date = DateTime.Now;
            Properties.Settings.Default.Iterator = iterator;
        }

        #region Top Buttons
        /* print button click that export the checked items' packing slip */
        private void printButton_Click(object sender, EventArgs e)
        {
            // initialize printing objects
            SearsPackingSlip searsPS = new SearsPackingSlip();

            // check the user check item and get the selected transaction id for exportin packing slip
            foreach (ListViewItem item in listview.CheckedItems)
            {
                // for sears order
                if (item.SubItems[0].Text == "Sears")
                {
                    string transaction = item.SubItems[4].Text;
                    SearsValues value = sears.generateValue(transaction);
                    searsPS.createPackingSlip(value, new int[0]);
                }
            }
        }

        /* the event for detail button click that show the detail page for the selected item */
        private void detailButton_Click(object sender, EventArgs e)
        {
            // the case if the user does not select any thing or select more than one
            if (listview.CheckedItems.Count != 1)
            {
                MessageBox.Show("Please select one item to see more details", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SearsValues value = sears.generateValue(listview.CheckedItems[0].SubItems[4].Text);

            new DetailPage(value).ShowDialog(this);
        }
        #endregion

        /* the event for selection all checkbox check that select all the items in the list view */
        private void selectionAllCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (selectionAllCheckbox.Checked)
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

        /* a supporting method that get new order from sears and show them on the list view */
        private void showSearsResult()
        {
            // get orders from sears
            sears.GetOrder();
            SearsValues[] searsValue = sears.GetAllNewOrder();

            // show new orders to the list view 
            DateTime timeNow = DateTime.Now;
            foreach (SearsValues value in searsValue)
            {
                ListViewItem item = new ListViewItem("Sears");

                TimeSpan span = timeNow.Subtract(value.CustOrderDate);
                item.SubItems.Add(span.Days + "d " + span.Hours + "h");

                if (value.LineCount > 1)
                {
                    item.SubItems.Add("(Multiple Items)");
                    item.SubItems.Add("(Multiple Items)");
                }
                else
                {
                    item.SubItems.Add(value.Description[0]);
                    item.SubItems.Add(value.TrxVendorSKU[0]);
                }

                item.SubItems.Add(value.TransactionID);
                item.SubItems.Add(value.CustOrderDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(value.TrxBalanceDue.ToString());

                int total = 0;
                foreach (int qty in value.TrxQty)
                    total += qty;
                item.SubItems.Add(total.ToString());

                item.SubItems.Add(value.Recipient.Name);
                listview.Items.Add(item);
            }
        }
    }
}
