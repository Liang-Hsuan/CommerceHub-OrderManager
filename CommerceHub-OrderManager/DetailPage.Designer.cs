namespace CommerceHub_OrderManager
{
    partial class DetailPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailPage));
            this.orderSummaryPanel = new System.Windows.Forms.Panel();
            this.moneyPicturebox = new System.Windows.Forms.PictureBox();
            this.totalOrderTextbox = new System.Windows.Forms.TextBox();
            this.totalLabel = new System.Windows.Forms.Label();
            this.unitPriceTotalTextbox = new System.Windows.Forms.TextBox();
            this.gsthstTextbox = new System.Windows.Forms.TextBox();
            this.pstTextbox = new System.Windows.Forms.TextBox();
            this.otherFeeTextbox = new System.Windows.Forms.TextBox();
            this.otherFeeLabel = new System.Windows.Forms.Label();
            this.pstLabel = new System.Windows.Forms.Label();
            this.hstgstLabel = new System.Windows.Forms.Label();
            this.unitPriceTotalLabel = new System.Windows.Forms.Label();
            this.shipByDateTextbox = new System.Windows.Forms.TextBox();
            this.paidDateTextbox = new System.Windows.Forms.TextBox();
            this.orderDateTextbox = new System.Windows.Forms.TextBox();
            this.shipByDateLabel = new System.Windows.Forms.Label();
            this.paidDateLabel = new System.Windows.Forms.Label();
            this.orderDateLabel = new System.Windows.Forms.Label();
            this.orderSummaryLabel = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.printPackingSlipButton = new System.Windows.Forms.Button();
            this.topOrderNumberTextbox = new System.Windows.Forms.TextBox();
            this.topStatusLabel = new System.Windows.Forms.Label();
            this.topOrderLabel = new System.Windows.Forms.Label();
            this.logoPicturebox = new System.Windows.Forms.PictureBox();
            this.recipientPanel = new System.Windows.Forms.Panel();
            this.soldToPhoneTextbox = new System.Windows.Forms.TextBox();
            this.housePicturebox = new System.Windows.Forms.PictureBox();
            this.verifyButton = new System.Windows.Forms.Button();
            this.shipToPhoneTextbox = new System.Windows.Forms.TextBox();
            this.verifyTextbox = new System.Windows.Forms.TextBox();
            this.shipToCombineTextbox = new System.Windows.Forms.TextBox();
            this.shipToAddress2Textbox = new System.Windows.Forms.TextBox();
            this.shipToAddress1Textbox = new System.Windows.Forms.TextBox();
            this.shipToNameTextbox = new System.Windows.Forms.TextBox();
            this.shipToLabel = new System.Windows.Forms.Label();
            this.soldToTextbox = new System.Windows.Forms.TextBox();
            this.soldToLabel = new System.Windows.Forms.Label();
            this.buyerLabel = new System.Windows.Forms.Label();
            this.listview = new System.Windows.Forms.ListView();
            this.selectionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.itemHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.unitHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.qtyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.totalHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cancelHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.reasonHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectAllCheckbox = new System.Windows.Forms.CheckBox();
            this.markCancelButton = new System.Windows.Forms.Button();
            this.reasonCombobox = new System.Windows.Forms.ComboBox();
            this.setReasonButton = new System.Windows.Forms.Button();
            this.orderSummaryPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.moneyPicturebox)).BeginInit();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPicturebox)).BeginInit();
            this.recipientPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.housePicturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // orderSummaryPanel
            // 
            this.orderSummaryPanel.BackColor = System.Drawing.Color.White;
            this.orderSummaryPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.orderSummaryPanel.Controls.Add(this.moneyPicturebox);
            this.orderSummaryPanel.Controls.Add(this.totalOrderTextbox);
            this.orderSummaryPanel.Controls.Add(this.totalLabel);
            this.orderSummaryPanel.Controls.Add(this.unitPriceTotalTextbox);
            this.orderSummaryPanel.Controls.Add(this.gsthstTextbox);
            this.orderSummaryPanel.Controls.Add(this.pstTextbox);
            this.orderSummaryPanel.Controls.Add(this.otherFeeTextbox);
            this.orderSummaryPanel.Controls.Add(this.otherFeeLabel);
            this.orderSummaryPanel.Controls.Add(this.pstLabel);
            this.orderSummaryPanel.Controls.Add(this.hstgstLabel);
            this.orderSummaryPanel.Controls.Add(this.unitPriceTotalLabel);
            this.orderSummaryPanel.Controls.Add(this.shipByDateTextbox);
            this.orderSummaryPanel.Controls.Add(this.paidDateTextbox);
            this.orderSummaryPanel.Controls.Add(this.orderDateTextbox);
            this.orderSummaryPanel.Controls.Add(this.shipByDateLabel);
            this.orderSummaryPanel.Controls.Add(this.paidDateLabel);
            this.orderSummaryPanel.Controls.Add(this.orderDateLabel);
            this.orderSummaryPanel.Controls.Add(this.orderSummaryLabel);
            this.orderSummaryPanel.Location = new System.Drawing.Point(12, 103);
            this.orderSummaryPanel.Name = "orderSummaryPanel";
            this.orderSummaryPanel.Size = new System.Drawing.Size(439, 257);
            this.orderSummaryPanel.TabIndex = 0;
            // 
            // moneyPicturebox
            // 
            this.moneyPicturebox.Image = ((System.Drawing.Image)(resources.GetObject("moneyPicturebox.Image")));
            this.moneyPicturebox.Location = new System.Drawing.Point(48, 176);
            this.moneyPicturebox.Name = "moneyPicturebox";
            this.moneyPicturebox.Size = new System.Drawing.Size(51, 51);
            this.moneyPicturebox.TabIndex = 17;
            this.moneyPicturebox.TabStop = false;
            // 
            // totalOrderTextbox
            // 
            this.totalOrderTextbox.BackColor = System.Drawing.Color.White;
            this.totalOrderTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.totalOrderTextbox.ForeColor = System.Drawing.Color.Gray;
            this.totalOrderTextbox.Location = new System.Drawing.Point(313, 207);
            this.totalOrderTextbox.MaxLength = 20;
            this.totalOrderTextbox.Name = "totalOrderTextbox";
            this.totalOrderTextbox.ReadOnly = true;
            this.totalOrderTextbox.Size = new System.Drawing.Size(100, 13);
            this.totalOrderTextbox.TabIndex = 16;
            // 
            // totalLabel
            // 
            this.totalLabel.AutoSize = true;
            this.totalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.totalLabel.Location = new System.Drawing.Point(236, 207);
            this.totalLabel.Name = "totalLabel";
            this.totalLabel.Size = new System.Drawing.Size(71, 13);
            this.totalLabel.TabIndex = 15;
            this.totalLabel.Text = "Total Order";
            // 
            // unitPriceTotalTextbox
            // 
            this.unitPriceTotalTextbox.BackColor = System.Drawing.Color.White;
            this.unitPriceTotalTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.unitPriceTotalTextbox.ForeColor = System.Drawing.Color.Gray;
            this.unitPriceTotalTextbox.Location = new System.Drawing.Point(313, 59);
            this.unitPriceTotalTextbox.MaxLength = 20;
            this.unitPriceTotalTextbox.Name = "unitPriceTotalTextbox";
            this.unitPriceTotalTextbox.ReadOnly = true;
            this.unitPriceTotalTextbox.Size = new System.Drawing.Size(100, 13);
            this.unitPriceTotalTextbox.TabIndex = 14;
            // 
            // gsthstTextbox
            // 
            this.gsthstTextbox.BackColor = System.Drawing.Color.White;
            this.gsthstTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gsthstTextbox.ForeColor = System.Drawing.Color.Gray;
            this.gsthstTextbox.Location = new System.Drawing.Point(313, 91);
            this.gsthstTextbox.MaxLength = 20;
            this.gsthstTextbox.Name = "gsthstTextbox";
            this.gsthstTextbox.ReadOnly = true;
            this.gsthstTextbox.Size = new System.Drawing.Size(100, 13);
            this.gsthstTextbox.TabIndex = 13;
            // 
            // pstTextbox
            // 
            this.pstTextbox.BackColor = System.Drawing.Color.White;
            this.pstTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pstTextbox.ForeColor = System.Drawing.Color.Gray;
            this.pstTextbox.Location = new System.Drawing.Point(313, 122);
            this.pstTextbox.MaxLength = 20;
            this.pstTextbox.Name = "pstTextbox";
            this.pstTextbox.ReadOnly = true;
            this.pstTextbox.Size = new System.Drawing.Size(100, 13);
            this.pstTextbox.TabIndex = 12;
            // 
            // otherFeeTextbox
            // 
            this.otherFeeTextbox.BackColor = System.Drawing.Color.White;
            this.otherFeeTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.otherFeeTextbox.ForeColor = System.Drawing.Color.Gray;
            this.otherFeeTextbox.Location = new System.Drawing.Point(313, 154);
            this.otherFeeTextbox.MaxLength = 20;
            this.otherFeeTextbox.Name = "otherFeeTextbox";
            this.otherFeeTextbox.ReadOnly = true;
            this.otherFeeTextbox.Size = new System.Drawing.Size(100, 13);
            this.otherFeeTextbox.TabIndex = 11;
            // 
            // otherFeeLabel
            // 
            this.otherFeeLabel.AutoSize = true;
            this.otherFeeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.otherFeeLabel.Location = new System.Drawing.Point(244, 154);
            this.otherFeeLabel.Name = "otherFeeLabel";
            this.otherFeeLabel.Size = new System.Drawing.Size(63, 13);
            this.otherFeeLabel.TabIndex = 10;
            this.otherFeeLabel.Text = "Other Fee";
            // 
            // pstLabel
            // 
            this.pstLabel.AutoSize = true;
            this.pstLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pstLabel.Location = new System.Drawing.Point(243, 122);
            this.pstLabel.Name = "pstLabel";
            this.pstLabel.Size = new System.Drawing.Size(64, 13);
            this.pstLabel.TabIndex = 9;
            this.pstLabel.Text = "PST Total";
            // 
            // hstgstLabel
            // 
            this.hstgstLabel.AutoSize = true;
            this.hstgstLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hstgstLabel.Location = new System.Drawing.Point(213, 91);
            this.hstgstLabel.Name = "hstgstLabel";
            this.hstgstLabel.Size = new System.Drawing.Size(94, 13);
            this.hstgstLabel.TabIndex = 8;
            this.hstgstLabel.Text = "GST HST Total";
            // 
            // unitPriceTotalLabel
            // 
            this.unitPriceTotalLabel.AutoSize = true;
            this.unitPriceTotalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unitPriceTotalLabel.Location = new System.Drawing.Point(211, 59);
            this.unitPriceTotalLabel.Name = "unitPriceTotalLabel";
            this.unitPriceTotalLabel.Size = new System.Drawing.Size(96, 13);
            this.unitPriceTotalLabel.TabIndex = 7;
            this.unitPriceTotalLabel.Text = "Unit Price Total";
            // 
            // shipByDateTextbox
            // 
            this.shipByDateTextbox.BackColor = System.Drawing.Color.White;
            this.shipByDateTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipByDateTextbox.ForeColor = System.Drawing.Color.Gray;
            this.shipByDateTextbox.Location = new System.Drawing.Point(105, 122);
            this.shipByDateTextbox.MaxLength = 20;
            this.shipByDateTextbox.Name = "shipByDateTextbox";
            this.shipByDateTextbox.ReadOnly = true;
            this.shipByDateTextbox.Size = new System.Drawing.Size(100, 13);
            this.shipByDateTextbox.TabIndex = 6;
            // 
            // paidDateTextbox
            // 
            this.paidDateTextbox.BackColor = System.Drawing.Color.White;
            this.paidDateTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.paidDateTextbox.ForeColor = System.Drawing.Color.Gray;
            this.paidDateTextbox.Location = new System.Drawing.Point(105, 91);
            this.paidDateTextbox.MaxLength = 20;
            this.paidDateTextbox.Name = "paidDateTextbox";
            this.paidDateTextbox.ReadOnly = true;
            this.paidDateTextbox.Size = new System.Drawing.Size(100, 13);
            this.paidDateTextbox.TabIndex = 5;
            // 
            // orderDateTextbox
            // 
            this.orderDateTextbox.BackColor = System.Drawing.Color.White;
            this.orderDateTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.orderDateTextbox.ForeColor = System.Drawing.Color.Gray;
            this.orderDateTextbox.Location = new System.Drawing.Point(105, 59);
            this.orderDateTextbox.MaxLength = 20;
            this.orderDateTextbox.Name = "orderDateTextbox";
            this.orderDateTextbox.ReadOnly = true;
            this.orderDateTextbox.Size = new System.Drawing.Size(100, 13);
            this.orderDateTextbox.TabIndex = 4;
            // 
            // shipByDateLabel
            // 
            this.shipByDateLabel.AutoSize = true;
            this.shipByDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shipByDateLabel.Location = new System.Drawing.Point(18, 122);
            this.shipByDateLabel.Name = "shipByDateLabel";
            this.shipByDateLabel.Size = new System.Drawing.Size(81, 13);
            this.shipByDateLabel.TabIndex = 3;
            this.shipByDateLabel.Text = "Ship-By Date";
            // 
            // paidDateLabel
            // 
            this.paidDateLabel.AutoSize = true;
            this.paidDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.paidDateLabel.Location = new System.Drawing.Point(36, 91);
            this.paidDateLabel.Name = "paidDateLabel";
            this.paidDateLabel.Size = new System.Drawing.Size(63, 13);
            this.paidDateLabel.TabIndex = 2;
            this.paidDateLabel.Text = "Paid Date";
            // 
            // orderDateLabel
            // 
            this.orderDateLabel.AutoSize = true;
            this.orderDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.orderDateLabel.Location = new System.Drawing.Point(30, 59);
            this.orderDateLabel.Name = "orderDateLabel";
            this.orderDateLabel.Size = new System.Drawing.Size(69, 13);
            this.orderDateLabel.TabIndex = 1;
            this.orderDateLabel.Text = "Order Date";
            // 
            // orderSummaryLabel
            // 
            this.orderSummaryLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.orderSummaryLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.orderSummaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.orderSummaryLabel.Location = new System.Drawing.Point(0, 0);
            this.orderSummaryLabel.Name = "orderSummaryLabel";
            this.orderSummaryLabel.Size = new System.Drawing.Size(438, 35);
            this.orderSummaryLabel.TabIndex = 0;
            this.orderSummaryLabel.Text = "Order Summary";
            this.orderSummaryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // topPanel
            // 
            this.topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topPanel.BackColor = System.Drawing.Color.White;
            this.topPanel.Controls.Add(this.printPackingSlipButton);
            this.topPanel.Controls.Add(this.topOrderNumberTextbox);
            this.topPanel.Controls.Add(this.topStatusLabel);
            this.topPanel.Controls.Add(this.topOrderLabel);
            this.topPanel.Controls.Add(this.logoPicturebox);
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1436, 74);
            this.topPanel.TabIndex = 1;
            // 
            // printPackingSlipButton
            // 
            this.printPackingSlipButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.printPackingSlipButton.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.printPackingSlipButton.FlatAppearance.BorderSize = 2;
            this.printPackingSlipButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.printPackingSlipButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.printPackingSlipButton.Location = new System.Drawing.Point(1302, 16);
            this.printPackingSlipButton.Name = "printPackingSlipButton";
            this.printPackingSlipButton.Size = new System.Drawing.Size(137, 41);
            this.printPackingSlipButton.TabIndex = 6;
            this.printPackingSlipButton.Text = "Print Packing Slip";
            this.printPackingSlipButton.UseVisualStyleBackColor = false;
            // 
            // topOrderNumberTextbox
            // 
            this.topOrderNumberTextbox.BackColor = System.Drawing.Color.White;
            this.topOrderNumberTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.topOrderNumberTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.topOrderNumberTextbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(154)))), ((int)(((byte)(192)))));
            this.topOrderNumberTextbox.Location = new System.Drawing.Point(199, 20);
            this.topOrderNumberTextbox.MaxLength = 20;
            this.topOrderNumberTextbox.Name = "topOrderNumberTextbox";
            this.topOrderNumberTextbox.ReadOnly = true;
            this.topOrderNumberTextbox.Size = new System.Drawing.Size(148, 16);
            this.topOrderNumberTextbox.TabIndex = 5;
            // 
            // topStatusLabel
            // 
            this.topStatusLabel.AutoSize = true;
            this.topStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.topStatusLabel.Location = new System.Drawing.Point(149, 40);
            this.topStatusLabel.Name = "topStatusLabel";
            this.topStatusLabel.Size = new System.Drawing.Size(171, 17);
            this.topStatusLabel.TabIndex = 2;
            this.topStatusLabel.Text = "Status: Awaiting Shipment";
            // 
            // topOrderLabel
            // 
            this.topOrderLabel.AutoSize = true;
            this.topOrderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.topOrderLabel.Location = new System.Drawing.Point(149, 18);
            this.topOrderLabel.Name = "topOrderLabel";
            this.topOrderLabel.Size = new System.Drawing.Size(49, 17);
            this.topOrderLabel.TabIndex = 1;
            this.topOrderLabel.Text = "Order:";
            // 
            // logoPicturebox
            // 
            this.logoPicturebox.Image = ((System.Drawing.Image)(resources.GetObject("logoPicturebox.Image")));
            this.logoPicturebox.Location = new System.Drawing.Point(12, 12);
            this.logoPicturebox.Name = "logoPicturebox";
            this.logoPicturebox.Size = new System.Drawing.Size(120, 50);
            this.logoPicturebox.TabIndex = 0;
            this.logoPicturebox.TabStop = false;
            // 
            // recipientPanel
            // 
            this.recipientPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recipientPanel.BackColor = System.Drawing.Color.White;
            this.recipientPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.recipientPanel.Controls.Add(this.soldToPhoneTextbox);
            this.recipientPanel.Controls.Add(this.housePicturebox);
            this.recipientPanel.Controls.Add(this.verifyButton);
            this.recipientPanel.Controls.Add(this.shipToPhoneTextbox);
            this.recipientPanel.Controls.Add(this.verifyTextbox);
            this.recipientPanel.Controls.Add(this.shipToCombineTextbox);
            this.recipientPanel.Controls.Add(this.shipToAddress2Textbox);
            this.recipientPanel.Controls.Add(this.shipToAddress1Textbox);
            this.recipientPanel.Controls.Add(this.shipToNameTextbox);
            this.recipientPanel.Controls.Add(this.shipToLabel);
            this.recipientPanel.Controls.Add(this.soldToTextbox);
            this.recipientPanel.Controls.Add(this.soldToLabel);
            this.recipientPanel.Controls.Add(this.buyerLabel);
            this.recipientPanel.Location = new System.Drawing.Point(506, 104);
            this.recipientPanel.Name = "recipientPanel";
            this.recipientPanel.Size = new System.Drawing.Size(424, 257);
            this.recipientPanel.TabIndex = 2;
            // 
            // soldToPhoneTextbox
            // 
            this.soldToPhoneTextbox.BackColor = System.Drawing.Color.White;
            this.soldToPhoneTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.soldToPhoneTextbox.ForeColor = System.Drawing.Color.Gray;
            this.soldToPhoneTextbox.Location = new System.Drawing.Point(87, 75);
            this.soldToPhoneTextbox.MaxLength = 40;
            this.soldToPhoneTextbox.Name = "soldToPhoneTextbox";
            this.soldToPhoneTextbox.ReadOnly = true;
            this.soldToPhoneTextbox.Size = new System.Drawing.Size(206, 13);
            this.soldToPhoneTextbox.TabIndex = 24;
            // 
            // housePicturebox
            // 
            this.housePicturebox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.housePicturebox.Image = ((System.Drawing.Image)(resources.GetObject("housePicturebox.Image")));
            this.housePicturebox.Location = new System.Drawing.Point(326, 178);
            this.housePicturebox.Name = "housePicturebox";
            this.housePicturebox.Size = new System.Drawing.Size(54, 41);
            this.housePicturebox.TabIndex = 23;
            this.housePicturebox.TabStop = false;
            // 
            // verifyButton
            // 
            this.verifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.verifyButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.verifyButton.FlatAppearance.BorderSize = 2;
            this.verifyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verifyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(147)))), ((int)(((byte)(208)))));
            this.verifyButton.Location = new System.Drawing.Point(303, 121);
            this.verifyButton.Name = "verifyButton";
            this.verifyButton.Size = new System.Drawing.Size(106, 32);
            this.verifyButton.TabIndex = 22;
            this.verifyButton.Text = "Verify Address";
            this.verifyButton.UseVisualStyleBackColor = true;
            this.verifyButton.Click += new System.EventHandler(this.verifyButton_Click);
            // 
            // shipToPhoneTextbox
            // 
            this.shipToPhoneTextbox.BackColor = System.Drawing.Color.White;
            this.shipToPhoneTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipToPhoneTextbox.ForeColor = System.Drawing.Color.Gray;
            this.shipToPhoneTextbox.Location = new System.Drawing.Point(87, 197);
            this.shipToPhoneTextbox.MaxLength = 40;
            this.shipToPhoneTextbox.Name = "shipToPhoneTextbox";
            this.shipToPhoneTextbox.ReadOnly = true;
            this.shipToPhoneTextbox.Size = new System.Drawing.Size(206, 13);
            this.shipToPhoneTextbox.TabIndex = 21;
            // 
            // verifyTextbox
            // 
            this.verifyTextbox.BackColor = System.Drawing.Color.White;
            this.verifyTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.verifyTextbox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(168)))), ((int)(((byte)(17)))));
            this.verifyTextbox.Location = new System.Drawing.Point(87, 216);
            this.verifyTextbox.MaxLength = 20;
            this.verifyTextbox.Name = "verifyTextbox";
            this.verifyTextbox.ReadOnly = true;
            this.verifyTextbox.Size = new System.Drawing.Size(116, 13);
            this.verifyTextbox.TabIndex = 20;
            // 
            // shipToCombineTextbox
            // 
            this.shipToCombineTextbox.BackColor = System.Drawing.Color.White;
            this.shipToCombineTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipToCombineTextbox.ForeColor = System.Drawing.Color.Gray;
            this.shipToCombineTextbox.Location = new System.Drawing.Point(87, 178);
            this.shipToCombineTextbox.MaxLength = 40;
            this.shipToCombineTextbox.Name = "shipToCombineTextbox";
            this.shipToCombineTextbox.ReadOnly = true;
            this.shipToCombineTextbox.Size = new System.Drawing.Size(206, 13);
            this.shipToCombineTextbox.TabIndex = 19;
            // 
            // shipToAddress2Textbox
            // 
            this.shipToAddress2Textbox.BackColor = System.Drawing.Color.White;
            this.shipToAddress2Textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipToAddress2Textbox.ForeColor = System.Drawing.Color.Gray;
            this.shipToAddress2Textbox.Location = new System.Drawing.Point(87, 159);
            this.shipToAddress2Textbox.MaxLength = 40;
            this.shipToAddress2Textbox.Name = "shipToAddress2Textbox";
            this.shipToAddress2Textbox.ReadOnly = true;
            this.shipToAddress2Textbox.Size = new System.Drawing.Size(206, 13);
            this.shipToAddress2Textbox.TabIndex = 18;
            // 
            // shipToAddress1Textbox
            // 
            this.shipToAddress1Textbox.BackColor = System.Drawing.Color.White;
            this.shipToAddress1Textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipToAddress1Textbox.ForeColor = System.Drawing.Color.Gray;
            this.shipToAddress1Textbox.Location = new System.Drawing.Point(87, 140);
            this.shipToAddress1Textbox.MaxLength = 40;
            this.shipToAddress1Textbox.Name = "shipToAddress1Textbox";
            this.shipToAddress1Textbox.ReadOnly = true;
            this.shipToAddress1Textbox.Size = new System.Drawing.Size(206, 13);
            this.shipToAddress1Textbox.TabIndex = 17;
            // 
            // shipToNameTextbox
            // 
            this.shipToNameTextbox.BackColor = System.Drawing.Color.White;
            this.shipToNameTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.shipToNameTextbox.ForeColor = System.Drawing.Color.Gray;
            this.shipToNameTextbox.Location = new System.Drawing.Point(87, 121);
            this.shipToNameTextbox.MaxLength = 40;
            this.shipToNameTextbox.Name = "shipToNameTextbox";
            this.shipToNameTextbox.ReadOnly = true;
            this.shipToNameTextbox.Size = new System.Drawing.Size(206, 13);
            this.shipToNameTextbox.TabIndex = 16;
            // 
            // shipToLabel
            // 
            this.shipToLabel.AutoSize = true;
            this.shipToLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shipToLabel.Location = new System.Drawing.Point(30, 121);
            this.shipToLabel.Name = "shipToLabel";
            this.shipToLabel.Size = new System.Drawing.Size(51, 13);
            this.shipToLabel.TabIndex = 15;
            this.shipToLabel.Text = "Ship To";
            // 
            // soldToTextbox
            // 
            this.soldToTextbox.BackColor = System.Drawing.Color.White;
            this.soldToTextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.soldToTextbox.ForeColor = System.Drawing.Color.Gray;
            this.soldToTextbox.Location = new System.Drawing.Point(87, 56);
            this.soldToTextbox.MaxLength = 40;
            this.soldToTextbox.Name = "soldToTextbox";
            this.soldToTextbox.ReadOnly = true;
            this.soldToTextbox.Size = new System.Drawing.Size(206, 13);
            this.soldToTextbox.TabIndex = 14;
            // 
            // soldToLabel
            // 
            this.soldToLabel.AutoSize = true;
            this.soldToLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.soldToLabel.Location = new System.Drawing.Point(30, 65);
            this.soldToLabel.Name = "soldToLabel";
            this.soldToLabel.Size = new System.Drawing.Size(51, 13);
            this.soldToLabel.TabIndex = 8;
            this.soldToLabel.Text = "Sold To";
            // 
            // buyerLabel
            // 
            this.buyerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buyerLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.buyerLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buyerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buyerLabel.Location = new System.Drawing.Point(0, 0);
            this.buyerLabel.Name = "buyerLabel";
            this.buyerLabel.Size = new System.Drawing.Size(423, 35);
            this.buyerLabel.TabIndex = 1;
            this.buyerLabel.Text = "Buyer / Recipient Info";
            this.buyerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listview
            // 
            this.listview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listview.CheckBoxes = true;
            this.listview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.selectionHeader,
            this.itemHeader,
            this.unitHeader,
            this.qtyHeader,
            this.totalHeader,
            this.cancelHeader,
            this.reasonHeader});
            this.listview.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listview.Location = new System.Drawing.Point(13, 382);
            this.listview.Name = "listview";
            this.listview.Size = new System.Drawing.Size(917, 459);
            this.listview.TabIndex = 3;
            this.listview.UseCompatibleStateImageBehavior = false;
            this.listview.View = System.Windows.Forms.View.Details;
            this.listview.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listview_ColumnWidthChanging);
            // 
            // selectionHeader
            // 
            this.selectionHeader.Text = "";
            this.selectionHeader.Width = 24;
            // 
            // itemHeader
            // 
            this.itemHeader.Text = "Item";
            this.itemHeader.Width = 347;
            // 
            // unitHeader
            // 
            this.unitHeader.Text = "Unit $";
            this.unitHeader.Width = 76;
            // 
            // qtyHeader
            // 
            this.qtyHeader.Text = "Qty";
            this.qtyHeader.Width = 47;
            // 
            // totalHeader
            // 
            this.totalHeader.Text = "Total $";
            this.totalHeader.Width = 87;
            // 
            // cancelHeader
            // 
            this.cancelHeader.Text = "Status";
            this.cancelHeader.Width = 82;
            // 
            // reasonHeader
            // 
            this.reasonHeader.Text = "";
            this.reasonHeader.Width = 140;
            // 
            // selectAllCheckbox
            // 
            this.selectAllCheckbox.AutoSize = true;
            this.selectAllCheckbox.Location = new System.Drawing.Point(17, 387);
            this.selectAllCheckbox.Name = "selectAllCheckbox";
            this.selectAllCheckbox.Size = new System.Drawing.Size(15, 14);
            this.selectAllCheckbox.TabIndex = 4;
            this.selectAllCheckbox.UseVisualStyleBackColor = true;
            this.selectAllCheckbox.CheckedChanged += new System.EventHandler(this.selectAllCheckbox_CheckedChanged);
            // 
            // markCancelButton
            // 
            this.markCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.markCancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(126)))), ((int)(((byte)(116)))));
            this.markCancelButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.markCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.markCancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.markCancelButton.ForeColor = System.Drawing.Color.White;
            this.markCancelButton.Location = new System.Drawing.Point(801, 382);
            this.markCancelButton.Name = "markCancelButton";
            this.markCancelButton.Size = new System.Drawing.Size(129, 23);
            this.markCancelButton.TabIndex = 5;
            this.markCancelButton.Text = "Mark as Cancelled";
            this.markCancelButton.UseVisualStyleBackColor = false;
            this.markCancelButton.Click += new System.EventHandler(this.markCancelButton_Click);
            // 
            // reasonCombobox
            // 
            this.reasonCombobox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.reasonCombobox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.reasonCombobox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.reasonCombobox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.reasonCombobox.DropDownWidth = 170;
            this.reasonCombobox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reasonCombobox.FormattingEnabled = true;
            this.reasonCombobox.Items.AddRange(new object[] {
            "Select Reason",
            "Incoorect Ship To Address",
            "Incorrect SKU",
            "Cancelled at Merchant\'s Request",
            "Cannot fulfill the order in time",
            "Cannot Ship as Ordered",
            "Invalid Item Cost",
            "Merchant detected fraud",
            "Order missing information",
            "Out of Stock",
            "Product Has Been Discontinued",
            "Other"});
            this.reasonCombobox.Location = new System.Drawing.Point(806, 441);
            this.reasonCombobox.Name = "reasonCombobox";
            this.reasonCombobox.Size = new System.Drawing.Size(121, 21);
            this.reasonCombobox.TabIndex = 6;
            this.reasonCombobox.Text = "Select Reason";
            this.reasonCombobox.Visible = false;
            // 
            // setReasonButton
            // 
            this.setReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.setReasonButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(168)))), ((int)(((byte)(17)))));
            this.setReasonButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.setReasonButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.setReasonButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setReasonButton.ForeColor = System.Drawing.Color.White;
            this.setReasonButton.Location = new System.Drawing.Point(817, 483);
            this.setReasonButton.Name = "setReasonButton";
            this.setReasonButton.Size = new System.Drawing.Size(98, 23);
            this.setReasonButton.TabIndex = 7;
            this.setReasonButton.Text = "Set Reason";
            this.setReasonButton.UseVisualStyleBackColor = false;
            this.setReasonButton.Visible = false;
            this.setReasonButton.Click += new System.EventHandler(this.setReasonButton_Click);
            // 
            // DetailPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1436, 873);
            this.Controls.Add(this.setReasonButton);
            this.Controls.Add(this.reasonCombobox);
            this.Controls.Add(this.markCancelButton);
            this.Controls.Add(this.selectAllCheckbox);
            this.Controls.Add(this.listview);
            this.Controls.Add(this.recipientPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.orderSummaryPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DetailPage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Order Details";
            this.orderSummaryPanel.ResumeLayout(false);
            this.orderSummaryPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.moneyPicturebox)).EndInit();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPicturebox)).EndInit();
            this.recipientPanel.ResumeLayout(false);
            this.recipientPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.housePicturebox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel orderSummaryPanel;
        private System.Windows.Forms.Label orderSummaryLabel;
        private System.Windows.Forms.TextBox shipByDateTextbox;
        private System.Windows.Forms.TextBox paidDateTextbox;
        private System.Windows.Forms.TextBox orderDateTextbox;
        private System.Windows.Forms.Label shipByDateLabel;
        private System.Windows.Forms.Label paidDateLabel;
        private System.Windows.Forms.Label orderDateLabel;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.PictureBox logoPicturebox;
        private System.Windows.Forms.TextBox topOrderNumberTextbox;
        private System.Windows.Forms.Label topStatusLabel;
        private System.Windows.Forms.Label topOrderLabel;
        private System.Windows.Forms.Button printPackingSlipButton;
        private System.Windows.Forms.Label hstgstLabel;
        private System.Windows.Forms.Label unitPriceTotalLabel;
        private System.Windows.Forms.Label pstLabel;
        private System.Windows.Forms.TextBox unitPriceTotalTextbox;
        private System.Windows.Forms.TextBox gsthstTextbox;
        private System.Windows.Forms.TextBox pstTextbox;
        private System.Windows.Forms.TextBox otherFeeTextbox;
        private System.Windows.Forms.Label otherFeeLabel;
        private System.Windows.Forms.Label totalLabel;
        private System.Windows.Forms.TextBox totalOrderTextbox;
        private System.Windows.Forms.Panel recipientPanel;
        private System.Windows.Forms.Label buyerLabel;
        private System.Windows.Forms.Label soldToLabel;
        private System.Windows.Forms.TextBox shipToCombineTextbox;
        private System.Windows.Forms.TextBox shipToAddress2Textbox;
        private System.Windows.Forms.TextBox shipToAddress1Textbox;
        private System.Windows.Forms.TextBox shipToNameTextbox;
        private System.Windows.Forms.Label shipToLabel;
        private System.Windows.Forms.TextBox soldToTextbox;
        private System.Windows.Forms.TextBox verifyTextbox;
        private System.Windows.Forms.TextBox shipToPhoneTextbox;
        private System.Windows.Forms.Button verifyButton;
        private System.Windows.Forms.PictureBox moneyPicturebox;
        private System.Windows.Forms.PictureBox housePicturebox;
        private System.Windows.Forms.TextBox soldToPhoneTextbox;
        private System.Windows.Forms.ListView listview;
        private System.Windows.Forms.ColumnHeader selectionHeader;
        private System.Windows.Forms.ColumnHeader itemHeader;
        private System.Windows.Forms.ColumnHeader unitHeader;
        private System.Windows.Forms.ColumnHeader qtyHeader;
        private System.Windows.Forms.ColumnHeader totalHeader;
        private System.Windows.Forms.CheckBox selectAllCheckbox;
        private System.Windows.Forms.ColumnHeader cancelHeader;
        private System.Windows.Forms.Button markCancelButton;
        private System.Windows.Forms.ColumnHeader reasonHeader;
        private System.Windows.Forms.ComboBox reasonCombobox;
        private System.Windows.Forms.Button setReasonButton;
    }
}