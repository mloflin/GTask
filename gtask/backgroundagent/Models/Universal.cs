using System;
using System.Globalization;
using System.Threading;

namespace BackgroundAgent
{
    class Universal
    {
        //Everything stored locally is in US format as provided from Google. Either ShortDatePattern (mm-dd-yyyy 00:00:00) or .ToString("yyyy-MM-dd'T'00:00:00.00Z")
        //ConvertToUniversalDate is only used when...
        //1) Grabbing data out of storage to show to the user
        //2) Sorts when comparing to Min/Max value
        //3) List Sorting

        public static string ConvertToUniversalDate(string oldDate)
        {
            ///SUMMARY - Converts US to the phones current culture
            string newDate = oldDate;
            string right = null;

            if (oldDate != null)
            {
                if (oldDate.Substring(oldDate.Length - 1, 1) == "Z")
                {
                    right = oldDate.Substring(oldDate.Length - 12, 12);
                    right = " " + right.Substring(0, 8);
                }
                else
                {
                    right = oldDate.Substring(oldDate.Length - 9, 9);
                }

                //Check string & Culture to see if the due date needs to be converted
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                DateTimeFormatInfo usDTF = new CultureInfo("en-US").DateTimeFormat;
                DateTimeFormatInfo newDTF = new CultureInfo(currentCulture.ToString()).DateTimeFormat;

                if (!String.IsNullOrEmpty(oldDate) && currentCulture.ToString() != "en-US")
                {
                    newDate = Convert.ToDateTime(oldDate, usDTF).ToString(newDTF.ShortDatePattern) + right;
                }
                else if (!String.IsNullOrEmpty(oldDate))
                {
                    newDate = Convert.ToDateTime(oldDate, usDTF).ToString(usDTF.ShortDatePattern) + right;
                }
            }
            return newDate;
        }

        

        public static string ConvertFromUniversalDate(string oldDate)
        {
            ///SUMMARY - Converts from current culture to US
            string newDate = oldDate;
            string right9 = null;

            if (oldDate != null)
            {
                right9 = oldDate.Substring(oldDate.Length - 9, 9);

                //Check string & Culture to see if the due date needs to be converted
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (!String.IsNullOrEmpty(oldDate) && currentCulture.ToString() != "en-US")
                {
                    DateTimeFormatInfo usDTF = new CultureInfo("en-US").DateTimeFormat;
                    DateTimeFormatInfo newDTF = new CultureInfo(currentCulture.ToString()).DateTimeFormat;
                    newDate = Convert.ToDateTime(oldDate, newDTF).ToString(usDTF.ShortDatePattern) + right9;
                }
            }
            return newDate;
        }
    }
}