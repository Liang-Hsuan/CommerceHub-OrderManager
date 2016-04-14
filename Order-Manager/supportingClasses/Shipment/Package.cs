using System;
using System.Data.SqlClient;
using System.Linq;
using Order_Manager.channel.sears;
using Order_Manager.channel.shop.ca;
using Order_Manager.channel.giantTiger;

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
        public string IdentificationNumber { get; set; }    // for ups shipment void
        public string SelfLink { get; set; }                // for canada post shipment void
        public string LabelLink { get; set; }               // for canada post label recovery

        /* first constructor that take no argument */
        public Package()
        {
            Weight = 0;
            Length = 0;
            Width = 0;
            Height = 0;

            PackageType = "";
            Service = "";

            TrackingNumber = "";
            IdentificationNumber = "";
            SelfLink = "";
            LabelLink = "";
        }

        /* second constructor that take SearsValues object as parameter */
        public Package(SearsValues value)
        {
            // generate package detail -> weight and dimensions
            decimal[] skuDetail = { 0, 0, 0, 0 };

            foreach (decimal[] detailList in value.TrxVendorSku.Select(GetSkuDetail).Where(detailList => !detailList.Equals(null)))
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
            Service = "UPS Standard";
        }

        /* third constructor that take ShopCaValues object as parameter */
        public Package(ShopCaValues value)
        {
            // generate package detail -> weight and dimensions
            decimal[] skuDetail = { 0, 0, 0, 0 };

            foreach (decimal[] detailList in value.Sku.Select(GetSkuDetail).Where(detailList => !detailList.Equals(null)))
            {
                for (int i = 0; i < 4; i++)
                    skuDetail[i] += detailList[i];
            }

            // allocate data
            Weight = skuDetail[0] / 1000;
            Length = skuDetail[1];
            Width = skuDetail[2];
            Height = skuDetail[3];

            // service is set to default
            Service = "Expedited Parcel";
            SelfLink = "";
            LabelLink = "";
        }

        /* fourth constructor that take GiantTigerValues object as parameter */
        public Package(GiantTigerValues value)
        {
            // generate package detail -> weight and dimensions
            decimal[] skuDetail = { 0, 0, 0, 0 };

            foreach (decimal[] detailList in value.VendorSku.Select(GetSkuDetail).Where(detailList => !detailList.Equals(null)))
            {
                for (int i = 0; i < 4; i++)
                    skuDetail[i] += detailList[i];
            }

            // allocate data
            Weight = skuDetail[0] / 1000;
            Length = skuDetail[1];
            Width = skuDetail[2];
            Height = skuDetail[3];

            // service is set to default
            Service = "Expedited Parcel";
            SelfLink = "";
            LabelLink = "";
        }

        /* fiveth constructor that take all parameters */
        public Package(decimal weight, decimal length, decimal width, decimal height, string packageType, string service, string trackingNumber, 
                       string identificationNumber, string selfLink, string labelLink)
        {
            Weight = weight;
            Length = length;
            Width = width;
            Height = height;

            PackageType = packageType;
            Service = service;

            TrackingNumber = trackingNumber;
            IdentificationNumber = identificationNumber;
            SelfLink = selfLink;
            LabelLink = labelLink;
        }

        /* a method that get the detail of the given sku */
        public static decimal[] GetSkuDetail(string sku)
        {
            // local supporting fields
            decimal[] list = new decimal[4];

            // [0] weight, [1] length, [2] width, [3] height
            using (SqlConnection conneciton = new SqlConnection(Properties.Settings.Default.Designcs))
            {
                SqlCommand command = new SqlCommand("SELECT Weight_grams, Shippable_Height_cm, Shippable_Width_cm, Shippable_Depth_cm " +
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
