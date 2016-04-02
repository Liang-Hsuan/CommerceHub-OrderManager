using System;

namespace Order_Manager.channel
{
    /*
     * An interface that defines all essential public methods for all shopping channel classes
     */
    public interface Channel
    {
        /* method that get all new order */
        void GetOrder();

        /* method that delete old data */
        void Delete();

        /* methods for getting number of order and shipment for a particular date */
        int GetNumberOfOrder(DateTime time);
        int GetNumberOfShipped(DateTime time);
    }
}
