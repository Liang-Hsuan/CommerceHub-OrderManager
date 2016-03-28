using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Manager.channel.shop.ca
{
    public class ShopCa
    {
        // fields for directory on sftp server
        private const string SHIPMENT_DIR = "toclient/order";
        private const string CONFIRM_DIR = "fromclient/order_status";
    }
}
