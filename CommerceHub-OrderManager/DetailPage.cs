using CommerceHub_OrderManager.channel.sears;
using CommerceHub_OrderManager.supportingClasses;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CommerceHub_OrderManager
{
    public partial class DetailPage : Form
    {
        // field for storing order details
        private SearsValues value;

        /* constructor that initializes graphic compents and order fields */
        public DetailPage(SearsValues value)
        {
            InitializeComponent();

            this.value = value;
            showResult(value);
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

        #region ListView
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
        }

        /* when clicked set the reason of cancelling to the checked items */
        private void setReasonButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listview.CheckedItems)
            {
                item.SubItems[6].Text = reasonCombobox.SelectedItem.ToString();
            }
        }

        /* prevent user from changing header size */
        private void listview_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listview.Columns[e.ColumnIndex].Width;
        }
        #endregion

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
            double price = 0;
            foreach (double unitPrice in value.UnitPrice)
                price += unitPrice;
            unitPriceTotalTextbox.Text = price.ToString();

            // GST and HST
            price = 0;
            foreach (double gstHst in value.GST_HST_Extended)
                price += gstHst;
            foreach (double gstHst in value.GST_HST_Total)
                price += gstHst;
            gsthstTextbox.Text = price.ToString();

            // PST
            price = 0;
            foreach (double pst in value.PST_Extended)
                price += pst;
            foreach (double pst in value.PST_Total)
                price += pst;
            pstTextbox.Text = price.ToString();

            // other fee
            price = 0;
            foreach (double fee in value.LineHandling)
                price += fee;
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

            for (int i = 0; i < value.LineCount; i++)
            {
                ListViewItem item = new ListViewItem();

                item.SubItems.Add(value.Description[i] + "  SKU: " + value.TrxVendorSKU[i]);
                item.SubItems.Add("$ " + value.UnitPrice[i]);
                item.SubItems.Add(value.TrxQty[i].ToString());
                item.SubItems.Add("$ " + value.LineBalanceDue[i]);
                item.SubItems.Add("");
                item.SubItems.Add("");

                listview.Items.Add(item);
            }
        }
    }
}
