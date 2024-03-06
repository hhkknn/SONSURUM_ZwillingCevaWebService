using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZwillingCevaWebService.Models
{
    [Serializable]
    public class StockTransfer
    { 
        public string TransactionType { get; set; }

        public string StockTransferDate { get; set; }

        public List<StockTransferDetail> Detail { get; set; }

        public string CevaTransactionNumber { get; set; }
    } 
    [Serializable]
    public class StockTransferDetail
    {
        private string ProductCodeVal;

        public string ProductNumber
        {
            get { return ProductCodeVal; }
            set { ProductCodeVal = value; }
        }

        public double Quantity;
    }
}