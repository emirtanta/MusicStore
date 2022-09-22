using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Utility
{
    public static class ProjectConstant
    {
        public const string ResultNowFound = "Data Not Found";

        /***************************************************************/

        //kapak resimlerini getiren procedure ismi
        public const string Proc_CoverType_GetAll = "usp_GetCoverTypes";

        //tek bir kapak datasını getirir
        public const string Proc_CoverType_Get = "usp_GetCoverType";

        // kapak datasını silen procedure
        public const string Proc_CoverType_Delete = "usp_DeleteCoverType";

        // kapak datasını ekleyen procedure
        public const string Proc_CoverType_Create = "usp_CreateCoverType";

        // kapak datasını güncelleyen procedure
        public const string Proc_CoverType_Update = "usp_UpdateCoverType";
        /****************************************************************/


        /**************** Rol ile İlgili İşlemler **********************/
        public const string Role_User_Indi = "Bireysel Müşteri";
        public const string Role_User_Comp = "Şirket Müşteri";
        public const string Role_User_Admin = "Admin";
        public const string Role_User_Employee = "Employee";

        /************  Session İşlemleri için  *******************************/
        public const string shoppingCard = "ShoppingCard";

        //sepetteki verilerin tutarnı hesaplar
        public static double GetPriceBaseOnQuantity(int quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
                return price;
            else
            {
                if (quantity < 100)
                    return price50;
                else
                    return price100;
            }
        }

        //tiny ediötdeki veriyi yazar
        public static string ConvertToRawHtml(string description)
        {
            char[] array = new char[description.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < description.Length; i++)
            {
                char let = description[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }


        
        

        /******************* Sipariş Durumu ****************************/
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefund = "Refund";

        /****************** Ödeme Durumu  *************************/
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "Delayed";



        /************************   *******************************/


    }
}
