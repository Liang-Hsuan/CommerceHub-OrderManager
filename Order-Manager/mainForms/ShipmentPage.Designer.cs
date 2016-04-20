namespace Order_Manager.mainForms
{
    partial class ShipmentPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipmentPage));
            this.listview = new System.Windows.Forms.ListView();
            this.selectionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.transactionIdHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.trackingNumberHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.IdentificationHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectAllCheckbox = new System.Windows.Forms.CheckBox();
            this.voidShipmentButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.endOfDayButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
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
            this.transactionIdHeader,
            this.trackingNumberHeader,
            this.IdentificationHeader});
            this.listview.Location = new System.Drawing.Point(11, 52);
            this.listview.Name = "listview";
            this.listview.Size = new System.Drawing.Size(575, 332);
            this.listview.TabIndex = 4;
            this.listview.UseCompatibleStateImageBehavior = false;
            this.listview.View = System.Windows.Forms.View.Details;
            this.listview.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listview_ColumnWidthChanging);
            // 
            // selectionHeader
            // 
            this.selectionHeader.Text = "";
            this.selectionHeader.Width = 107;
            // 
            // transactionIdHeader
            // 
            this.transactionIdHeader.Text = "Transaction ID";
            this.transactionIdHeader.Width = 129;
            // 
            // trackingNumberHeader
            // 
            this.trackingNumberHeader.Text = "Tracking Number";
            this.trackingNumberHeader.Width = 136;
            // 
            // IdentificationHeader
            // 
            this.IdentificationHeader.Text = "Identification";
            this.IdentificationHeader.Width = 203;
            // 
            // selectAllCheckbox
            // 
            this.selectAllCheckbox.AutoSize = true;
            this.selectAllCheckbox.Location = new System.Drawing.Point(15, 57);
            this.selectAllCheckbox.Name = "selectAllCheckbox";
            this.selectAllCheckbox.Size = new System.Drawing.Size(15, 14);
            this.selectAllCheckbox.TabIndex = 3;
            this.selectAllCheckbox.UseVisualStyleBackColor = true;
            this.selectAllCheckbox.CheckedChanged += new System.EventHandler(this.selectAllCheckbox_CheckedChanged);
            // 
            // voidShipmentButton
            // 
            this.voidShipmentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.voidShipmentButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(126)))), ((int)(((byte)(116)))));
            this.voidShipmentButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.voidShipmentButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.voidShipmentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.voidShipmentButton.ForeColor = System.Drawing.Color.White;
            this.voidShipmentButton.Location = new System.Drawing.Point(447, 12);
            this.voidShipmentButton.Name = "voidShipmentButton";
            this.voidShipmentButton.Size = new System.Drawing.Size(139, 34);
            this.voidShipmentButton.TabIndex = 2;
            this.voidShipmentButton.Text = "Void Shipment";
            this.voidShipmentButton.UseVisualStyleBackColor = false;
            this.voidShipmentButton.Click += new System.EventHandler(this.cancelShipmentButton_Click);
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.Transparent;
            this.backButton.FlatAppearance.BorderSize = 0;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backButton.Image = ((System.Drawing.Image)(resources.GetObject("backButton.Image")));
            this.backButton.Location = new System.Drawing.Point(11, 11);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(57, 34);
            this.backButton.TabIndex = 0;
            this.backButton.UseVisualStyleBackColor = false;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // endOfDayButton
            // 
            this.endOfDayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.endOfDayButton.BackColor = System.Drawing.Color.Transparent;
            this.endOfDayButton.FlatAppearance.BorderSize = 0;
            this.endOfDayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.endOfDayButton.Image = ((System.Drawing.Image)(resources.GetObject("endOfDayButton.Image")));
            this.endOfDayButton.Location = new System.Drawing.Point(394, 11);
            this.endOfDayButton.Name = "endOfDayButton";
            this.endOfDayButton.Size = new System.Drawing.Size(47, 34);
            this.endOfDayButton.TabIndex = 1;
            this.endOfDayButton.UseVisualStyleBackColor = false;
            this.endOfDayButton.Click += new System.EventHandler(this.endOfDayButton_Click);
            // 
            // ShipmentPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(600, 396);
            this.Controls.Add(this.endOfDayButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.voidShipmentButton);
            this.Controls.Add(this.selectAllCheckbox);
            this.Controls.Add(this.listview);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ShipmentPage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Daily Shipment";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listview;
        private System.Windows.Forms.ColumnHeader selectionHeader;
        private System.Windows.Forms.ColumnHeader transactionIdHeader;
        private System.Windows.Forms.ColumnHeader trackingNumberHeader;
        private System.Windows.Forms.CheckBox selectAllCheckbox;
        private System.Windows.Forms.Button voidShipmentButton;
        private System.Windows.Forms.ColumnHeader IdentificationHeader;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button endOfDayButton;
    }
}