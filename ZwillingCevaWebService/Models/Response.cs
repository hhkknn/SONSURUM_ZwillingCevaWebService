using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZwillingCevaWebService.SAPLayer
{
    public class Response
    {
        public string TransactionNumber { get; set; }

        public bool Successful { get; set; }

        public string ResultDescription { get; set; }

        public string SAPNumber { get; set; }
    }
}