using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZwillingCevaWebService.SAPLayer
{
    public class SAPConnection
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DBName { get; set; }
        public string Server { get; set; }
        public string LicenseServer { get; set; }
    }
    public class SAPResponse
    {
        public string errorMsg { get; set; }
        public string errorCode { get; set; }
        public string email { get; set; }
        public string ObjectKey { get; set; }
        public string Identity { get; set; }
    }
    public class errorClass
    {
        public string errorMsg { get; set; }
        public string errorCode { get; set; }
        public string reservationCode { get; set; }
    }
    public class ConnectionList
    {
        public SAPbobsCOM.Company oCompany { get; set; }
        public string isAvailable { get; set; }
        public string error { get; set; }
        public int number { get; set; }
        public bool isConnected { get; set; }
    }
}