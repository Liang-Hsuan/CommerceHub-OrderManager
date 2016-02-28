namespace CommerceHub_OrderManager
{
    partial class Main
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.weeklyOverviewLabel = new System.Windows.Forms.Label();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.listview = new System.Windows.Forms.ListView();
            this.selectionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ageHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.itemNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.itemSkuHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.transactionNumberHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.orderDateHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.orderTotalHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.qtyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recipientHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.createLabelButton = new System.Windows.Forms.Button();
            this.printButton = new System.Windows.Forms.Button();
            this.detailButton = new System.Windows.Forms.Button();
            this.selectionAllCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            this.SuspendLayout();
            // 
            // weeklyOverviewLabel
            // 
            this.weeklyOverviewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.weeklyOverviewLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.weeklyOverviewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weeklyOverviewLabel.ForeColor = System.Drawing.Color.White;
            this.weeklyOverviewLabel.Location = new System.Drawing.Point(12, 9);
            this.weeklyOverviewLabel.Name = "weeklyOverviewLabel";
            this.weeklyOverviewLabel.Size = new System.Drawing.Size(1412, 54);
            this.weeklyOverviewLabel.TabIndex = 0;
            this.weeklyOverviewLabel.Text = "Weekly Overview";
            this.weeklyOverviewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chart
            // 
            this.chart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.chart.BackSecondaryColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.chart.BorderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.chart.BorderSkin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            this.chart.BorderSkin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(82)))), ((int)(((byte)(124)))));
            this.chart.BorderSkin.BorderWidth = 2;
            this.chart.BorderSkin.PageColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            chartArea1.AxisY.Title = "# of Orders";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            chartArea1.BorderColor = System.Drawing.Color.LightGray;
            chartArea1.Name = "ChartArea";
            this.chart.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(238)))), ((int)(((byte)(247)))));
            legend1.Name = "Legend";
            this.chart.Legends.Add(legend1);
            this.chart.Location = new System.Drawing.Point(12, 63);
            this.chart.Name = "chart";
            series1.BorderColor = System.Drawing.Color.Black;
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(100)))), ((int)(((byte)(158)))));
            series1.Legend = "Legend";
            series1.Name = "orders";
            series1.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            series1.ShadowOffset = 1;
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "ChartArea";
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            series2.Legend = "Legend";
            series2.MarkerSize = 2;
            series2.Name = "shipment";
            series3.BorderWidth = 4;
            series3.ChartArea = "ChartArea";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series3.Color = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(100)))), ((int)(((byte)(158)))));
            series3.IsVisibleInLegend = false;
            series3.Legend = "Legend";
            series3.Name = "dataPoint";
            this.chart.Series.Add(series1);
            this.chart.Series.Add(series2);
            this.chart.Series.Add(series3);
            this.chart.Size = new System.Drawing.Size(1412, 255);
            this.chart.TabIndex = 1;
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
            this.ageHeader,
            this.itemNameHeader,
            this.itemSkuHeader,
            this.transactionNumberHeader,
            this.orderDateHeader,
            this.orderTotalHeader,
            this.qtyHeader,
            this.recipientHeader});
            this.listview.FullRowSelect = true;
            this.listview.GridLines = true;
            this.listview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listview.Location = new System.Drawing.Point(12, 378);
            this.listview.Name = "listview";
            this.listview.Size = new System.Drawing.Size(1412, 481);
            this.listview.TabIndex = 2;
            this.listview.UseCompatibleStateImageBehavior = false;
            this.listview.View = System.Windows.Forms.View.Details;
            // 
            // selectionHeader
            // 
            this.selectionHeader.Text = "";
            this.selectionHeader.Width = 76;
            // 
            // ageHeader
            // 
            this.ageHeader.Text = "Age";
            this.ageHeader.Width = 93;
            // 
            // itemNameHeader
            // 
            this.itemNameHeader.Text = "Item Name";
            this.itemNameHeader.Width = 270;
            // 
            // itemSkuHeader
            // 
            this.itemSkuHeader.Text = "Item SKU";
            this.itemSkuHeader.Width = 198;
            // 
            // transactionNumberHeader
            // 
            this.transactionNumberHeader.Text = "Transaction ID #";
            this.transactionNumberHeader.Width = 194;
            // 
            // orderDateHeader
            // 
            this.orderDateHeader.Text = "Order Date";
            this.orderDateHeader.Width = 129;
            // 
            // orderTotalHeader
            // 
            this.orderTotalHeader.Text = "Order Total";
            this.orderTotalHeader.Width = 137;
            // 
            // qtyHeader
            // 
            this.qtyHeader.Text = "Qty";
            this.qtyHeader.Width = 84;
            // 
            // recipientHeader
            // 
            this.recipientHeader.Text = "Recipient";
            this.recipientHeader.Width = 235;
            // 
            // createLabelButton
            // 
            this.createLabelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(168)))), ((int)(((byte)(17)))));
            this.createLabelButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.createLabelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.createLabelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createLabelButton.ForeColor = System.Drawing.Color.White;
            this.createLabelButton.Location = new System.Drawing.Point(12, 338);
            this.createLabelButton.Name = "createLabelButton";
            this.createLabelButton.Size = new System.Drawing.Size(160, 34);
            this.createLabelButton.TabIndex = 3;
            this.createLabelButton.Text = "Create Lable(s)";
            this.createLabelButton.UseVisualStyleBackColor = false;
            // 
            // printButton
            // 
            this.printButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.printButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.printButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.printButton.Location = new System.Drawing.Point(178, 338);
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(106, 34);
            this.printButton.TabIndex = 4;
            this.printButton.Text = "Print";
            this.printButton.UseVisualStyleBackColor = false;
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // detailButton
            // 
            this.detailButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.detailButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(126)))), ((int)(((byte)(116)))));
            this.detailButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.detailButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.detailButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailButton.ForeColor = System.Drawing.Color.White;
            this.detailButton.Location = new System.Drawing.Point(1264, 340);
            this.detailButton.Name = "detailButton";
            this.detailButton.Size = new System.Drawing.Size(160, 34);
            this.detailButton.TabIndex = 5;
            this.detailButton.Text = "More Detailes";
            this.detailButton.UseVisualStyleBackColor = false;
            this.detailButton.Click += new System.EventHandler(this.detailButton_Click);
            // 
            // selectionAllCheckbox
            // 
            this.selectionAllCheckbox.AutoSize = true;
            this.selectionAllCheckbox.Location = new System.Drawing.Point(16, 383);
            this.selectionAllCheckbox.Name = "selectionAllCheckbox";
            this.selectionAllCheckbox.Size = new System.Drawing.Size(15, 14);
            this.selectionAllCheckbox.TabIndex = 6;
            this.selectionAllCheckbox.UseVisualStyleBackColor = true;
            this.selectionAllCheckbox.CheckedChanged += new System.EventHandler(this.selectionAllCheckbox_CheckedChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(245)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1436, 873);
            this.Controls.Add(this.selectionAllCheckbox);
            this.Controls.Add(this.detailButton);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.createLabelButton);
            this.Controls.Add(this.listview);
            this.Controls.Add(this.chart);
            this.Controls.Add(this.weeklyOverviewLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CommerceHub Order Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label weeklyOverviewLabel;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private System.Windows.Forms.ListView listview;
        private System.Windows.Forms.ColumnHeader ageHeader;
        private System.Windows.Forms.ColumnHeader itemNameHeader;
        private System.Windows.Forms.ColumnHeader itemSkuHeader;
        private System.Windows.Forms.ColumnHeader transactionNumberHeader;
        private System.Windows.Forms.ColumnHeader orderDateHeader;
        private System.Windows.Forms.ColumnHeader orderTotalHeader;
        private System.Windows.Forms.ColumnHeader qtyHeader;
        private System.Windows.Forms.ColumnHeader recipientHeader;
        private System.Windows.Forms.Button createLabelButton;
        private System.Windows.Forms.Button printButton;
        private System.Windows.Forms.Button detailButton;
        private System.Windows.Forms.ColumnHeader selectionHeader;
        private System.Windows.Forms.CheckBox selectionAllCheckbox;
    }
}

