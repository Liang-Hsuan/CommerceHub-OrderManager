using System;
using System.Collections.Generic;
using Order_Manager.supportingClasses.Address;
using Order_Manager.supportingClasses.Shipment;

namespace Order_Manager.channel.sears
{
    /*
     * A class that store all the necessary fields for Sears order (packing slip and xml)
     */
    public class SearsValues
    {
        // fields for xml feed data
        public string PartnerTrxId { get; }
        public string TransactionId { get; set; }
        public int LineCount { get; set; }
        public string PoNumber { get; set; }
        public double TrxBalanceDue { get; set; }
        public List<double> LineBalanceDue { get; set; }  // for cancel supporting purpose
        public string VendorInvoiceNumber { get; set; }
        public List<int> MerchantLineNumber { get; set; }
        public List<string> TrxVendorSku { get; set; }
        public List<string> TrxMerchantSku { get; set; }
        public List<string> Upc { get; set; }
        public List<int> TrxQty { get; set; }
        public List<double> TrxUnitCost { get; set; }
        public string PackageDetailId { get; set; }
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
        public List<double> LineShipping { get; set; }
        public List<double> LineHandling { get; set; }
        public List<DateTime> ExpectedShipDate { get; set; }
        public List<double> GstHstExtended { get; set; }
        public List<double> PstExtended { get; set; }
        public List<double> GstHstTotal { get; set; }
        public List<double> PstTotal { get; set; }
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
            PartnerTrxId = "ashlinbpg";

            TransactionId = "";
            LineCount = 0;
            PoNumber = "";
            TrxBalanceDue = 0;
            LineBalanceDue = new List<double>();
            VendorInvoiceNumber = "";
            MerchantLineNumber = new List<int>();
            TrxVendorSku = new List<string>();
            TrxMerchantSku = new List<string>();
            Upc = new List<string>();
            TrxQty = new List<int>();
            TrxUnitCost = new List<double>();
            PackageDetailId = "";
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
            LineShipping = new List<double>();
            LineHandling = new List<double>();
            ExpectedShipDate = new List<DateTime>();
            GstHstExtended = new List<double>();
            PstExtended = new List<double>();
            GstHstTotal = new List<double>();
            PstTotal = new List<double>();
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
                           List<string> description, List<string> description2, List<double> unitPrice, List<double> lineShipping, List<double> lineHandling, List<DateTime> expectedShipDate, List<double> gstHstExtended, List<double> pstExtended, List<double> gstHstTotal, List<double> pstTotal, List<string> encodedPrice,
                           List<string> receivingInstructions, Address billTo, Address recipient, Address shipTo, string partnerPersonPlaceId, string freightLane, string spur)
        {
            PartnerTrxId = "ashlinbpg";

            TransactionId = transactionId;
            LineCount = lineCount;
            PoNumber = poNumber;
            TrxBalanceDue = trxBalanceDue;
            LineBalanceDue = lineBalanceDue;
            VendorInvoiceNumber = vendorInvoiceNumber;
            MerchantLineNumber = merchantLineNumber;
            TrxVendorSku = trxVendorSku;
            TrxMerchantSku = trxMerchantSku;
            Upc = upc;
            TrxQty = trxQty;
            TrxUnitCost = trxUnitCost;
            PackageDetailId = packageDetailId;
            ServiceLevel = serviceLevel1;
            Package = package;

            OrderDate = orderDate;
            PaymentMethod = paymentMethod;
            CustOrderNumber = custOrderNumber;
            CustOrderDate = custOrderDate;
            PackageDetailId = packslipMessage;
            Description = description;
            Description2 = description2;
            UnitPrice = unitPrice;
            LineShipping = lineShipping;
            LineHandling = lineHandling;
            ExpectedShipDate = expectedShipDate;
            GstHstExtended = gstHstExtended;
            PstExtended = pstExtended;
            GstHstTotal = gstHstTotal;
            PstTotal = pstTotal;
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
