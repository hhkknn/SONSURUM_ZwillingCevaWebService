using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ZwillingCevaWebService.SAPLayer
{
    public class LoginCompany
    {
        public static SAPbobsCOM.Company oCompany0;
        public static SAPbobsCOM.Company oCompany1;

        private static List<ConnectionList> _connlist;
        public static List<ConnectionList> Connlist
        {
            get
            {
                if (_connlist == null)
                    _connlist = new List<ConnectionList>();
                return _connlist;
            }

            set
            {
                _connlist = value;
            }
        }


        public static void ReleaseConnection(int no)
        {
            foreach (var item in Connlist.Where(w => w.number == no))
            {
                item.isAvailable = "0";
            }
        }

        public ConnectionList setLogin()
        {
            int errCode;
            string errDesc = "";
            int Connect = Connlist.Count();
            if (Connect == 0)
            {

                oCompany0 = new SAPbobsCOM.Company();
                oCompany1 = new SAPbobsCOM.Company();

                oCompany0.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
                oCompany0.SLDServer = ConfigurationManager.AppSettings["LicenseServer"];
                oCompany0.Server = ConfigurationManager.AppSettings["Server"];
                oCompany0.UserName = ConfigurationManager.AppSettings["UserName"];
                oCompany0.Password = ConfigurationManager.AppSettings["Password"];
                oCompany0.CompanyDB = ConfigurationManager.AppSettings["CompanyDB"];
                oCompany0.DbServerType = (SAPbobsCOM.BoDataServerTypes)Convert.ToInt32(ConfigurationManager.AppSettings["DbServerType"]);

                oCompany1.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
                oCompany1.SLDServer = ConfigurationManager.AppSettings["LicenseServer"];
                oCompany1.Server = ConfigurationManager.AppSettings["Server"];
                oCompany1.UserName = ConfigurationManager.AppSettings["UserName"];
                oCompany1.Password = ConfigurationManager.AppSettings["Password"];
                oCompany1.CompanyDB = ConfigurationManager.AppSettings["CompanyDB"];
                oCompany1.DbServerType = (SAPbobsCOM.BoDataServerTypes)Convert.ToInt32(ConfigurationManager.AppSettings["DbServerType"]);

                oCompany0.Connect();
                oCompany1.Connect();

                for (int i = 1; i <= 2; i++)
                {
                    switch (i)
                    {
                        case 1:
                            Connlist.Add(new ConnectionList { oCompany = oCompany0, isAvailable = oCompany0.Connected ? "0" : "1", error = oCompany0.GetLastErrorDescription(), number = 1, isConnected = oCompany0.Connected });
                            break;
                        case 2:
                            Connlist.Add(new ConnectionList { oCompany = oCompany1, isAvailable = oCompany1.Connected ? "0" : "1", error = oCompany1.GetLastErrorDescription(), number = 2, isConnected = oCompany1.Connected });
                            break;

                    }
                }

            }

            errDesc = oCompany0.GetLastErrorDescription();
            var a = Connlist.Where(x => x.isAvailable == "0");
            if (a.ToList().Count > 0)
            {
                ConnectionList oCompany = a.First();
                foreach (var item in Connlist.Where(x => x.isAvailable == "0" && x.number == oCompany.number))
                {
                    item.isAvailable = "1";
                }
                return oCompany;
            }
            //else if (a.ToList().Count == 0)
            //{
            //    ReleaseConnection(1);

            //    var b = Connlist.Where(x => x.isAvailable == "0");
            //    ConnectionList oCompany = b.First();
            //    foreach (var item in Connlist.Where(x => x.isAvailable == "0" && x.number == oCompany.number))
            //    {
            //        item.isAvailable = "1";
            //    }
            //    return oCompany;
            //} 
            else return new ConnectionList { number = -1, error = errDesc };
        }
    }
}