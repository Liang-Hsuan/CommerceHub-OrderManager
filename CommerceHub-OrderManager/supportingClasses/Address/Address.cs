namespace CommerceHub_OrderManager.supportingClasses.Address
{
    /* 
     * A class that store address value
     */
    public class Address
    {
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string DayPhone { get; set; }

        /* the first constructor that take no argument */
        public Address()
        {
            Name = "";
            Address1 = "";
            Address2 = "";
            City = "";
            State = "";
            PostalCode = "";
            DayPhone = "";
        }

        /* the second constructor that accepts all parameters */
        public Address(string name, string address1, string address2, string city, string state, string postalCode, string phone)
        {
            Name = name;
            Address1 = address1;
            Address2 = address2;
            City = city;
            State = state;
            PostalCode = postalCode;
            DayPhone = phone;
        }
    }
}
