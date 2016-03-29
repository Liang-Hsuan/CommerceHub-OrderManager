using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace Order_Manager.channel.shop.ca
{
    /*
     * A class that generate the packing slip for shop.ca order
     */
    public class ShopCaPackingSlip
    {
        // boolearn flag to track error
        public bool Error { get; private set; }

        /* get the save path of the sears packing slip */
        public string SavePath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\ShopCa_PackingSlip";

        /* a method that save the packing slip pdf */
        public void createPackingSlip(ShopCaValues value, int[] cancelIndex, bool preview)
        {
            // set error to false
            Error = false;

            // the case if all of the items in the order are cancelled -> don't need to print the packing slip
            if (cancelIndex.Length >= value.OrderItemId.Count)
                return;

            // first check if the save directory exist -> if not create it
            if (!File.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            // initialize fields
            Document doc = new Document(PageSize.LETTER, 0, 0, 0, 0);
            string file = SavePath + "\\" + value.OrderId + ".pdf"; ;
            PdfWriter writer;
            try
            {
                writer = PdfWriter.GetInstance(doc, new FileStream(file, FileMode.Create));
            }
            catch
            {
                Error = true;
                return;
            }

            // open the documents
            doc.Open();
            PdfContentByte contentByte = writer.DirectContent;

            #region Logo and Lines Set Up
            // add shop.ca logo
            Image logo = Image.GetInstance(@"..\..\image\shopCa.jpg");
            logo.ScalePercent(15f);
            logo.SetAbsolutePosition(40f, doc.PageSize.Height - 60f);
            doc.Add(logo);

            // drawline - horizontal
            PdfContentByte draw = writer.DirectContent;
            draw.MoveTo(40f, doc.PageSize.Height - 165f);
            draw.LineTo(doc.PageSize.Width - 40f, doc.PageSize.Height - 165f);
            draw.Stroke();

            // drawline - vertical
            draw.MoveTo(doc.PageSize.Width - 180f, doc.PageSize.Height - 30f);
            draw.LineTo(doc.PageSize.Width - 180f, doc.PageSize.Height - 155f);
            draw.Stroke();
            #endregion

            // initialize local fields for text
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            BaseFont boldFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            ColumnText ct = new ColumnText(draw);

            #region Ship To
            // set ship to
            // title
            Phrase text = new Phrase("Ship To:", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 40f, 680f, 150f, 700f, 0f, Element.ALIGN_LEFT);
            ct.Go();

            // ship to address
            text = new Phrase(value.ShipTo.Name + "\n" + value.ShipTo.Address1 + "\n" + value.ShipTo.Address2 + "\n" + value.ShipTo.City + ' ' + value.ShipTo.State + " , CA\n" + value.ShipTo.PostalCode, 
                   new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 40f, 630f, 175f, 700f, 12f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Top Right
            // order info
            // field names
            text = new Phrase("From:\n\n\n\n\nOrder#:\nOrder Placed Date:", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 310f, 655f, 425f, 760f, 15f, Element.ALIGN_RIGHT);
            ct.Go();

            // from
            text = new Phrase("SHOP.CA", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 439f, 729f, 489f, 744f, 0f, Element.ALIGN_LEFT);
            ct.Go();
            text = new Phrase("70 Peter Street, 2nd Floor\nToronto, Ontario, Canada, M5V 2G5\n1-855-4-SHOPCA", new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 439f, 672.5f, 559f, 742.5f, 12f, Element.ALIGN_LEFT);
            ct.Go();

            // order number & order placed date
            text = new Phrase(value.OrderId + "\n" + value.OrderCreateDate.ToString("yyyy-MM-dd"), new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 439f, 655f, 509f, 685f, 15f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            // words for customers
            text = new Phrase("Thank you for shopping with SHOP.CA! With thousands of premium brands, free shipping and free returns on most products, and " +
                              "Aeroplan Miles* earned on your purchases, SHOP.CA is the convenient, rewarding, and Canadian way to shop. We continue to add " +
                              "new brands and incredible new products every day at SHOP.CA, and we hope to see you back soon", new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 40f, 565f, doc.PageSize.Width - 40f, 615f, 11f, Element.ALIGN_LEFT);
            ct.Go();

            #region Order Title
            // drawline for order title
            draw.SetLineWidth(0.25f);
            draw.MoveTo(40f, 550f);
            draw.LineTo(198f, 550f);
            draw.Stroke();
            draw.MoveTo(40f, 550f);
            draw.LineTo(40f, 525f);
            draw.Stroke();
            draw.MoveTo(40f, 525f);
            draw.LineTo(198f, 525f);
            draw.Stroke();
            draw.MoveTo(198f, 525f);
            draw.LineTo(198f, 550f);
            draw.Stroke();

            // order title
            text = new Phrase("ORDER PACKING LIST", new Font(baseFont, 13));
            ct.SetSimpleColumn(text, 46f, 512f, 196, 532f, 0f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Order Column
            // draw box for order column names
            draw.MoveTo(40f, 520f);
            draw.LineTo(doc.PageSize.Width - 40f, 520f);
            draw.Stroke();
            draw.MoveTo(40f, 520f);
            draw.LineTo(40f, 495f);
            draw.Stroke();
            draw.MoveTo(233f, 520f);
            draw.LineTo(233f, 495f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 88f, 520f);
            draw.LineTo(doc.PageSize.Width - 88f, 495f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 520f);
            draw.LineTo(doc.PageSize.Width - 40f, 495f);
            draw.Stroke();
            draw.MoveTo(40f, 495f);
            draw.LineTo(doc.PageSize.Width - 40f, 495f);
            draw.Stroke();

            text = new Phrase("SKU                                                                                TITLE                                                    QTY"
                 , new Font(baseFont, 10f));
            ct.SetSimpleColumn(text, 120f, 483f, doc.PageSize.Width - 43f, 503f, 0f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Order Item
            float height = 495f;

            // adding items
            for (int i = 0; i < value.OrderItemId.Count; i++)
            {
                // boolean flag to check if the item index is cancelled or not
                bool found = false;

                // check if the item is cancelled
                foreach (int j in cancelIndex)
                {
                    if (i == j)
                        found = true;
                }

                // if the item is cancelled, skip this item
                if (found) continue;

                // draw box
                draw.MoveTo(40f, height);
                draw.LineTo(40f, height - 25f);
                draw.Stroke();
                draw.MoveTo(233f, height);
                draw.LineTo(233f, height - 25f);
                draw.Stroke();
                draw.MoveTo(doc.PageSize.Width - 88f, height);
                draw.LineTo(doc.PageSize.Width - 88f, height - 25f);
                draw.Stroke();
                draw.MoveTo(doc.PageSize.Width - 40f, height);
                draw.LineTo(doc.PageSize.Width - 40f, height - 25f);
                draw.Stroke();
                draw.MoveTo(40f, height - 25);
                draw.LineTo(doc.PageSize.Width - 40f, height - 25f);
                draw.Stroke();

                // sku
                text = new Phrase(value.Sku[i], new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 100f, height - 25, 200, height - 15, 0f, Element.ALIGN_LEFT);
                ct.Go();

                // title
                text = new Phrase(value.Title[i], new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 238f, height - 25, doc.PageSize.Width - 88f, height - 15, 0f, Element.ALIGN_LEFT);
                ct.Go();

                // title
                text = new Phrase(value.Quantity[i].ToString(), new Font(baseFont, 10));
                ct.SetSimpleColumn(text, doc.PageSize.Width - 70f, height - 25, doc.PageSize.Width - 40f, height - 15, 0f, Element.ALIGN_LEFT);
                ct.Go();

                // decrease height for next item
                height -= 25f;
            }
            #endregion

            #region Bottom Left Box
            // draw box
            draw.MoveTo(40f, 220f);
            draw.LineTo(doc.PageSize.Width / 2 - 15f, 220f);
            draw.Stroke();
            draw.MoveTo(40f, 220f);
            draw.LineTo(40f, 90f);
            draw.Stroke();
            draw.MoveTo(40f, 90f);
            draw.LineTo(doc.PageSize.Width / 2 - 15f, 90f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width / 2 - 15f, 90f);
            draw.LineTo(doc.PageSize.Width / 2 - 15f, 220f);
            draw.Stroke();

            // title
            text = new Phrase("If you are missing items from your order.", new Font(boldFont, 8));
            ct.SetSimpleColumn(text, 47f, 185f, doc.PageSize.Width / 2 - 22f, 205f, 0f, Element.ALIGN_LEFT);
            ct.Go();

            // content
            text = new Phrase("SHOP.CA is a premium marketplace that allows you to purchase products " +
                              "from hundreds of different merchant partners. If your order includes more " +
                              "than one type of product, it is very likely for items to ship at different times " +
                              "and arrive in separate boxes. You can view any outstanding items by logging " +
                              "into your SHOP.CA Member Account at any time, or by visiting " +
                              "\"Trac Order\" from the menu on the SHOP.CA homepage." +
                              "If you received damaged, defactive or incorrectly shipped merchandise, " +
                              "please contact our Customer Loyalty Team within 48 hours of receipt.", new Font(baseFont, 7));
            ct.SetSimpleColumn(text, 47f, 60f, doc.PageSize.Width / 2 - 22f, 200f, 11f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Bottom Right Box
            // draw box
            draw.MoveTo(doc.PageSize.Width / 2 + 15f, 220f);
            draw.LineTo(doc.PageSize.Width - 40f, 220f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width / 2 + 15f, 220f);
            draw.LineTo(doc.PageSize.Width / 2 + 15f, 90f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width / 2 + 15f, 90f);
            draw.LineTo(doc.PageSize.Width - 40f, 90f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 90f);
            draw.LineTo(doc.PageSize.Width - 40f, 220f);
            draw.Stroke();

            // title
            text = new Phrase("Need to return somthing?", new Font(boldFont, 8));
            ct.SetSimpleColumn(text, doc.PageSize.Width / 2 + 22f, 185f, doc.PageSize.Width - 47f, 205f, 0f, Element.ALIGN_LEFT);
            ct.Go();

            // content
            text = new Phrase("Our Customer Loyalty Team is always heppy to help!" +
                              "With just a few easy steps your order can be on its way back to us for a refund:\n" +
                              "1. Check out our return policy to make sure your order qualifies: shop.ca/returns\n" +
                              "2. Make sure the item to be returned is in the same condition as it arrived, " +
                              "including all packaging and tags intact.\n" +
                              "3.Contact us at 1.855.4.SHOPCA or shop.ca/help with your order number " +
                              "and we'll help you set up the return, and send you a prepaid shipping label!"
                              , new Font(baseFont, 7));
            ct.SetSimpleColumn(text, doc.PageSize.Width / 2 + 22f, 60f, doc.PageSize.Width - 47f, 200f, 11f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            // additional words
            text = new Phrase("*®Aeroplan and the Aeroplan logo are registered trademarks of Aimia Canada Inc.", new Font(baseFont, 5));
            ct.SetSimpleColumn(text, doc.PageSize.Width / 2 + 15f, 60f, doc.PageSize.Width - 40f, 70f, 0f, Element.ALIGN_RIGHT);
            ct.Go();

            #region Most Bottom Box
            draw.MoveTo(40f, 62f);
            draw.LineTo(doc.PageSize.Width - 40f, 62f);
            draw.Stroke();
            draw.MoveTo(40f, 62f);
            draw.LineTo(40f, 47f);
            draw.Stroke();
            draw.MoveTo(40f, 47f);
            draw.LineTo(doc.PageSize.Width - 40f, 47f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 47f);
            draw.LineTo(doc.PageSize.Width - 40f, 62f);
            draw.Stroke();

            text = new Phrase("Have another question? Our Customer Loyalty Team is ready to help! Please contact us directly for all other inquiries at 1.855.4.SHOPCA (746722) or shop.ca/help.",
                   new Font(baseFont, 7));
            ct.SetSimpleColumn(text, 47f, 42f, doc.PageSize.Width - 47f, 52f, 0f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            if (preview)
            {
                // start the pdf for previewing
                if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                    System.Diagnostics.Process.Start(file);
            }

            doc.Close();
        }
    }
}
