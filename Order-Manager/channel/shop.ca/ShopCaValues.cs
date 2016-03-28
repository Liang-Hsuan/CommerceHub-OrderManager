using Order_Manager.supportingClasses.Address;
using Order_Manager.supportingClasses.Shipment;
using System;
using System.Collections.Generic;

namespace Order_Manager.channel.shop.ca
{
    /*
     * A class for storing shop.ca order values
     */
    public class ShopCaValues
    {
        // fields for order
        public string OrderId { get; set; }
        public string SupplierId { get; set; }
        public string StoreName { get; set; }
        public DateTime OrderCreateDate { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalShippingCost { get; set; }
        public decimal TotalDiscount { get; set; }
        public Address BillTo { get; set; }
        public Address ShipTo { get; set; }
        public bool Option { get; set; }
        public string ShippingMethod { get; set; }
        public Package Package { get; set; }

        // fields for items
        public List<string> OrderItemId { get; set; }
        public List<string> Sku { get; set; }
        public List<string> Title { get; set; }
        public List<decimal> Ssrp { get; set; }
        public List<decimal> SsrpTax { get; set; }
        public List<int> Quantity { get; set; }
        public List<decimal> ItemPrice { get; set; }
        public List<decimal> ExtendedItemPrice { get; set; }
        public List<decimal> ItemTax { get; set; }
        public List<decimal> ItemShippingCost { get; set; }
        public List<decimal> ItemDiscount { get; set; }

        /* first constructor that take no argument */
        public ShopCaValues()
        {
            OrderId = "";
            SupplierId = "";
            StoreName = "";
            OrderCreateDate = DateTime.Today;
            GrandTotal = 0;
            TotalPrice = 0;
            TotalTax = 0;
            TotalShippingCost = 0;
            TotalDiscount = 0;
            BillTo = new Address();
            ShipTo = new Address();
            Option = false;
            ShippingMethod = "";
            Package = new Package();

            OrderItemId = new List<string>();
            Sku = new List<string>();
            Title = new List<string>();
            Ssrp = new List<decimal>();
            SsrpTax = new List<decimal>();
            Quantity = new List<int>();
            ItemPrice = new List<decimal>();
            ExtendedItemPrice = new List<decimal>();
            ItemTax = new List<decimal>();
            ItemShippingCost = new List<decimal>();
            ItemDiscount = new List<decimal>();
        }

        /* second constructor that take all the parametoers */
        public ShopCaValues(string orderId, string supplierId, string storeName, DateTime orderCreateDate, decimal grandTotal, decimal totalPrice, decimal totalTax, decimal totalShippingCost, decimal totalDiscount,
                            Address billTo, Address shipTo, bool option, string shippingMethod, Package package, List<string> orderItemId, List<string> sku, List<string> title, List<decimal> ssrp, List<decimal> ssrpTax,
                            List<int> quantity, List<decimal> itemPrice, List<decimal> extendedItemPrice, List<decimal> itemTax, List<decimal> itemShippingCost, List<decimal> itemDiscount)
        {
            OrderId = orderId;
            SupplierId = supplierId;
            StoreName = storeName;
            OrderCreateDate = orderCreateDate;
            GrandTotal = grandTotal;
            TotalPrice = totalPrice;
            TotalTax = totalTax;
            TotalShippingCost = totalShippingCost;
            TotalDiscount = totalDiscount;
            BillTo = billTo;
            ShipTo = shipTo;
            Option = option;
            ShippingMethod = shippingMethod;
            Package = package;

            OrderItemId = orderItemId;
            Sku = sku;
            Title = title;
            Ssrp = ssrp;
            SsrpTax = ssrpTax;
            Quantity = quantity;
            ItemPrice = itemPrice;
            ExtendedItemPrice = extendedItemPrice;
            ItemTax = itemTax;
            ItemShippingCost = itemShippingCost;
            ItemDiscount = itemDiscount;
        }
    }
}
