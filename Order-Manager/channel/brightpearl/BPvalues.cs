using System;
using Order_Manager.supportingClasses.Address;

namespace Order_Manager.channel.brightpearl
{
    /* 
     * A class that contains all necessary fields for posting an order to brightpearl
     */
    public class BPvalues
    {
        // customer details fields
        public Address Address { get; set; }

        // order details fields
        public string Reference { get; set; }
        public DateTime PlaceOn { get; set; }
        public int ChannelId { get; set; }
        public int DeliveryId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double RowNet { get; set; }
        public double RowTax { get; set; }
        public double TotalPaid { get; set; }

        /* first constructor that take no argument */
        public BPvalues()
        {
            Address = new Address();

            Reference = "";
            PlaceOn = DateTime.Today;
            ChannelId = 0;
            DeliveryId = 0;
            SKU = "";
            ProductName = "";
            Quantity = 0;
            RowNet = 0;
            RowTax = 0;
            TotalPaid = 0;
        }

        /* second constructor that take all parameters as argument */
        public BPvalues(Address address, string reference, DateTime placeOn, int channelId, int deliveryId, string sku, string productName, int quantity,
                        double rowNet, double rowTax, double totalPaid)
        {
            Address = address;

            Reference = reference;
            PlaceOn = placeOn;
            ChannelId = channelId;
            DeliveryId = deliveryId;
            SKU = sku;
            ProductName = productName;
            Quantity = quantity;
            RowNet = rowNet;
            RowTax = rowTax;
            TotalPaid = totalPaid;
        }
    }
}
