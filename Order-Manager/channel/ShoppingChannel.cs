using System;

namespace Order_Manager.channel
{
    /*
     * An abstract class for all shopping channels
     */
    public abstract class ShoppingChannel : Channel
    {
        /* inherent methods */
        public abstract void GetOrder();
        public abstract int GetNumberOfOrder(DateTime time);
        public abstract int GetNumberOfShipped(DateTime time);

        #region Method Addition
        /* a method that substring the given string */
        protected static string substringMethod(string original, string startingString, int additionIndex)
        {
            return original.Substring(original.IndexOf(startingString) + additionIndex);
        }

        /* a method that get the next target token */
        protected static string getTarget(string text)
        {
            int i = 0;
            while (text[i] != '<' && text[i] != '>' && text[i] != '"')
                i++;

            return text.Substring(0, i);
        }
        #endregion
    }
}
