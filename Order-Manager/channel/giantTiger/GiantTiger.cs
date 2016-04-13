using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tamir.SharpSsh;

namespace Order_Manager.channel.giantTiger
{
    /*
     * A class that connect to Giant Tiger sftp server and manage all the orders for Giant Tiger
     */
    public class GiantTiger : Channel
    {
        // fields for directory on sftp server
        private const string SHIPMENT_DIR = "toclient/order";
        private const string CONFIRM_DIR = "fromclient/order_status";

        // field for directory on local
        private readonly string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\ShopCaOrders";
        private readonly string newOrderDir;
        private readonly string completeOrderDir;

        // field for sftp connection
        private readonly Sftp sftp;

        /* constructor that initialize folders for csv feed */
        public GiantTiger()
        {
            #region Folder Check
            // check and generate folders
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);

            newOrderDir = rootDir + "\\GiantTigerNewOrders";
            if (!Directory.Exists(newOrderDir))
                Directory.CreateDirectory(newOrderDir);

            completeOrderDir = rootDir + "\\GiantTigerCompleteOrders";
            if (!Directory.Exists(completeOrderDir))
                Directory.CreateDirectory(completeOrderDir);
            #endregion
        }

        #region Public Get
        /* a method that get all new order on the server and update to the database */
        public void GetOrder()
        {

        }

        /* a method that return all the new order values */
        public GiantTigerValues[] GetAllNewOrder()
        {
            return null;
        }

        /* a method that return all shipped order */
        public GiantTigerValues[] GetAllShippedOrder()
        {
            return null;
        }
        #endregion

        #region Number of Orders and Shipments
        /* methods that return the number of order and shipment from the given date */
        public int GetNumberOfOrder(DateTime time)
        {
            return 0;
        }
        public int GetNumberOfShipped(DateTime time)
        {
            return 0;
        }
        #endregion

        #region Get Order Infomation
        /* method that get the new order from sftp server */
        private void GetOrder(string filePath, IEnumerable<string> fileList)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();

            // download the files from the given list
            foreach (string file in fileList.Where(file => file != "." && file != ".."))
            {
                sftp.Get(SHIPMENT_DIR + "/" + file, filePath + "\\" + file);

                // after download the file delete it on the server (no need it anymore)
                // ServerDelete.Delete(sftp.Host, sftp.Username, sftp.Password, SHIPMENT_DIR + "/" + file);
            }

            sftp.Close();
        }

        /* method that get the shipment order file name from sftp server */
        private IEnumerable<string> GetOrderName(string serverDir)
        {
            // connection to sftp server and read all the list of file
            sftp.Connect();
            ArrayList list = sftp.GetFileList(serverDir);
            sftp.Close();

            // retrieve all the files name to the list
            return (from object item in list select item.ToString()).ToArray();
        }

        /* method that get all the order in the file with the given dictionary <filePath, text> and return dictionary <orderId, filepath> */
        private static Dictionary<string, string> GetOrderId(Dictionary<string, string> fileText)
        {
            return null;
        }
        #endregion

        /* a method that delete obsolete orders in database and clear all local files */
        public void Delete()
        {
            // Local Delete
            new DirectoryInfo(newOrderDir).Delete(true);
        }
    }
}
