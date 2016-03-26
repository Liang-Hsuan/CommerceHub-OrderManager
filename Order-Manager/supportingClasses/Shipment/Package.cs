using System;
using System.Data.SqlClient;
using System.Linq;
using Order_Manager.channel.sears;

namespace Order_Manager.supportingClasses.Shipment
{
    /* 
     * A class that is able to store package detail
     */
    public class Package
    {
        // field for package dimensions
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }

        // field for package type
        public string PackageType { get; set; }
        public string Service { get; set; }

        // field for identification
        public string TrackingNumber { get; set; }
        public string IdentificationNumber { get; set; }

        /* first constructor that take no argument */
        public Package()
        {
            Weight = 0;
            Length = 0;
            Width = 0;
            Height = 0;

            PackageType = "Customer Supplied Package";
            Service = "UPS Express";

            TrackingNumber = "";
            IdentificationNumber = "";
        }

        /* second constructor that take SearsValues object as parameter */
        public Package(SearsValues value)
        {
            // generate package detail -> weight and dimensions
            decimal[] skuDetail = { 0, 0, 0, 0 };

            foreach (decimal[] detailList in value.TrxVendorSKU.Select(getSkuDetail).Where(detailList => !detailList.Equals(null)))
            {
                for (int i = 0; i < 4; i++)
                    skuDetail[i] += detailList[i];
            }

            // allocate data
            Weight = skuDetail[0] / 1000;
            Length = skuDetail[1];
            Width = skuDetail[2];
            Height = skuDetail[3];

            // those are set to default
            PackageType = "Customer Supplied Package";
            Service = "UPS Express";
        }

        /* third constructor that take all parameters */
        public Package(decimal weight, decimal length, decimal width, decimal height, string packageType, string service, string trackingNumber, string identificationNumber)
        {
            Weight = weight;
            Length = length;
            Width = width;
            Height = height;

            PackageType = packageType;
            Service = service;

            TrackingNumber = trackingNumber;
            IdentificationNumber = identificationNumber;
        }

        /* a method that get the detail of the given sku */
        private static decimal[] getSkuDetail(string sku)
        {
            // local supporting fields
            decimal[] list = new decimal[4];

            // [0] weight, [1] length, [2] width, [3] height
            using (SqlConnection conneciton = new SqlConnection(Properties.Settings.Default.Designcs))
            {
                SqlCommand command = new SqlCommand("SELECT Weight_grams, Shippable_Depth_cm, Shippable_Width_cm, Shippable_Height_cm " +
                                                    "FROM master_Design_Attributes design JOIN master_SKU_Attributes sku ON (design.Design_Service_Code = sku.Design_Service_Code) " +
                                                    "WHERE SKU_Ashlin = \'" + sku + "\';", conneciton);
                conneciton.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                // check if there is result
                if (!reader.HasRows)
                    return null;

                for (int i = 0; i < 4; i++)
                    list[i] = Convert.ToDecimal(reader.GetValue(i));
            }

            return list;
        }
    }
}
