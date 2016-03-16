using System;
using System.Collections.Generic;
using CommerceHub_OrderManager.supportingClasses;

namespace CommerceHub_OrderManager.channel.sears
{
    /*
     * A class that store all the necessary fields for Sears order (packing slip and xml)
     */
    public class SearsValues
    {
        // fields for xml feed data
        public string PartnerTrxID { get; }
        public string TransactionID { get; set; }
        public int LineCount { get; set; }
        public string PoNumber { get; set; }
        public double TrxBalanceDue { get; set; }
        public List<double> LineBalanceDue { get; set; }  // for cancel supporting purpose
        public string VendorInvoiceNumber { get; set; }
        public List<int> MerchantLineNumber { get; set; }
        public List<string> TrxVendorSKU { get; set; }
        public List<string> TrxMerchantSKU { get; set; }
        public List<string> UPC { get; set; }
        public List<int> TrxQty { get; set; }
        public List<double> TrxUnitCost { get; set; }
        public string PackageDetailID { get; set; }
        public string ServiceLevel { get; set; }
        public Package Package { get; set; }

        // other fields for packing slip
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string CustOrderNumber { get; set; }
        public DateTime CustOrderDate { get; set; }
        public string PackSlipMessage { get; set; }
        public List<string> Description { get; set; }
        public List<string> Description2 { get; set; }
        public List<double> UnitPrice { get; set; }
        public List<double> LineHandling { get; set; }
        public List<DateTime> ExpectedShipDate { get; set; }
        public List<double> GST_HST_Extended { get; set; }
        public List<double> PST_Extended { get; set; }
        public List<double> GST_HST_Total { get; set; }
        public List<double> PST_Total { get; set; }
        public List<string> EncodedPrice { get; set; }
        public List<string> ReceivingInstructions { get; set; }
        public Address BillTo { get; set; }
        public Address Recipient { get; set; }
        public Address ShipTo { get; set; }
        public string PartnerPersonPlaceId { get; set; }
        public string FreightLane { get; set; }
        public string Spur { get; set; }

        /* first constructor that take no argument */
        public SearsValues()
        {
            PartnerTrxID = "ashlinbpg";

            TransactionID = "";
            LineCount = 0;
            PoNumber = "";
            TrxBalanceDue = 0;
            LineBalanceDue = new List<double>();
            VendorInvoiceNumber = "";
            MerchantLineNumber = new List<int>();
            TrxVendorSKU = new List<string>();
            TrxMerchantSKU = new List<string>();
            UPC = new List<string>();
            TrxQty = new List<int>();
            TrxUnitCost = new List<double>();
            PackageDetailID = "";
            ServiceLevel = "";
            Package = new Package();

            OrderDate = DateTime.Now;
            PaymentMethod = "";
            CustOrderNumber = "";
            CustOrderDate = DateTime.Now;
            PackSlipMessage = "";
            Description = new List<string>();
            Description2 = new List<string>();
            UnitPrice = new List<double>();
            LineHandling = new List<double>();
            ExpectedShipDate = new List<DateTime>();
            GST_HST_Extended = new List<double>();
            PST_Extended = new List<double>();
            GST_HST_Total = new List<double>();
            PST_Total = new List<double>();
            EncodedPrice = new List<string>();
            ReceivingInstructions = new List<string>();
            BillTo = new Address();
            Recipient = new Address();
            ShipTo = new Address();
            PartnerPersonPlaceId = "";
            FreightLane = "";
            Spur = "";
        }

        /* second constructor that take all the parametoers */
        public SearsValues(string transactionId, int lineCount, string poNumber, double trxBalanceDue, List<double> lineBalanceDue, string vendorInvoiceNumber, List<int> merchantLineNumber, List<string> trxVendorSku,
                           List<string> trxMerchantSku, List<string> upc, List<int> trxQty, List<double> trxUnitCost, string packageDetailId, string serviceLevel1, Package package, DateTime orderDate, string paymentMethod, string custOrderNumber, DateTime custOrderDate, string packslipMessage,
                           List<string> description, List<string> description2, List<double> unitPrice, List<double> lineHandling, List<DateTime> expectedShipDate, List<double> gstHstExtended, List<double> pstExtended, List<double> gstHstTotal, List<double> pstTotal, List<string> encodedPrice,
                           List<string> receivingInstructions, Address billTo, Address recipient, Address shipTo, string partnerPersonPlaceId, string freightLane, string spur)
        {
            PartnerTrxID = "ashlinbpg";

            TransactionID = transactionId;
            LineCount = lineCount;
            PoNumber = poNumber;
            TrxBalanceDue = trxBalanceDue;
            LineBalanceDue = lineBalanceDue;
            VendorInvoiceNumber = vendorInvoiceNumber;
            MerchantLineNumber = merchantLineNumber;
            TrxVendorSKU = trxVendorSku;
            TrxMerchantSKU = trxMerchantSku;
            UPC = upc;
            TrxQty = trxQty;
            TrxUnitCost = trxUnitCost;
            PackageDetailID = packageDetailId;
            ServiceLevel = serviceLevel1;
            Package = package;

            OrderDate = orderDate;
            PaymentMethod = paymentMethod;
            CustOrderNumber = custOrderNumber;
            CustOrderDate = custOrderDate;
            PackageDetailID = packslipMessage;
            Description = description;
            Description2 = description2;
            UnitPrice = unitPrice;
            LineHandling = lineHandling;
            ExpectedShipDate = expectedShipDate;
            GST_HST_Extended = gstHstExtended;
            PST_Extended = pstExtended;
            GST_HST_Total = gstHstTotal;
            PST_Total = pstTotal;
            EncodedPrice = encodedPrice;
            ReceivingInstructions = receivingInstructions;
            BillTo = billTo;
            Recipient = recipient;
            ShipTo = shipTo;
            PartnerPersonPlaceId = partnerPersonPlaceId;
            FreightLane = freightLane;
            Spur = spur;
        }
    }
}
