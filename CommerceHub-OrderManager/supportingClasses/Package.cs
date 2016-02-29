namespace CommerceHub_OrderManager.supportingClasses
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

        /* first constructor that take no argument */
        public Package()
        {
            Weight = 0;
            Length = 0;
            Width = 0;
            Height = 0;

            PackageType = "Customer Supplied Package";
        }

        /* second constructor that take all parameters */
        public Package(decimal weight, decimal length, decimal width, decimal height, string packageType)
        {
            Weight = weight;
            Length = length;
            Width = width;
            Height = height;

            PackageType = packageType;
        }
    }
}
