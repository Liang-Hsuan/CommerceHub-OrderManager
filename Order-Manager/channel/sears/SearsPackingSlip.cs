using System;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Order_Manager.channel.sears
{
    /*
     * A class that generate the packing slip for sears order
     */
    public static class SearsPackingSlip
    {
        /* get the save path of the sears packing slip */
        public static string SavePath { get; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Sears_PackingSlip";

        /* a method that save the packing slip pdf */
        public static void createPackingSlip(SearsValues value, int[] cancelIndex, bool preview)
        {
            // the case if all of the items in the order are cancelled -> don't need to print the packing slip
            if (cancelIndex.Length >= value.LineCount)
                return;

            // first check if the save directory exist -> if not create it
            if (!File.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            // print each item for packing slip
            for (int i = 0; i < value.LineCount; i++)
            {
                // check if the item is in cancel list
                bool cancelled = cancelIndex.Any(j => i == j);

                // the case if the item is not cancelled -> generate and export it
                if (!cancelled)
                {
                    // initialize PdfWriter object
                    Document doc = new Document(PageSize.LETTER, 0, 0, 0, 35);
                    string file = SavePath + "\\" + value.TransactionID + "_" + (i + 1) + ".pdf";
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(file, FileMode.Create));

                    // open the document 
                    doc.Open();
                    PdfContentByte contentByte = writer.DirectContent;

                    #region Draw Line - Divided Half
                    // drawline
                    PdfContentByte draw = writer.DirectContent;
                    draw.MoveTo(doc.PageSize.Width / 2, 0);
                    draw.LineTo(doc.PageSize.Width / 2, doc.PageSize.Height);
                    draw.Stroke();
                    #endregion

                    // initialize local fields for text
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                    BaseFont boldFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                    ColumnText ct = new ColumnText(draw);

                    #region Shipping Address and Schedule
                    // set bill to
                    // title
                    Phrase text = new Phrase("BILL TO/FACTURER A:", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 10f, 753f, 150f, 773f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // bill recipient and address
                    text = new Phrase(value.BillTo.Name + "\n" + value.BillTo.Address1 + "\n" + value.BillTo.Address2 + "\n" + value.BillTo.City, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 726f, 150f, 771f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.BillTo.State + "\n" + value.BillTo.PostalCode, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 120f, 704f, 170f, 744f, 9f, Element.ALIGN_LEFT);
                    ct.Go();

                    // set ship to
                    // title
                    text = new Phrase("RECIPIENT/DESTINATAIRA:", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 10f, 693f, 150f, 713f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // ship recipient and address
                    text = new Phrase(value.Recipient.Name + "\n" + value.Recipient.Address1 + "\n" + value.Recipient.Address2 + "\n" + value.Recipient.City, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 666f, 150f, 711f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.Recipient.State + "\n" + value.Recipient.PostalCode, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 120f, 643f, 170f, 683f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.Recipient.DayPhone, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 645f, 150f, 655f, 0f, Element.ALIGN_LEFT);
                    ct.Go();


                    // arrival date
                    // title
                    text = new Phrase("ARRIVAL DATE/D'ARRIVEE", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 208f, 745f, 308f, 765f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    text = new Phrase(value.OrderDate.ToString("MM/dd/yy"), new Font(baseFont, 10));
                    ct.SetSimpleColumn(text, 226f, 745f, 276f, 755f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // schedule
                    text = new Phrase("SCHEDULE", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 227f, 727f, 312f, 737f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    text = new Phrase(value.CustOrderDate.ToString("MM/dd/yy"), new Font(baseFont, 10));
                    ct.SetSimpleColumn(text, 226f, 717f, 279f, 727f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    // sku description
                    text = new Phrase(value.Description[i] + "\n" + value.Description2[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 616f, 150f, 646f, 10f, Element.ALIGN_LEFT);
                    ct.Go();

                    #region Top Left Boxes
                    #region First Box
                    // upper and lower line for sku box
                    draw.MoveTo(15, 616);
                    draw.LineTo(118, 616);
                    draw.Stroke();
                    draw.MoveTo(15, 594);
                    draw.LineTo(118, 594);
                    draw.Stroke();

                    // straight lines for sku box
                    draw.MoveTo(15, 616);
                    draw.LineTo(15, 594);
                    draw.Stroke();
                    draw.MoveTo(38, 616);
                    draw.LineTo(38, 594);
                    draw.Stroke();
                    draw.MoveTo(61, 616);
                    draw.LineTo(61, 594);
                    draw.Stroke();
                    draw.MoveTo(95, 616);
                    draw.LineTo(95, 594);
                    draw.Stroke();
                    draw.MoveTo(118, 616);
                    draw.LineTo(118, 594);
                    draw.Stroke();

                    // title in the first box
                    text = new Phrase("D         M             I          SKU", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 23f, 599f, 173f, 609f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // data in the first box
                    string merchantSku = value.TrxMerchantSKU[i];
                    text = new Phrase(merchantSku.Remove(2), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 23f, 587f, 38f, 597f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("", new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 46f, 587f, 61f, 597f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    int index = 2;
                    if (merchantSku[2] == ' ')
                        index = 3;
                    text = new Phrase(merchantSku.Substring(index, 5), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 65f, 587f, 118f, 597f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    index = index + 5;
                    if (merchantSku.Length <= index)
                        merchantSku = "";
                    else if (merchantSku[index] == ' ')
                    {
                        index++;
                        merchantSku = merchantSku.Substring(index);
                    }
                    else
                        merchantSku = merchantSku.Substring(index);
                    text = new Phrase(merchantSku, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 99f, 587f, 118f, 597f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Second Box
                    // upper and lower line for second box
                    draw.MoveTo(15, 589);
                    draw.LineTo(130, 589);
                    draw.Stroke();
                    draw.MoveTo(15, 558);
                    draw.LineTo(130, 558);
                    draw.Stroke();

                    // straight lines for sku box
                    draw.MoveTo(15, 589);
                    draw.LineTo(15, 558);
                    draw.Stroke();
                    draw.MoveTo(55, 589);
                    draw.LineTo(55, 558);
                    draw.Stroke();
                    draw.MoveTo(90, 589);
                    draw.LineTo(90, 558);
                    draw.Stroke();
                    draw.MoveTo(130, 589);
                    draw.LineTo(130, 558);
                    draw.Stroke();

                    // title in the second box
                    text = new Phrase("  D             REASON       DATE\nM.U.C        RAISON", new iTextSharp.text.Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 24f, 553.5f, 174f, 603.5f, 21.5f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion
                    #endregion

                    #region Price
                    // price titles
                    text = new Phrase("MERCH. PRICE\nPRIX DE MARCH", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 622f, 247f, 637f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("DISCOUNT\nESCOMPTE", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 610f, 247f, 625f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("G.S.T./H.S.T.\nT.P.S./T.V.H.", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 598f, 247f, 613f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("P.S.T.\nT.V.P.", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 586f, 247f, 601f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("Total", new Font(boldFont, 7));
                    ct.SetSimpleColumn(text, 197f, 567f, 247f, 582f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("PRICE\nPRIX", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 546f, 247f, 561f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("DISCOUNT\nESCOMPTE", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 534f, 247f, 549f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("HANDLING\nMANUTENTION", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 522f, 247f, 537f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("ELEVY\nTAXE ENVIRO", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 510f, 247f, 525f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("DELIVERY\nLIVRAISON", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 498f, 247f, 513f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("G.S.T./H.S.T.\nT.P.S./T.V.H.", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 486f, 247f, 501f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("P.S.T.\nT.V.P.", new Font(baseFont, 5));
                    ct.SetSimpleColumn(text, 197f, 474f, 247f, 489f, 5f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("Total", new Font(boldFont, 7));
                    ct.SetSimpleColumn(text, 197f, 455f, 247f, 470f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // price number
                    char temp;
                    if (value.GST_HST_Extended[i] > 0 && value.PST_Extended[i] > 0)
                        temp = 'B';
                    else if (value.GST_HST_Extended[i] > 0)
                        temp = 'T';
                    else if (value.PST_Extended[i] > 0)
                        temp = 'P';
                    else
                        temp = ' ';

                    text = new Phrase(value.UnitPrice[i] + " " + temp, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 613f, 292f, 628f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    string[] tempTax = new string[2];
                    switch (temp)
                    {
                        case 'B':
                            tempTax[0] = value.GST_HST_Extended[i].ToString();
                            tempTax[1] = value.PST_Extended[i].ToString();
                            break;
                        case 'T':
                            tempTax[0] = value.GST_HST_Extended[i].ToString();
                            tempTax[1] = "";
                            break;
                        case 'P':
                            tempTax[0] = "";
                            tempTax[1] = value.PST_Extended[i].ToString();
                            break;
                        default:
                            tempTax[0] = "";
                            tempTax[1] = "";
                            break;
                    }
                    text = new Phrase(tempTax[0], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 589.5f, 292f, 604.5f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(tempTax[1], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 579f, 292f, 594f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase((value.UnitPrice[i] + value.GST_HST_Extended[i] + value.PST_Extended[i]).ToString(), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 562f, 292f, 582f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    if (value.GST_HST_Total[i] > 0 && value.PST_Total[i] > 0)
                        temp = 'B';
                    else if (value.GST_HST_Total[i] > 0)
                        temp = 'T';
                    else if (value.PST_Total[i] > 0)
                        temp = 'P';
                    else
                        temp = ' ';

                    switch (temp)
                    {
                        case 'B':
                            tempTax[0] = value.GST_HST_Total[i].ToString();
                            tempTax[1] = value.PST_Total[i].ToString();
                            break;
                        case 'T':
                            tempTax[0] = value.GST_HST_Total[i].ToString();
                            tempTax[1] = "";
                            break;
                        case 'P':
                            tempTax[0] = "";
                            tempTax[1] = value.PST_Total[i].ToString();
                            break;
                        default:
                            tempTax[0] = "";
                            tempTax[1] = "";
                            break;
                    }

                    text = new Phrase(value.UnitPrice[i] + " " + temp, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 533f, 293f, 553f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.LineHandling[i].ToString(), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 513f, 293f, 528f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(tempTax[0], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 478f, 293f, 493f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(tempTax[1].ToString(), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 466f, 293f, 481f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.LineBalanceDue[i].ToString(), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 252f, 455f, 292f, 470f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Left Unknown Region
                    // Other things that I don't know what is this for :(
                    text = new Phrase("SEARS CANADA / VI", new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 470f, 150f, 485f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("ORDER NO. DE COMMANDE", new Font(boldFont, 7));
                    ct.SetSimpleColumn(text, 10f, 457f, 150f, 472f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.CustOrderNumber, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 446f, 150f, 462f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("VENDOR SKU\n" + value.TrxVendorSKU[i], new Font(boldFont, 13));
                    ct.SetSimpleColumn(text, 10f, 400f, 150f, 450f, 11f, Element.ALIGN_LEFT);
                    ct.Go();

                    // sku title
                    text = new Phrase("D           M              I                    Q                  C              SKU             G", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 10f, 413.5f, 250f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();

                    // sku number
                    merchantSku = value.TrxMerchantSKU[i];
                    text = new Phrase(merchantSku.Remove(2), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 24f, 413.5f, 39f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("", new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 57f, 413.5f, 72f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    index = 2;
                    if (merchantSku[2] == ' ')
                        index = 3;
                    text = new Phrase(merchantSku.Substring(index, 5), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 83f, 413.5f, 123f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.TrxQty[i].ToString(), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 143f, 413.5f, 158f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    index = index + 5;
                    if (merchantSku.Length <= index)
                        merchantSku = "";
                    else if (merchantSku[index] == ' ')
                    {
                        index++;
                        merchantSku = merchantSku.Substring(index);
                    }
                    else
                        merchantSku = merchantSku.Substring(index);
                    text = new Phrase(merchantSku, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 166f, 413.5f, 186f, 428.5f, 10f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Left Bottom Additional Info
                    // some number
                    text = new Phrase(value.PoNumber, new iTextSharp.text.Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 357f, 150f, 367f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // sears infomation
                    text = new Phrase("CUSTOMER INQUIRY/DE CLIENT", new iTextSharp.text.Font(boldFont, 7));
                    ct.SetSimpleColumn(text, 10f, 338f, 150f, 348f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("WWW.SEARS.CA", new iTextSharp.text.Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 10f, 328f, 150f, 338f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("1-800-267-3277", new iTextSharp.text.Font(baseFont, 10));
                    ct.SetSimpleColumn(text, 10f, 318f, 150f, 328f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // note
                    text = new Phrase("THIS BILL OF SALE IS REQUIRED FOR RETURN OR ADJUSTMENT\nOF PURCHASE.\nCETTE FACTURE EST REQUISE POUR TOUT RETROU DE\nMARCHANDIS OU RECLAMATION.", new iTextSharp.text.Font(boldFont, 9));
                    ct.SetSimpleColumn(text, 10f, 277f, 320f, 317f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Button Return Region
                    // draw line first
                    draw.MoveTo(10, 255);
                    draw.LineTo(doc.PageSize.Width / 2, 255);
                    draw.Stroke();

                    #region Left Box
                    // horizontal lines 
                    draw.MoveTo(14, 245);
                    draw.LineTo(129, 245);
                    draw.Stroke();
                    draw.MoveTo(14, 229.5);
                    draw.LineTo(129, 229.5);
                    draw.Stroke();
                    draw.MoveTo(14, 214);
                    draw.LineTo(129, 214);
                    draw.Stroke();

                    // vertical lines
                    draw.MoveTo(14, 245);
                    draw.LineTo(14, 214);
                    draw.Stroke();
                    draw.MoveTo(129, 245);
                    draw.LineTo(129, 214);
                    draw.Stroke();

                    // text
                    text = new Phrase("REASON\n RAISON", new iTextSharp.text.Font(baseFont, 11));
                    ct.SetSimpleColumn(text, 47f, 208f, 97f, 250f, 16f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Right Box
                    // horizontal lines
                    draw.MoveTo(139, 245);
                    draw.LineTo(254, 245);
                    draw.Stroke();
                    draw.MoveTo(139, 214);
                    draw.LineTo(254, 214);
                    draw.Stroke();

                    // vertical lines
                    draw.MoveTo(139, 245);
                    draw.LineTo(139, 214);
                    draw.Stroke();
                    draw.MoveTo(179, 245);
                    draw.LineTo(179, 214);
                    draw.Stroke();
                    draw.MoveTo(214, 245);
                    draw.LineTo(214, 214);
                    draw.Stroke();
                    draw.MoveTo(254, 245);
                    draw.LineTo(254, 214);
                    draw.Stroke();

                    // text
                    text = new Phrase("  D             REASON      DATE\n                  RAISON", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 148f, 214f, 254f, 259f, 21f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion
                    #endregion

                    #region Barcode Left Bottom
                    #region Barcode One
                    // barcode number
                    text = new Phrase(value.EncodedPrice[i], new iTextSharp.text.Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 16f, 177f, 176f, 192f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // add barcode
                    Barcode128 barcode128 = new Barcode128();
                    barcode128.Code = value.EncodedPrice[i];
                    barcode128.StartStopText = false;
                    barcode128.Font = null;
                    barcode128.Extended = true;

                    Image image = barcode128.CreateImageWithBarcode(contentByte, BaseColor.BLACK, BaseColor.BLACK);
                    image.ScaleAbsoluteHeight(43f);
                    image.SetAbsolutePosition(24f, 145f);
                    contentByte.AddImage(image);

                    // bar code text under
                    text = new Phrase(value.CustOrderDate.ToString("MM/dd/yy") + "             " + value.LineBalanceDue[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 16f, 123f, 176f, 133f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Barcode Two
                    string code = value.TrxMerchantSKU[i].Replace(" ", string.Empty);
                    if (code.Length % 2 != 0)
                        code += "0";

                    BarcodeInter25 barcode25 = new BarcodeInter25();
                    barcode25.Code = code;
                    barcode25.StartStopText = false;
                    barcode25.Font = null;
                    barcode25.Extended = true;

                    image = barcode25.CreateImageWithBarcode(contentByte, BaseColor.BLACK, BaseColor.BLACK);
                    image.ScaleAbsoluteHeight(43f);
                    image.SetAbsolutePosition(24f, 70f);
                    contentByte.AddImage(image);

                    // barcode text
                    text = new Phrase(value.TrxMerchantSKU[i] + "\n" + value.Description[i], new iTextSharp.text.Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 16f, 48f, 156f, 68f, 9f, Element.ALIGN_LEFT);
                    ct.Go();

                    // section description
                    text = new Phrase("THIS BILL OF SALE IS\nREQUIRED FOR RETURN", new iTextSharp.text.Font(boldFont, 9));
                    ct.SetSimpleColumn(text, 181f, 66f, 291f, 116f, 9f, Element.ALIGN_LEFT);
                    ct.Go();

                    // useless stuff
                    text = new Phrase("66", new iTextSharp.text.Font(boldFont, 11));
                    ct.SetSimpleColumn(text, 200f, 61f, 220f, 71f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("V2C", new iTextSharp.text.Font(boldFont, 13));
                    ct.SetSimpleColumn(text, 258f, 3f, 300f, 23f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion
                    #endregion

                    #region Right Ship To Address
                    // title
                    text = new Phrase("RECIPIENT/DESTINATAIRA:", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 318f, 753f, 418f, 773f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // ship to recipient and address
                    text = new Phrase(value.Recipient.Name + "\n" + value.Recipient.Address1 + "\n" + value.Recipient.Address2 + "\n" + value.Recipient.City, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 726f, 458f, 771f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.Recipient.State + "\n" + value.Recipient.PostalCode + "\n" + value.ExpectedShipDate[i].ToString("MM/dd/yy"), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 428f, 704f, 488f, 744f, 9f, Element.ALIGN_LEFT);
                    ct.Go();

                    // ship to phone and total price
                    text = new Phrase(value.Recipient.DayPhone + "\n" + value.LineBalanceDue[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 515f, 729f, 605f, 779f, 17f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Barcode Right Top
                    // barcode number
                    text = new Phrase(value.EncodedPrice[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 683f, 468f, 693f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // add barcode
                    barcode128 = new Barcode128();
                    barcode128.Code = value.EncodedPrice[i];
                    barcode128.StartStopText = false;
                    barcode128.Font = null;
                    barcode128.Extended = true;

                    image = barcode128.CreateImageWithBarcode(contentByte, BaseColor.BLACK, BaseColor.BLACK);
                    image.ScaleAbsoluteHeight(43f);
                    image.SetAbsolutePosition(326f, 646f);
                    contentByte.AddImage(image);

                    // bar code text under
                    text = new Phrase(value.CustOrderDate.ToString("MM/dd/yy") + "             " + value.LineBalanceDue[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 625f, 488f, 635f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Top Right Box
                    // upper and lower line for sku box
                    draw.MoveTo(318, 611);
                    draw.LineTo(421, 611);
                    draw.Stroke();
                    draw.MoveTo(318, 589);
                    draw.LineTo(421, 589);
                    draw.Stroke();

                    // straight lines for sku box
                    draw.MoveTo(318, 611);
                    draw.LineTo(318, 589);
                    draw.Stroke();
                    draw.MoveTo(341, 611);
                    draw.LineTo(341, 589);
                    draw.Stroke();
                    draw.MoveTo(364, 611);
                    draw.LineTo(364, 589);
                    draw.Stroke();
                    draw.MoveTo(398, 611);
                    draw.LineTo(398, 589);
                    draw.Stroke();
                    draw.MoveTo(421, 611);
                    draw.LineTo(421, 589);
                    draw.Stroke();

                    // title in the first box
                    text = new Phrase(" D        M             I          SKU", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 326f, 594f, 476f, 604f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // data in the first box
                    merchantSku = value.TrxMerchantSKU[i];
                    text = new Phrase(merchantSku.Remove(2), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 326f, 582f, 349f, 592f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("", new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 350f, 582f, 365f, 592f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    index = 2;
                    if (merchantSku[2] == ' ')
                        index = 3;
                    text = new Phrase(merchantSku.Substring(index, 5), new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 368f, 582f, 400f, 592f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    index = index + 5;
                    if (merchantSku.Length <= index)
                        merchantSku = "";
                    else if (merchantSku[index] == ' ')
                    {
                        index++;
                        merchantSku = merchantSku.Substring(index);
                    }
                    else
                        merchantSku = merchantSku.Substring(index);
                    text = new Phrase(merchantSku, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 402f, 582f, 422f, 592f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    // sku description
                    text = new Phrase(value.Description[i] + "\n" + value.Description2[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 560f, 458f, 585f, 10f, Element.ALIGN_LEFT);
                    ct.Go();

                    #region Right Unknown Region
                    // determine if the order is direct shipment
                    string take;
                    if (value.PartnerPersonPlaceId == "")
                        take = "DIRECT";
                    else
                        take = value.PartnerPersonPlaceId;

                    text = new Phrase(take, new Font(boldFont, 16));
                    ct.SetSimpleColumn(text, 318f, 519f, 418f, 539f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase("CSU:", new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 318f, 518f, 418f, 528f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    if (take != "DIRECT")
                    {
                        // strange address
                        text = new Phrase(value.ShipTo.Name + "\n" + value.ShipTo.Address1 + "\n" + value.ShipTo.Address2 + "\n" + value.ShipTo.City, new Font(baseFont, 9));
                        ct.SetSimpleColumn(text, 318f, 481f, 468f, 526f, 9f, Element.ALIGN_LEFT);
                        ct.Go();
                        text = new Phrase(value.ShipTo.State + "\n" + value.ShipTo.PostalCode, new Font(baseFont, 9));
                        ct.SetSimpleColumn(text, 428f, 459f, 488f, 499f, 9f, Element.ALIGN_LEFT);
                        ct.Go();

                        // strange message
                        text = new Phrase(value.PaymentMethod + "                                                " + value.LineBalanceDue[i], new Font(baseFont, 13));
                        ct.SetSimpleColumn(text, 318f, 452f, 618f, 472f, 9f, Element.ALIGN_LEFT);
                        ct.Go();
                    }

                    // strange message continue
                    text = new Phrase(value.PackSlipMessage, new Font(baseFont, 7));
                    ct.SetSimpleColumn(text, 318f, 441f, 418f, 451f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Buttom Right Ship To Address
                    //title
                    text = new Phrase("SHIP TO/EXPEDIER A:", new Font(boldFont, 10));
                    ct.SetSimpleColumn(text, 318f, 408f, 458f, 423f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // address
                    text = new Phrase(value.Recipient.Name + "\n" + value.ShipTo.Address1 + "\n" + value.ShipTo.Address2 + "\n" + value.ShipTo.City, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 375f, 468f, 420f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.ShipTo.State + "\n" + value.ShipTo.PostalCode, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 428f, 362f, 488f, 392f, 9f, Element.ALIGN_LEFT);
                    ct.Go();

                    // freight info
                    text = new Phrase("FREIGHE LANE/LIGNE DE FRET:                   SPUR/CONVOYEUR:", new Font(boldFont, 7));
                    ct.SetSimpleColumn(text, 318f, 348f, 568f, 358f, 0f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.FreightLane, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 348f, 358f, 358f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    text = new Phrase(value.Spur, new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 465f, 348f, 495f, 358f, 9f, Element.ALIGN_LEFT);
                    ct.Go();
                    #endregion

                    #region Right Buttom Barcode
                    // barcode number
                    text = new Phrase(value.ReceivingInstructions[i], new Font(baseFont, 9));
                    ct.SetSimpleColumn(text, 318f, 306f, 468f, 316f, 0f, Element.ALIGN_LEFT);
                    ct.Go();

                    // add barcode
                    code = value.ReceivingInstructions[i].Replace(" ", string.Empty);
                    if (code.Length % 2 != 0)
                        code += "0";

                    barcode25 = new BarcodeInter25();
                    barcode25.Code = code;
                    barcode25.StartStopText = false;
                    barcode25.Font = null;
                    barcode25.Extended = true;

                    image = barcode25.CreateImageWithBarcode(contentByte, BaseColor.BLACK, BaseColor.BLACK);
                    image.ScaleAbsoluteHeight(43f);
                    image.SetAbsolutePosition(326f, 269f);
                    contentByte.AddImage(image);

                    // a minor stuff
                    text = new Phrase("V2C", new Font(boldFont, 30));
                    ct.SetSimpleColumn(text, 480f, 85f, 550f, 135f, 0f, Element.ALIGN_LEFT);
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
    }
}
