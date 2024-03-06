using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.SAPLayer;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.CevaLayer
{
    public class SendBusinessPartner
    {
        public List<BusinessPartnerResponse> SendBusinessPartnerByCode(string cardCode)
        {
            List<BusinessPartnerResponse> results = new List<BusinessPartnerResponse>();
            int connectionNumber = 0;
            try
            {
                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralService oGeneralService;
                SAPbobsCOM.CompanyService oCompService = null;

                ConnectionList connection = new ConnectionList();

                ZwillingCevaWebService.SAPLayer.LoginCompany log = new SAPLayer.LoginCompany();

                connection = log.setLogin();

                SAPbobsCOM.Company oCompany = connection.oCompany;
                connectionNumber = connection.number;

                oCompService = oCompany.GetCompanyService();

                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"CardCode\" from \"OCRD\" where ISNULL(\"U_E_STATUS\",'') <> 'S' and \"CardCode\" = '" + cardCode + "'");

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Dealer dealerRequest = new CevaWMSQa.Dealer();


                CevaWMSQa.Login login = new CevaWMSQa.Login();


                dealerRequest = new CevaWMSQa.Dealer();
                login = new CevaWMSQa.Login();
                dealerRequest = new CevaWMSQa.Dealer();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";


                if (oRS.RecordCount > 0)
                {
                    oBP.GetByKey(oRS.Fields.Item(0).Value.ToString());

                    dealerRequest.CompanyCode = "47";

                    dealerRequest.DealerCode = oBP.CardCode;

                    if (oBP.CardType == SAPbobsCOM.BoCardTypes.cCustomer)
                        dealerRequest.DealerType = CevaWMSQa.DealerType.Customer;
                    else
                        dealerRequest.DealerType = CevaWMSQa.DealerType.Supplier;


                    dealerRequest.DealerName = oBP.CardName;

                    //dealerRequest.Body.dealer.Address = oBP.Address == "" ? oBP.Address : oBP.Address;

                    dealerRequest.Address = oBP.Address;

                    //RegionCode ?? Bölge Kodu

                    dealerRequest.PostalCode = oBP.Addresses.ZipCode;

                    dealerRequest.CountryCode = oBP.Addresses.Country;

                    dealerRequest.CityCode = oBP.Addresses.State;

                    //SAP'de district code yani ilçe metin giriliyor. Kod seçilmiyor CEVA kod istiyor.
                    //dealerRequest.Body.dealer.DistrictCode = oBP.Addresses.;

                    //TownCode ??  Semt Kodu 

                    dealerRequest.DealerGroupCode = oBP.GroupCode.ToString();

                    dealerRequest.Phone1 = oBP.Phone1;

                    dealerRequest.Phone2 = oBP.Phone2;

                    dealerRequest.Fax = oBP.Fax;

                    dealerRequest.Email = oBP.EmailAddress;

                    dealerRequest.AuthorizedPeople1 = oBP.ContactPerson;

                    dealerRequest.TaxOffice = oBP.AdditionalID;

                    dealerRequest.TaxNumber = oBP.UnifiedFederalTaxID;


                    //Requestin xml halini tabloya kaydet.

                    var dealerRequestXML = XmlUtils.SerializeToXml(dealerRequest);


                    var response = service.CreateDealer(login, dealerRequest);

                    if (response.Successful)
                    {
                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "1");
                        oGeneralData.SetProperty("U_Type", "BusinessPartner");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", dealerRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");


                        oGeneralService.Add(oGeneralData);

                        oBP.UserFields.Fields.Item("U_E_STATUS").Value = "S";

                        results.Add(new BusinessPartnerResponse { dealerCode = oCompany.GetNewObjectKey(), response = response.ResultDescription });
                    }
                    else
                    {

                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "1");
                        oGeneralData.SetProperty("U_Type", "BusinessPartner");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", dealerRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");


                        oGeneralService.Add(oGeneralData);


                        oBP.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                        results.Add(new BusinessPartnerResponse { dealerCode = "", response = response.ResultDescription });

                    }

                    oBP.Update();

                }

                LoginCompany.ReleaseConnection(connection.number);
            }
            catch (Exception)
            {
            } 
            finally
            {
                LoginCompany.ReleaseConnection(connectionNumber); 
            }

            return results;
        }

        public List<BusinessPartnerResponse> SendAllBussinessPartner()
        {
            List<BusinessPartnerResponse> results = new List<BusinessPartnerResponse>();
            int connectionNumber = 0;
            try
            {
                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralService oGeneralService;
                SAPbobsCOM.CompanyService oCompService = null;

                ConnectionList connection = new ConnectionList();

                ZwillingCevaWebService.SAPLayer.LoginCompany log = new SAPLayer.LoginCompany();

                connection = log.setLogin();

                SAPbobsCOM.Company oCompany = connection.oCompany;
                connectionNumber = connection.number;

                oCompService = oCompany.GetCompanyService();

                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"CardCode\" from \"OCRD\" where ISNULL(\"U_E_STATUS\",'') <> 'S'");

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Dealer dealerRequest = new CevaWMSQa.Dealer();

                CevaWMSQa.Login login = new CevaWMSQa.Login();


                dealerRequest = new CevaWMSQa.Dealer();
                login = new CevaWMSQa.Login(); 
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";

                while (!oRS.EoF)
                {
                    oBP.GetByKey(oRS.Fields.Item(0).Value.ToString());

                    dealerRequest.CompanyCode = "47";

                    dealerRequest.DealerCode = oBP.CardCode;

                    if (oBP.CardType == SAPbobsCOM.BoCardTypes.cCustomer)
                        dealerRequest.DealerType = CevaWMSQa.DealerType.Customer;
                    else
                        dealerRequest.DealerType = CevaWMSQa.DealerType.Supplier;


                    dealerRequest.DealerName = oBP.CardName;

                    dealerRequest.Address = oBP.Address;

                    //RegionCode ?? Bölge Kodu  

                    dealerRequest.PostalCode = oBP.Addresses.ZipCode;

                    dealerRequest.CountryCode = oBP.Addresses.Country;

                    dealerRequest.CityCode = oBP.Addresses.State;

                    //SAP'de district code yani ilçe metin giriliyor. Kod seçilmiyor CEVA kod istiyor.
                    //dealerRequest.DistrictCode = oBP.Addresses.;

                    //TownCode ??  Semt Kodu 


                    dealerRequest.DealerGroupCode = oBP.GroupCode.ToString();

                    dealerRequest.Phone1 = oBP.Phone1;

                    dealerRequest.Phone2 = oBP.Phone2;

                    dealerRequest.Fax = oBP.Fax;

                    dealerRequest.Email = oBP.EmailAddress;

                    dealerRequest.AuthorizedPeople1 = oBP.ContactPerson;

                    dealerRequest.TaxOffice = oBP.AdditionalID;

                    dealerRequest.TaxNumber = oBP.UnifiedFederalTaxID;

                    var response = service.CreateDealer(login, dealerRequest);

                    var dealerRequestXML = XmlUtils.SerializeToXml(dealerRequest);

                    if (response.Successful)
                    {
                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "1");
                        oGeneralData.SetProperty("U_Type", "BusinessPartner");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", dealerRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");


                        oGeneralService.Add(oGeneralData);

                        oBP.UserFields.Fields.Item("U_E_STATUS").Value = "S";
                    }
                    else
                    {

                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "1");
                        oGeneralData.SetProperty("U_Type", "BusinessPartner");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", dealerRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");


                        oGeneralService.Add(oGeneralData);


                        oBP.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                    }

                    var aa = oBP.Update();
                    oRS.MoveNext();

                    results.Add(new BusinessPartnerResponse { dealerCode = oCompany.GetNewObjectKey(), response = response.ResultDescription });


                }

                LoginCompany.ReleaseConnection(connection.number);
            }
            catch (Exception)
            { 
            }
            finally
            { 
                LoginCompany.ReleaseConnection(connectionNumber);
            }

            return results;
        }
    }
}