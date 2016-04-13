using System;
using System.Linq;
using System.Windows.Forms;
using Order_Manager.channel.brightpearl;
using Order_Manager.channel.sears;
using Order_Manager.supportingClasses;
using Order_Manager.supportingClasses.Shipment;
using Order_Manager.channel.shop.ca;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace Order_Manager.mainForms
{
    public partial class Main : Form
    {
        // field for online shopping channels' order
        private readonly Sears sears = new Sears();
        private readonly ShopCa shopCa = new ShopCa();

        // field for brightpearl connection
        private readonly BPconnect bp = new BPconnect();

        // field for parent
        private readonly Form parent;

        // field for storing data
        private struct Order
        {
            public string Source;
            public string TransactionId;
        }
        private Order[] orderList;

        public Main(Form parent)
        {
            InitializeComponent();

            // show new orders from shopping channels
            ShowResult();

            // show result on the chart
            RefreshGraph();

            // get parent
            this.parent = parent;
        }

        #region Close Events
        /* save data when form close */
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Date = DateTime.Today;
            Properties.Settings.Default.Save();

            parent.Close();
        }

        /* delete data that are too old after program is closed */
        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            sears.Delete();
            shopCa.Delete();
        }
        #endregion

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
            if (confirm.DialogResult != DialogResult.OK) return;

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
                Order order = new Order
                {
                    Source = listview.CheckedItems[i].SubItems[0].Text,
                    TransactionId = listview.CheckedItems[i].SubItems[4].Text
                };

                orderList[i] = order;
            }

            // call backgorund worker
            if (!backgroundWorker.IsBusy)
                backgroundWorker.RunWorkerAsync();
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

            // fields for message
            string message = "Packing Slip have successfully exported to";
            bool[] channel = { false, false };  // [0] sears, [1] shop.ca

            foreach (Order order in from ListViewItem item in listview.CheckedItems
                                    select new Order
                                    {
                                        Source = item.SubItems[0].Text,
                                        TransactionId = item.SubItems[4].Text
                                    })
            {
                switch (order.Source)
                {
                    case "Sears":
                        {
                            // the case if it is sears order
                            try
                            {
                                SearsPackingSlip.CreatePackingSlip(sears.GenerateValue(order.TransactionId), new int[0], false);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            channel[0] = true;
                        }
                        break;
                    case "Shop.ca":
                        {
                            // the case if it is shop.ca order
                            try
                            {
                                ShopCaPackingSlip.CreatePackingSlip(shopCa.GenerateValue(order.TransactionId), new int[0], false);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            channel[1] = true;
                        }
                        break;
                }
            }

            // create message
            if (channel[0])
                message += "\n" + SearsPackingSlip.SavePath;
            if (channel[1])
                message += "\n" + ShopCaPackingSlip.SavePath;

            MessageBox.Show(message, "Congratulation");
        }

        /* the event for refresh button clicks that refresh the order in listview and the chart */
        private void refreshButton_Click(object sender, EventArgs e)
        {
            // set wait cursor for working
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            ShowResult();
            RefreshGraph();

            // set default cursor after complete
            System.Windows.Forms.Cursor.Current = Cursors.Default;
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

            switch (listview.CheckedItems[0].SubItems[0].Text)
            {
                case "Sears":
                    {
                        // the case if it is sears order
                        SearsValues value = sears.GenerateValue(listview.CheckedItems[0].SubItems[4].Text);
                        new DetailPage(value).ShowDialog(this);
                    }
                    break;
                case "Shop.ca":
                    {
                        // the case if it is shopl.ca order
                        ShopCaValues value = shopCa.GenerateValue(listview.CheckedItems[0].SubItems[4].Text);
                        new DetailPage(value).ShowDialog(this);
                    }
                    break;
            }
        }
        #endregion

        #region Shipment Confirm Event
        /* background worker that processing each order from the orderList */
        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // initialize all carrier fields
            Ups ups = new Ups();
            CanadaPost canadaPost = new CanadaPost();

            // start processing orders
            foreach (Order order in orderList)
            {
                // for sears order
                switch (order.Source)
                {
                    case "Sears":
                        {
                            #region Sears Order
                            // first get the detail for the order
                            SearsValues value = sears.GenerateValue(order.TransactionId);

                            // check if the order has been shipped before -> if not, ship it now
                            if (value.Package.TrackingNumber != "")
                            {
                                value.Package = new Package(value);

                                // second ship it
                                string[] digest = ups.PostShipmentConfirm(value);
                                if (ups.Error)
                                {
                                    MessageBox.Show(ups.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                string[] result = ups.PostShipmentAccept(digest[1]);
                                if (ups.Error)
                                {
                                    MessageBox.Show(ups.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                // get identification, tracking, label and shipment confirm with no cancellation of item
                                value.Package.IdentificationNumber = digest[0];
                                value.Package.TrackingNumber = result[0];
                                ups.ExportLabel(result[1], value.TransactionId, false);
                            }
                            sears.GenerateXml(value, new System.Collections.Generic.Dictionary<int, string>());

                            // post order to brightpearl with no cancellation
                            bp.PostOrder(value, new int[0]);
                            #endregion
                        }
                        break;
                    case "Shop.ca":
                        {
                            #region Shop.ca Order
                            // first get the detail for the order
                            ShopCaValues value = shopCa.GenerateValue(order.TransactionId);

                            // check if the order has been shipped before -> if not, ship it now
                            if (value.Package.TrackingNumber != "")
                            {
                                value.Package = new Package(value);

                                // second ship it
                                string[] links = canadaPost.CreateShipment(value);
                                if (canadaPost.Error)
                                {
                                    MessageBox.Show(canadaPost.ErrorMessage, "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                // get tracking, self link, label link and shipment confirm with no cancellation of item
                                value.Package.TrackingNumber = links[0];
                                value.Package.SelfLink = links[1];
                                value.Package.LabelLink = links[2];

                                System.Threading.Thread.Sleep(5000);

                                // get artifact and export it
                                byte[] binary = canadaPost.GetArtifact(links[2]);
                                canadaPost.ExportLabel(binary, value.OrderId, true, false);
                            }
                            shopCa.GenerateCsv(value, new System.Collections.Generic.Dictionary<int, string>());

                            // post order to brightpearl with no cancellation
                            bp.PostOrder(value, new int[0]);
                            #endregion
                        }
                        break;
                }
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
            MessageBox.Show("Order have been processed successfully.\nShipping labels have been exported to Desktop.", "Congratulation", MessageBoxButtons.OK);
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
        /* a supporting method that get new order from online shopping channels and show them on the list view */
        private void ShowResult()
        {
            // first clear the listview
            listview.Items.Clear();

            #region Sears
            // get orders from sears
            sears.GetOrder();
            SearsValues[] searsValue = sears.GetAllNewOrder();

            // show sears new orders to the list view 
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
                    item.SubItems.Add(value.TrxVendorSku[0]);
                }

                item.SubItems.Add(value.TransactionId);
                item.SubItems.Add(value.CustOrderDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(value.TrxBalanceDue.ToString());

                int total = value.TrxQty.Sum();
                item.SubItems.Add(total.ToString());

                item.SubItems.Add(value.Recipient.Name);
                listview.Items.Add(item);
            }
            #endregion

            #region Shop.ca
            // get orders from shop.ca
            shopCa.GetOrder();
            ShopCaValues[] shopCaValues = shopCa.GetAllNewOrder();

            // show shop.ca new orders to the list view
            foreach (ShopCaValues value in shopCaValues)
            {
                ListViewItem item = new ListViewItem("Shop.ca");

                TimeSpan span = timeNow.Subtract(value.OrderCreateDate);
                item.SubItems.Add(span.Days + "d " + span.Hours + "h");

                if (value.OrderItemId.Count > 1)
                {
                    item.SubItems.Add("(Multiple Items)");
                    item.SubItems.Add("(Multiple Items)");
                }
                else
                {
                    item.SubItems.Add(value.Title[0]);
                    item.SubItems.Add(value.Sku[0]);
                }

                item.SubItems.Add(value.OrderId);
                item.SubItems.Add(value.OrderCreateDate.ToString("yyyy-MM-dd"));
                item.SubItems.Add(value.GrandTotal.ToString());

                int total = value.Quantity.Sum();
                item.SubItems.Add(total.ToString());

                item.SubItems.Add(value.ShipTo.Name);
                listview.Items.Add(item);
            }
            #endregion
        }

        /* a supporting method that refresh the chart */
        private void RefreshGraph()
        {
            // clear chart first
            foreach (var series in chart.Series)
                series.Points.Clear();

            // creating chart
            DateTime from = DateTime.Today;
            for (int i = -6; i <= 0; i++)
            {
                from = DateTime.Today.AddDays(i);

                int order = sears.GetNumberOfOrder(from) + shopCa.GetNumberOfOrder(from);
                int shipped = sears.GetNumberOfShipped(from) + shopCa.GetNumberOfShipped(from);

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

        #region Chart Events
        /* restore focus */
        private void chart_MouseEnter(object sender, EventArgs e)
        {
            chart.Focus();
        }

        /* mouse move event for the chart that show the line of the current position */
        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            chart.ChartAreas[0].CursorY.SetCursorPixelPosition(new Point(e.X, e.Y), true);
        }

        /* tooltip event for the chart that show the tooptip for the mouse value on */
        private void chart_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            // check selected chart element is a data point and set tooltip text
            if (e.HitTestResult.ChartElementType != ChartElementType.DataPoint) return;

            // get selected data point and its x-axis label
            DataPoint dataPoint = (DataPoint)e.HitTestResult.Object;
            DateTime time = DateTime.ParseExact(dataPoint.AxisLabel, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            if (chart.Series["point"].Points.Contains(dataPoint))
                e.Text = "Order\nSears: " + sears.GetNumberOfOrder(time) + ", Shop.ca: " + shopCa.GetNumberOfOrder(time);
            else if (chart.Series["shipment"].Points.Contains(dataPoint))
                e.Text = "Shipment\nSears: " + sears.GetNumberOfShipped(time) + ", Shop.ca: " + shopCa.GetNumberOfShipped(time);
        }

        /* mouse wheel event on the chart that zoom in and out */
        private void chart_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Delta < 0)
                {
                    chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                    chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0)
                {
                    double xMin = chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                    double xMax = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                    double yMin = chart.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                    double yMax = chart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

                    double posXStart = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 3;
                    double posXFinish = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 3;
                    double posYStart = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2;
                    double posYFinish = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2;

                    chart.ChartAreas[0].AxisX.ScaleView.Zoom(posXStart, posXFinish);
                    chart.ChartAreas[0].AxisY.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { /* ignore */ }
        }
        #endregion
    }
}
