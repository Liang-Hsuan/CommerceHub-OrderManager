using CommerceHub_OrderManager.channel.brightpearl;
using CommerceHub_OrderManager.channel.sears;
using CommerceHub_OrderManager.supportingClasses;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CommerceHub_OrderManager
{
    public partial class Main : Form
    {
        // field for commerce hub order
        private readonly Sears sears = new Sears();

        // field for brightpearl connection
        private readonly BPconnect bp = new BPconnect();

        // field for storing data
        private struct Order
        {
            public string source;
            public string transactionId;
        }
        private Order[] orderList;

        public Main()
        {
            InitializeComponent();

            // show new orders from sears
            showSearsResult();

            // show result on the chart
            refreshGraph();
        }

        /* save data when form close */
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Date = DateTime.Today;
            Properties.Settings.Default.Save();
        }

        #region Top Buttons
        /* ship and confirm the selected items */
        private void shipmentConfirmButton_Click(object sender, EventArgs e)
        {
            // error check -> the case if the user has not select any of the order to confirm
            if (listview.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select the order you want to ship", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // get user confirmation
            ConfirmPanel confirm = new ConfirmPanel("Are you sure you want to ship the order you selected ?");
            confirm.ShowDialog(this);

            // if user select to confirm, process the orders that have been checked
            if (confirm.DialogResult == DialogResult.OK)
            {
                // start the timer 
                timer.Start();

                // set confirm button to enable
                shipmentConfirmButton.Enabled = false;

                // initialize orderList for further shipment confirm use
                int length = listview.CheckedItems.Count;
                orderList = new Order[length];

                // adding each order to the list with source and transaction id
                for (int i = 0; i < length; i++)
                {
                    Order order = new Order();

                    order.source = listview.CheckedItems[i].SubItems[0].Text;
                    order.transactionId = listview.CheckedItems[i].SubItems[4].Text;

                    orderList[i] = order;
                }

                // call backgorund worker
                if (!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync();
            }
        }

        /* print button click that export the checked items' packing slip */
        private void printButton_Click(object sender, EventArgs e)
        {
            // error check -> the case if the user has not select any of the order to print
            if (listview.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select the order you want to export the packing slip", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // initialize printing objects
            SearsPackingSlip searsPS = new SearsPackingSlip();

            // fields for message
            string message = "Packing Slip have successfully exported to\n";
            bool channel = false;

            // check the user check item and get the selected transaction id for exportin packing slip
            foreach (SearsValues value in from ListViewItem item in listview.CheckedItems where item.SubItems[0].Text == "Sears" select item.SubItems[4].Text into transaction select sears.GenerateValue(transaction))
            {
                searsPS.createPackingSlip(value, new int[0], false);
                channel = true;
            }

            // create message
            if (channel)
                message += searsPS.SavePath;

            MessageBox.Show(message, "Congratulation");
        }

        /* the event for refresh button clicks that refresh the order in listview and the chart */
        private void refreshButton_Click(object sender, EventArgs e)
        {
            showSearsResult();
            refreshGraph();
        }

        /* tje evemt fpr shipment button click that show the shipment page */
        private void shipmentButton_Click(object sender, EventArgs e)
        {
            new ShipmentPage().ShowDialog(this);
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

            SearsValues value = sears.GenerateValue(listview.CheckedItems[0].SubItems[4].Text);

            new DetailPage(value).ShowDialog(this);
        }
        #endregion

        #region Shipment Confirm Event
        /* background worker that processing each order from the orderList */
        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // initialize UPS field in case there is order require UPS shipment
            UPS ups = new UPS();

            // start processing orders
            foreach (Order order in orderList)
            {
                #region Sears Order
                // for sears order
                if (order.source == "Sears")
                {
                    // first get the detail for the order
                    SearsValues value = sears.GenerateValue(order.transactionId);

                    // second ship it
                    string[] digest = ups.postShipmentConfirm(value, new Package(value));
                    if (digest == null)
                    {
                        MessageBox.Show("Error occur while requesting shipment, please refresh and try again.", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (digest.Contains("Error:"))
                    {
                        MessageBox.Show(digest[0], "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string[] result = ups.postShipmentAccept(digest[1]);
                    if (result == null)
                    {
                        MessageBox.Show("Error occur while requesting shipment, please refresh and try again.", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // get identification, tracking, label and shipment confirm with no cancellation of item
                    value.Package.IdentificationNumber = digest[0];
                    value.Package.TrackingNumber = result[0];
                    ups.exportLabel(result[1], value.TransactionID, false);
                    sears.GenerateXML(value, new System.Collections.Generic.Dictionary<int, string>());

                    // post order to brightpearl with no cancellation
                    bp.postOrder(value, new int[0]); 
                }
                #endregion
            }
        }
        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // stop the timer
            timer.Stop();

            // set confirm button to enable and status label to nothing
            statusLabel.Text = "";
            shipmentConfirmButton.Enabled = true;

            // show user that the orders have completed
            MessageBox.Show("Order have been processed successfully.\nPacking slip have been exported to Desktop.", "Congratulation", MessageBoxButtons.OK);
        }

        /* timer that displaying status change */
        private void timer_Tick(object sender, EventArgs e)
        {
            statusLabel.Text = bp.Status;
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

        #region Supporting Methods
        /* a supporting method that get new order from sears and show them on the list view */
        private void showSearsResult()
        {
            // first clear the listview
            listview.Items.Clear();

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

                int total = value.TrxQty.Sum();
                item.SubItems.Add(total.ToString());

                item.SubItems.Add(value.Recipient.Name);
                listview.Items.Add(item);
            }
        }

        /* a supporting method that refresh the chart */
        private void refreshGraph()
        {
            // clear chart first
            foreach (var series in chart.Series)
                series.Points.Clear();

            // creating chart
            DateTime from = DateTime.Today;
            for (int i = -6; i <= 0; i++)
            {
                from = DateTime.Today.AddDays(i);

                int order = sears.GetNumberOfOrder(from);
                int shipped = sears.GetNumberOfShipped(from);

                if (order < 1)
                {
                    chart.Series["orders"].Points.AddXY(from.ToString("MM/dd/yyyy"), 0);
                    chart.Series["point"].Points.AddXY(from.ToString("MM/dd/yyyy"), 0);
                    chart.Series["shipment"].Points.AddXY(from.ToString("MM/dd/yyyy"), 0);
                }
                else
                {
                    chart.Series["orders"].Points.AddXY(from.ToString("MM/dd/yyyy"), order);
                    chart.Series["point"].Points.AddXY(from.ToString("MM/dd/yyyy"), order);
                    chart.Series["shipment"].Points.AddXY(from.ToString("MM/dd/yyyy"), shipped);
                }
            }

            chart.Series["shipment"]["PointWidth"] = "0.1";
            chart.Series["point"].MarkerSize = 10;
        }
        #endregion

    }
}
