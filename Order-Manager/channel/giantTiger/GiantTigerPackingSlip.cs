using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;

namespace Order_Manager.channel.giantTiger
{
    /*
     * A class that generate the packing slip for giant tiger order
     */
    public static class GiantTigerPackingSlip
    {
        /* get the save path of the sears packing slip */
        public static string SavePath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\GiantTiger_PackingSlip";

        /* a method that save the packing slip pdf */
        public static void CreatePackingSlip(GiantTigerValues value, int[] cancelIndex, bool preview)
        {
            // the case if all of the items in the order are cancelled -> don't need to print the packing slip
            if (cancelIndex.Length >= value.VendorSku.Count)
                return;

            // first check if the save directory exist -> if not create it
            if (!File.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            // initialize fields
            Document doc = new Document(PageSize.LETTER, 0, 0, 0, 0);
            string file = SavePath + "\\" + value.PoNumber + ".pdf";
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(file, FileMode.Create));

            // open the documents
            doc.Open();
            PdfContentByte contentByte = writer.DirectContent;
            PdfContentByte draw = writer.DirectContent;

            #region Logo and Barcode Set Up
            // add giant tiger logo
            Image logo = Image.GetInstance(@"..\..\image\giant tiger.jpg");
            logo.ScalePercent(4.5f);
            logo.SetAbsolutePosition(40f, doc.PageSize.Height - 100f);
            doc.Add(logo);

            // add barcode
            Barcode39 barcode128 = new Barcode39
            {
                Code = "19791104008",
                StartStopText = false,
                Font = null,
                Extended = true
            };

            Image image = barcode128.CreateImageWithBarcode(contentByte, BaseColor.BLACK, BaseColor.BLACK);
            image.ScaleAbsoluteHeight(40f);
            image.SetAbsolutePosition(340f, doc.PageSize.Height - 80f);
            contentByte.AddImage(image);
            #endregion

            // initialize local fields for text
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            BaseFont boldFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            ColumnText ct = new ColumnText(draw);

            #region Sold To
            // sold to
            Phrase text = new Phrase("SOLD TO / VENDU A", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 40f, 655f, 200f, 670f, 0f, Element.ALIGN_LEFT);
            ct.Go();

            // sold to address
            text = new Phrase(value.ShipTo.Name + '\n' + value.ShipTo.Address1 + '\n' + value.ShipTo.Address2 + '\n' + value.ShipTo.City + ", " + value.ShipTo.State + ' ' + value.ShipTo.PostalCode + "\nCanada"
                 , new Font(baseFont, 9));
            ct.SetSimpleColumn(text, 42f, 568f, 177f, 668f, 10f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Ship To
            // ship to
            text = new Phrase("SHIP TO / EXPEDIE A", new Font(boldFont, 10f));
            ct.SetSimpleColumn(text, 300f, 655f, 450f, 670f, 0f, Element.ALIGN_LEFT);
            ct.Go();

            // ship to address
            text = new Phrase(value.ShipTo.Name + '\n' + value.ShipTo.Address1 + '\n' + value.ShipTo.Address2 + '\n' + value.ShipTo.City + ", " + value.ShipTo.State + ' ' + value.ShipTo.PostalCode + "\nCanada"
                 , new Font(baseFont, 9f));
            ct.SetSimpleColumn(text, 302f, 568f, 447f, 668f, 10f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion

            #region Draw First Box
            draw.MoveTo(40f, 580f);
            draw.LineTo(doc.PageSize.Width - 40f, 580f);
            draw.Stroke();

            draw.MoveTo(doc.PageSize.Width - 40f, 580f);
            draw.LineTo(doc.PageSize.Width - 40f, 530f);
            draw.Stroke();

            draw.MoveTo(doc.PageSize.Width - 40f, 530f);
            draw.LineTo(40f, 530f);
            draw.Stroke();

            draw.MoveTo(40f, 530f);
            draw.LineTo(40f, 580f);
            draw.Stroke();

            draw.MoveTo(160f, 580f);
            draw.LineTo(160f, 530f);
            draw.Stroke();

            draw.MoveTo(280f, 580f);
            draw.LineTo(280f, 530f);
            draw.Stroke();

            draw.MoveTo(320f, 580f);
            draw.LineTo(320f, 530f);
            draw.Stroke();

            draw.MoveTo(500f, 580f);
            draw.LineTo(500f, 530f);
            draw.Stroke();

            draw.MoveTo(40f, 543f);
            draw.LineTo(doc.PageSize.Width - 40f, 543f);
            draw.Stroke();
            #endregion

            #region Messgae in First Box
            text = new Phrase("SHIPPING METHOD \\\nMODE D'EXPEDITION", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 40f, 550f, 160f, 580f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("ORDER DATE \\\nDATE DE LA\nCOMMANDE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 160f, 540f, 280f, 580f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("PAGE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 280f, 550f, 320f, 580f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("ORDER NUMBER \\\nNUMERO DE COMMANDE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 320f, 550f, 500f, 580f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("PO NUMBER \\\nBON DE\nCOMMANDE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 500f, 540f, doc.PageSize.Width - 40f, 580f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("Canada Post Ground", new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 40f, 523f, 160f, 533f, 0f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase(value.OrderDate.ToString("MM/dd/yyyy"), new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 160f, 523f, 280f, 533f, 0f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("1 of 1", new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 280f, 523f, 320f, 533f, 0f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase(value.WebOrderNo, new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 320f, 523f, 500f, 533f, 0f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase(value.PoNumber, new Font(baseFont, 10));
            ct.SetSimpleColumn(text, 500f, 523f, doc.PageSize.Width - 40f, 533f, 0f, Element.ALIGN_CENTER);
            ct.Go();
            #endregion

            #region Draw Second Box
            draw.MoveTo(40f, 520f);
            draw.LineTo(doc.PageSize.Width - 40f, 520f);
            draw.Stroke();

            draw.MoveTo(doc.PageSize.Width - 40f, 520f);
            draw.LineTo(doc.PageSize.Width - 40f, 483f);
            draw.Stroke();

            draw.MoveTo(doc.PageSize.Width - 40f, 483f);
            draw.LineTo(40f, 483f);
            draw.Stroke();

            draw.MoveTo(40f, 483f);
            draw.LineTo(40f, 520f);
            draw.Stroke();

            draw.MoveTo(130f, 520f);
            draw.LineTo(130f, 483f);
            draw.Stroke();

            draw.MoveTo(220f, 520f);
            draw.LineTo(220f, 483f);
            draw.Stroke();

            draw.MoveTo(400f, 520f);
            draw.LineTo(400f, 483f);
            draw.Stroke();

            draw.MoveTo(480f, 520f);
            draw.LineTo(480f, 483f);
            draw.Stroke();
            #endregion

            #region Message in Second Box
            text = new Phrase("QTY ORDERED \\\nQTE\nCOMMANDEE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 40f, 480f, 130f, 520f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("ITEM \\\nARTICLE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 130f, 480f, 220f, 520f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("DESCRIPTION", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 220f, 480f, 400f, 520f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("QTY SHIPPED \\\nQTE EXPEDIEE", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 400f, 480f, 480f, 520f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            text = new Phrase("VENDOR SKU \\\nNO D'ARTICLE DU\nFOURNISSEUR", new Font(boldFont, 10));
            ct.SetSimpleColumn(text, 480f, 480f, doc.PageSize.Width - 40f, 520f, 11f, Element.ALIGN_CENTER);
            ct.Go();

            // item addition
            draw.SetLineWidth(0.25f);
            float height = 480f;

            // adding items
            for (int i = 0; i < value.VendorSku.Count; i++)
            {
                // if the item is cancelled, skip this item
                if (cancelIndex.Any(j => i == j)) continue;

                // draw box
                draw.MoveTo(40f, height);
                draw.LineTo(40f, height - 10f);
                draw.Stroke();
                draw.MoveTo(40f, height - 10f);
                draw.LineTo(doc.PageSize.Width - 40f, height - 10f);
                draw.Stroke();
                draw.MoveTo(doc.PageSize.Width - 40f, height - 10f);
                draw.LineTo(doc.PageSize.Width - 40f, height);
                draw.Stroke();
                draw.MoveTo(130f, height);
                draw.LineTo(130f, height - 10f);
                draw.Stroke();
                draw.MoveTo(220f, height);
                draw.LineTo(220f, height - 10f);
                draw.Stroke();
                draw.MoveTo(400f, height);
                draw.LineTo(400f, height - 10f);
                draw.Stroke();
                draw.MoveTo(480f, height);
                draw.LineTo(480f, height - 10f);
                draw.Stroke();

                // qty
                text = new Phrase(value.Quantity[i].ToString(), new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 40f, height - 19f, 130, height - 9f, 0f, Element.ALIGN_CENTER);
                ct.Go();

                // item
                text = new Phrase(value.HostSku[i], new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 130f, height - 19f, 220f, height - 9f, 0f, Element.ALIGN_CENTER);
                ct.Go();

                // description
                text = new Phrase("", new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 220f, height - 19f, 400f, height - 9f, 0f, Element.ALIGN_CENTER);
                ct.Go();

                // qty shipped
                text = new Phrase(value.Quantity[i].ToString(), new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 400f, height - 19f, 480f, height - 9f, 0f, Element.ALIGN_CENTER);
                ct.Go();

                // vendor sku
                text = new Phrase(value.VendorSku[i], new Font(baseFont, 10));
                ct.SetSimpleColumn(text, 480f, height - 19f, doc.PageSize.Width - 40f, height - 9f, 0f, Element.ALIGN_CENTER);
                ct.Go();

                // decrease height for next item
                height -= 10f;
            }
            #endregion

            #region Ending Boxes
            #region Top Box
            // restore width
            draw.SetLineWidth(1f);

            // draw box
            draw.MoveTo(40f, 250f);
            draw.LineTo(doc.PageSize.Width - 40f, 250f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 250f);
            draw.LineTo(doc.PageSize.Width - 40f, 235f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 235f);
            draw.LineTo(40f, 235f);
            draw.Stroke();
            draw.MoveTo(40f, 235f);
            draw.LineTo(40f, 250f);
            draw.Stroke();

            // message in the box
            text = new Phrase("Thank you for ordering from Giant Tiger!        Merci d'avoir place une commande chez Tigre Geant!", new Font(boldFont, 10f));
            ct.SetSimpleColumn(text, 40f, 224f, doc.PageSize.Width - 40f, 239f, 0f, Element.ALIGN_CENTER);
            ct.Go();
            #endregion

            #region Bottom Box
            // draw box
            draw.MoveTo(40f, 225f);
            draw.LineTo(doc.PageSize.Width - 40f, 225f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 225f);
            draw.LineTo(doc.PageSize.Width - 40f, 50f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width - 40f, 50f);
            draw.LineTo(40f, 50f);
            draw.Stroke();
            draw.MoveTo(40f, 50f);
            draw.LineTo(40f, 225f);
            draw.Stroke();
            draw.MoveTo(doc.PageSize.Width / 2, 225f);
            draw.LineTo(doc.PageSize.Width / 2, 50f);
            draw.Stroke();

            // message in the left box
            text = new Phrase("Didn't receive your entire order or questions about your order?\n" +
                              "You may receive your order in separate shipments. To track your order status. Please log into My Account at gianttiger.com.\n\r" +
                              "Want to Return or Exchange an Item?\n" +
                              "If you are not satisfied with your order for any reason please contact our Customer Service Team at 1-844-99-GIANT (44268) or email webstorehelp@gianttiger.com. " +
                              "For in-store returns or exchanges, your shipment confirmation email is required. To review our return policy, please visit gianttiger.com/ReturnPolicy.",
                              new Font(baseFont, 9f));
            ct.SetSimpleColumn(text, 50f, 55f, doc.PageSize.Width / 2 - 10, 225f, 10f, Element.ALIGN_LEFT);
            ct.Go();

            // message in the right box
            text = new Phrase("Votre commande est incomplete ou vous avez des questions a son sujet?\n" +
                              "Il se peut que vous receviez votre commande dans des envois distincts. Pour suivre l'etat de votre commande, s'il vous plait vous connecter a Mon Compte au tigregeant.com.\n\r" +
                              "Want to Return or Exchange an Item?\n" +
                              "Vous voulez retourner ou echanger un article?\nSi vous n'etes pas satisfait avec votre achat, veuillez communiquer avec notre service a la clientele au 1-844-99-GIANT (44268) " +
                              "ou envoyer un courriel a aideachatenligne@tigregeant.com. Les echanges et les remboursements peuvent etre effectues a n'importe quel de nos magasins avec la presentation du courriel confirmant " +
                              "l'expedition de votre commande. Pour consulter notre politique de retour. s'il vous plait visitez tigregeant.com/PolitiqueRetour.",
                              new Font(baseFont, 9f));
            ct.SetSimpleColumn(text, doc.PageSize.Width / 2 + 10f, 55f, doc.PageSize.Width - 50f, 225f, 10f, Element.ALIGN_LEFT);
            ct.Go();
            #endregion
            #endregion

            doc.Close();

            // start the pdf for previewing
            if (System.Diagnostics.Process.GetProcessesByName(file).Length < 1)
                System.Diagnostics.Process.Start(file);
        }
    }
}
