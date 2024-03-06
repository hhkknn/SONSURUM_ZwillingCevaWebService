using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.SAPLayer;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.CevaLayer
{
    public class SendInventoryTransferRequest
    {

        public List<InventoryTransferRequestResponse> SendInventoryTransferRequestByCode(int InventoryTransferRequestNo)
        {
            List<InventoryTransferRequestResponse> results = new List<InventoryTransferRequestResponse>();
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

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Order InventoryTransferRequest = new CevaWMSQa.Order();
                CevaWMSQa.OrderDetail InventoryTransferRequestDetail = new CevaWMSQa.OrderDetail();
                List<CevaWMSQa.OrderDetail> InventoryTransferRequestDetailList = new List<CevaWMSQa.OrderDetail>();
                CevaWMSQa.OrderProperties orderProperties = new CevaWMSQa.OrderProperties();
                List<CevaWMSQa.OrderProperties> orderPropertiesList = new List<CevaWMSQa.OrderProperties>();
                SAPbobsCOM.StockTransfer oDocuments = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest);

                CevaWMSQa.Login login = new CevaWMSQa.Login();

                login = new CevaWMSQa.Login();
                InventoryTransferRequest = new CevaWMSQa.Order();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";


                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSUpdate = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                oRS.DoQuery("Select \"DocEntry\" from \"OWTQ\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E') and \"DocEntry\" = '" + InventoryTransferRequestNo + "'");


                if (oRS.RecordCount > 0)
                {
                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));
                    oBP.GetByKey(oDocuments.CardCode);

                    InventoryTransferRequest.CompanyCode = "47";

                    //InventoryTransferRequest.WarehouseCode = "01";

                    InventoryTransferRequest.WarehouseCode = "02"; //Online çıkışlar stok nakli ile yapılamayacağı için hep 02 verildi. 

                    InventoryTransferRequest.OrderNumber = "Y" + oDocuments.DocEntry.ToString();

                    InventoryTransferRequest.OrderReferenceNumber = oDocuments.DocEntry.ToString();

                    InventoryTransferRequest.MovementCode = "204";

                    if (string.Equals(oDocuments.ToWarehouse.ToString(), "STR_01_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_02_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_03_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_04_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_05_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_06_A"))
                    {
                        //Bu depo kodları değiştirilebileceğinedn dolayı buradan almaktansa özellik 63 alanından alınabilir. (63 mağaza olduğunu gösterir.)  
                        InventoryTransferRequest.MovementCode = "202";
                        InventoryTransferRequest.WarehouseCode = "01"; //202 hareket kodunda yalnızca 01 depo kodu gönderilebilir.
                    }

                    InventoryTransferRequest.RecordType = CevaWMSQa.RecordType.New;

                    InventoryTransferRequest.DeliveryDate = oDocuments.DocDate;

                    InventoryTransferRequest.DeliveryDate = oDocuments.DocDate;

                    InventoryTransferRequest.ToDealerCode = oDocuments.CardCode;

                    InventoryTransferRequest.FromDealerCode = oDocuments.CardCode;

                    InventoryTransferRequest.OrderType = CevaWMSQa.OrderType.Delivery; // Sabit gönderilmesi istendi.

                    InventoryTransferRequest.DeliveryAddress = oDocuments.Address;

                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        InventoryTransferRequestDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                        InventoryTransferRequestDetail.ProductNumber = oDocuments.Lines.ItemCode;

                        InventoryTransferRequestDetail.LineRecordType = CevaWMSQa.RecordType.New;

                        InventoryTransferRequestDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                        InventoryTransferRequestDetailList.Add(InventoryTransferRequestDetail);

                        InventoryTransferRequestDetail = new CevaWMSQa.OrderDetail();
                    }


                    InventoryTransferRequest.Detail = InventoryTransferRequestDetailList.ToArray();


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 1;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString()) ?
                        oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString() : oDocuments.CardName;

                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 2;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_County").Value.ToString();// ilçe
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 3;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_City").Value.ToString();// il
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 4;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_ZipCode").Value.ToString();// posta kodu
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 5;
                    orderProperties.PropertyValue1 = oBP.CardForeignName;// Firma adı
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 6;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString() : !string.IsNullOrEmpty(oBP.UnifiedFederalTaxID.ToString()) ? oBP.UnifiedFederalTaxID.ToString() : oBP.VatIDNum.ToString();// Tc kimlik veya Vergi Kimlik No
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 7;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString() : oBP.AdditionalID.ToString();
                    orderPropertiesList.Add(orderProperties);

                    InventoryTransferRequest.Properties = orderPropertiesList.ToArray();




                    InventoryTransferRequestDetailList = new List<CevaWMSQa.OrderDetail>();
                    var InventoryTransferRequestXML = XmlUtils.SerializeToXml(InventoryTransferRequest);

                    var response = service.CreateOrder(login, InventoryTransferRequest);



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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", InventoryTransferRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);


                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "S";

                        oRSUpdate.DoQuery("UPDATE OWTQ Set U_E_STATUS='S' where DocEntry = '" + InventoryTransferRequestNo + "'");
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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", InventoryTransferRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                        oRSUpdate.DoQuery("UPDATE OWTQ Set U_E_STATUS='E' where DocEntry = '" + InventoryTransferRequestNo + "'");

                    }

                    int a = oDocuments.Update();

                    string errorr = oCompany.GetLastErrorDescription();

                    results.Add(new InventoryTransferRequestResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });

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

        public List<InventoryTransferRequestResponse> SendAllInventoryTransferRequest()
        {
            List<InventoryTransferRequestResponse> results = new List<InventoryTransferRequestResponse>();
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

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Order InventoryTransferRequest = new CevaWMSQa.Order();
                CevaWMSQa.OrderDetail InventoryTransferRequestDetail = new CevaWMSQa.OrderDetail();
                List<CevaWMSQa.OrderDetail> InventoryTransferRequestDetailList = new List<CevaWMSQa.OrderDetail>();
                CevaWMSQa.OrderProperties orderProperties = new CevaWMSQa.OrderProperties();
                List<CevaWMSQa.OrderProperties> orderPropertiesList = new List<CevaWMSQa.OrderProperties>();
                SAPbobsCOM.StockTransfer oDocuments = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest);


                CevaWMSQa.Login login = new CevaWMSQa.Login();


                login = new CevaWMSQa.Login();
                InventoryTransferRequest = new CevaWMSQa.Order();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";


                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSupdate = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                oRS.DoQuery("Select \"DocEntry\" from \"OWTQ\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E')");
                int DocEntry = 0;
                while (!oRS.EoF)
                {

                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));
                    oBP.GetByKey(oDocuments.CardCode);

                    InventoryTransferRequest.CompanyCode = "47";

                    //InventoryTransferRequest.WarehouseCode = "01";

                    InventoryTransferRequest.WarehouseCode = "02"; //Online çıkışlar stok nakli ile yapılamayacağı için hep 02 verildi.

                    InventoryTransferRequest.OrderNumber = "Y" + oDocuments.DocEntry.ToString();

                    InventoryTransferRequest.OrderReferenceNumber = oDocuments.DocEntry.ToString();

                    InventoryTransferRequest.MovementCode = "204";

                    if (string.Equals(oDocuments.ToWarehouse.ToString(), "STR_01_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_02_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_03_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_04_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_05_A") || string.Equals(oDocuments.ToWarehouse.ToString(), "STR_06_A"))
                    {
                        InventoryTransferRequest.MovementCode = "202";
                        InventoryTransferRequest.WarehouseCode = "01"; //202 hareket kodunda yalnızca 01 depo kodu gönderilebilir.
                    }

                    InventoryTransferRequest.RecordType = CevaWMSQa.RecordType.New;

                    InventoryTransferRequest.DeliveryDate = oDocuments.DocDate;

                    InventoryTransferRequest.DeliveryDate = oDocuments.DocDate;

                    InventoryTransferRequest.ToDealerCode = oDocuments.CardCode;

                    InventoryTransferRequest.FromDealerCode = oDocuments.CardCode;

                    InventoryTransferRequest.OrderType = CevaWMSQa.OrderType.Delivery; // Sabit gönderilmesi istendi.

                    InventoryTransferRequest.DeliveryAddress = oDocuments.Address;

                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        InventoryTransferRequestDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                        InventoryTransferRequestDetail.ProductNumber = oDocuments.Lines.ItemCode;

                        InventoryTransferRequestDetail.LineRecordType = CevaWMSQa.RecordType.New;

                        InventoryTransferRequestDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                        InventoryTransferRequestDetailList.Add(InventoryTransferRequestDetail);

                        InventoryTransferRequestDetail = new CevaWMSQa.OrderDetail();
                    }


                    InventoryTransferRequest.Detail = InventoryTransferRequestDetailList.ToArray();




                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 1;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString()) ?
                        oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString() : oDocuments.CardName;

                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 2;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_County").Value.ToString();// ilçe
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 3;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_City").Value.ToString();// il
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 4;
                    orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_ZipCode").Value.ToString();// posta kodu
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 5;
                    orderProperties.PropertyValue1 = oBP.CardForeignName;// Firma adı
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 6;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString() : !string.IsNullOrEmpty(oBP.UnifiedFederalTaxID.ToString()) ? oBP.UnifiedFederalTaxID.ToString() : oBP.VatIDNum.ToString();// Tc kimlik veya Vergi Kimlik No
                    orderPropertiesList.Add(orderProperties);


                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 7;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString() : oBP.AdditionalID.ToString();
                    orderPropertiesList.Add(orderProperties);

                    InventoryTransferRequest.Properties = orderPropertiesList.ToArray();


                    InventoryTransferRequestDetailList = new List<CevaWMSQa.OrderDetail>();

                    var InventoryTransferRequestXML = XmlUtils.SerializeToXml(InventoryTransferRequest);

                    var response = service.CreateOrder(login, InventoryTransferRequest);



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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", InventoryTransferRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);


                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "S";

                        oRSupdate.DoQuery("UPDATE OWTQ Set U_E_STATUS='E' where DocEntry = '" + DocEntry + "'");
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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", InventoryTransferRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                        oRSupdate.DoQuery("UPDATE OWTQ Set U_E_STATUS='E' where DocEntry = '" + DocEntry + "'");

                    }

                    oDocuments.Update();

                    results.Add(new InventoryTransferRequestResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });

                    oRS.MoveNext();

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

        public List<SalesOrderResponse> sendStokNakliTalebiCikis(int inventoryrequestNo)
        {
            List<SalesOrderResponse> results = new List<SalesOrderResponse>();
            SAPbobsCOM.GeneralData oGeneralData;
            SAPbobsCOM.GeneralService oGeneralService;
            SAPbobsCOM.CompanyService oCompService = null;
            int connectionNumber = 0;
            try
            {
                ConnectionList connection = new ConnectionList();

                ZwillingCevaWebService.SAPLayer.LoginCompany log = new SAPLayer.LoginCompany();

                connection = log.setLogin();

                SAPbobsCOM.Company oCompany = null;
                oCompany = connection.oCompany;
                connectionNumber = connection.number;
                oCompService = oCompany.GetCompanyService();

                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Order orderRequest = new CevaWMSQa.Order();
                CevaWMSQa.OrderDetail orderDetail = new CevaWMSQa.OrderDetail();
                List<CevaWMSQa.OrderDetail> orderDetailList = new List<CevaWMSQa.OrderDetail>();
                CevaWMSQa.OrderProperties orderProperties = new CevaWMSQa.OrderProperties();
                List<CevaWMSQa.OrderProperties> orderPropertiesList = new List<CevaWMSQa.OrderProperties>();
                SAPbobsCOM.StockTransfer oDocuments = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest);

                CevaWMSQa.Login login = new CevaWMSQa.Login();

                login = new CevaWMSQa.Login();
                orderRequest = new CevaWMSQa.Order();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";

                SAPbobsCOM.Items oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSFindState = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                //oRS.DoQuery("Select \"DocEntry\" from \"OWTQ\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E') and \"DocEntry\" = '" + inventoryrequestNo + "'");
                oRS.DoQuery("Select \"DocEntry\",\"Filler\" from \"OWTQ\" where \"DocEntry\" = '" + inventoryrequestNo + "'");

                if (oRS.RecordCount > 0)
                {
                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));


                    oBP.GetByKey(oDocuments.CardCode);

                    orderRequest.CompanyCode = "47";

                    if (oRS.Fields.Item("Filler").Value.ToString() == "99")
                    {
                        orderRequest.WarehouseCode = "02";
                    }
                    else
                    {
                        orderRequest.WarehouseCode = "01";
                    }

                    orderRequest.OrderReferenceNumber = oDocuments.UserFields.Fields.Item("U_SASNO").Value.ToString();

                    orderRequest.MovementCode = "207";

                    orderRequest.OrderNumber = orderRequest.MovementCode.ToString() + "-C-" + oDocuments.DocEntry.ToString();

                    orderRequest.OrderReferenceNumber = orderRequest.MovementCode.ToString() + "-C-" + oDocuments.DocEntry.ToString();

                    orderRequest.RecordType = CevaWMSQa.RecordType.New;

                    orderRequest.DeliveryDate = oDocuments.DocDate;//oDocuments.DocDate.ToString("yyyy-MM-dd");

                    orderRequest.FromDealerCode = oDocuments.CardCode;

                    orderRequest.ToDealerCode = oDocuments.CardCode;

                    orderRequest.OrderType = CevaWMSQa.OrderType.Delivery; // Sabit gönderilmesi istendi.

                    orderRequest.DeliveryAddress = oDocuments.Address.ToString();

                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        oItems.GetByKey(oDocuments.Lines.ItemCode);

                        if (oItems.InventoryItem == SAPbobsCOM.BoYesNoEnum.tYES)
                        {
                            orderDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                            orderDetail.ProductNumber = oDocuments.Lines.ItemCode;

                            orderDetail.LineRecordType = CevaWMSQa.RecordType.New;

                            orderDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                            orderDetail.UnitPrice = Convert.ToDecimal(oDocuments.Lines.UnitPrice);

                            orderDetailList.Add(orderDetail);
                        }

                        orderDetail = new CevaWMSQa.OrderDetail();
                    }

                    orderRequest.Detail = orderDetailList.ToArray();

                    //Tek Seferlik müşteri ise sipariş üzerindeki adı gönderiliyor.
                    //oRSCardCode.DoQuery("Select ISNULL(\"QryGroup4\",'') from \"OCRD\" where \"CardCode\"= '" + oDocuments.CardCode + "'");

                    //if (string.Equals(oRSCardCode.Fields.Item(0).Value, "Y"))
                    //{

                    //SAPbobsCOM.Recordset oRS= 

                    //oRS.DoQuery("Select * from \"CRD1\" where \"CardCode\" = '" + oDocuments.CardCode.ToString() + "'");

                    orderProperties.PropertyCode = 1;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString()) ?
                        oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString() : oDocuments.CardName;

                    orderPropertiesList.Add(orderProperties);
                    //}

                    //orderProperties = new CevaWMSQa.OrderProperties();
                    //orderProperties.PropertyCode = 2;
                    //orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToCounty;// ilçe
                    //orderPropertiesList.Add(orderProperties);

                    //oRSFindState.DoQuery("Select \"Name\" from \"OCST\" where \"Code\" = '" + oDocuments.AddressExtension.ShipToState + "'");

                    //orderProperties = new CevaWMSQa.OrderProperties();
                    //orderProperties.PropertyCode = 3;
                    //orderProperties.PropertyValue1 = oRSFindState.Fields.Item(0).Value.ToString();// il
                    //orderPropertiesList.Add(orderProperties);

                    //orderProperties = new CevaWMSQa.OrderProperties();
                    //orderProperties.PropertyCode = 4;
                    //orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToZipCode;// posta kodu
                    //orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 5;
                    orderProperties.PropertyValue1 = oBP.CardForeignName;// Firma adı
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 6;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TcknVkn").Value.ToString() : !string.IsNullOrEmpty(oBP.UnifiedFederalTaxID.ToString()) ? oBP.UnifiedFederalTaxID.ToString() : oBP.VatIDNum.ToString();// Tc kimlik veya Vergi Kimlik No
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 7;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString()) ? oDocuments.UserFields.Fields.Item("U_TaxOffice").Value.ToString() : oBP.AdditionalID.ToString();
                    orderPropertiesList.Add(orderProperties);

                    orderRequest.Properties = orderPropertiesList.ToArray();

                    var orderRequestXML = XmlUtils.SerializeToXml(orderRequest);

                    var response = service.CreateOrder(login, orderRequest);

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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", orderRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "S";
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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", orderRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";
                    }

                    oDocuments.Update();

                    results.Add(new SalesOrderResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });
                }


                LoginCompany.ReleaseConnection(connection.number);
            }
            catch (Exception ex)
            {
                //Logger.addLog
                Logger.addLog("Siparis", string.Format("{0}|{1}|{2}", ">", DateTime.Now.ToString(), "Sipariş gönderilirken hata oluştu: " + ex.Message.ToString() + " - DocEntry: " + inventoryrequestNo));
            }
            finally
            {
                LoginCompany.ReleaseConnection(connectionNumber);
            }

            return results;
        }


        public List<SalesOrderResponse> SendStokNakliTalebiGiris(int inventoryrequestNo)
        {
            List<SalesOrderResponse> results = new List<SalesOrderResponse>();
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

                CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Asn PurchaseOrder = new CevaWMSQa.Asn();
                CevaWMSQa.AsnDetail PurchaseOrderDetail = new CevaWMSQa.AsnDetail();
                List<CevaWMSQa.AsnDetail> PurchaseOrderDetailList = new List<CevaWMSQa.AsnDetail>();
                SAPbobsCOM.StockTransfer oDocuments = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryTransferRequest);

                CevaWMSQa.Login login = new CevaWMSQa.Login();


                login = new CevaWMSQa.Login();
                PurchaseOrder = new CevaWMSQa.Asn();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";
                SAPbobsCOM.Items oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                //oRS.DoQuery("Select \"DocEntry\" from \"OPOR\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E') and \"DocNum\" = '" + PurchaseOrderNo + "'");
                oRS.DoQuery("Select \"DocEntry\",\"ToWhsCode\" from \"OWTQ\" where \"DocEntry\" = '" + inventoryrequestNo + "'");


                if (oRS.RecordCount > 0)
                {
                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));

                    PurchaseOrder.CompanyCode = "47";

                    if (oRS.Fields.Item("ToWhsCode").Value.ToString() == "99")
                    {
                        PurchaseOrder.WarehouseCode = "02";
                    }
                    else
                    {
                        PurchaseOrder.WarehouseCode = "01";
                    }

                    PurchaseOrder.AsnNumber = "107" + "-G-" + oDocuments.DocEntry.ToString();

                    PurchaseOrder.AsnReferenceNumber = "107" + "-G-" + oDocuments.DocEntry.ToString();

                    //if (string.Equals(oDocuments.AddressExtension.ShipToCountry, "TR"))
                    //    PurchaseOrder.MovementCode = "101";
                    //else
                    //    PurchaseOrder.MovementCode = "102";

                    PurchaseOrder.MovementCode = "107";

                    PurchaseOrder.RecordType = CevaWMSQa.RecordType.New;

                    PurchaseOrder.AsnDate = oDocuments.DocDate;

                    PurchaseOrder.DeliveryDate = oDocuments.DocDate;

                    PurchaseOrder.DealerCode = oDocuments.CardCode;

                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        if (oItems.InventoryItem == SAPbobsCOM.BoYesNoEnum.tYES)
                        {
                            PurchaseOrderDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                            PurchaseOrderDetail.ProductNumber = oDocuments.Lines.ItemCode;

                            PurchaseOrderDetail.LineRecordType = CevaWMSQa.RecordType.New;

                            PurchaseOrderDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                            PurchaseOrderDetailList.Add(PurchaseOrderDetail);
                        }

                        PurchaseOrderDetail = new CevaWMSQa.AsnDetail();
                    }


                    PurchaseOrder.Detail = PurchaseOrderDetailList.ToArray();

                    var PurchaseOrderXML = XmlUtils.SerializeToXml(PurchaseOrder);

                    var response = service.CreateAsn(login, PurchaseOrder);



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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", PurchaseOrderXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);


                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "S";
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
                        oGeneralData.SetProperty("U_TypeCode", "3");
                        oGeneralData.SetProperty("U_Type", "InventoryTransferRequest");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", PurchaseOrderXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                    }

                    int a = oDocuments.Update();

                    string errorr = oCompany.GetLastErrorDescription();

                    results.Add(new SalesOrderResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });
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