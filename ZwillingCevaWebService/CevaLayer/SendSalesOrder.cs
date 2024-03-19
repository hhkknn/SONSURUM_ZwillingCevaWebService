using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.SAPLayer;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.CevaLayer
{
    public class SendSalesOrder
    {
        public List<SalesOrderResponse> sendSalesOrderByCode(int salesOrderNo)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

                //CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient();
                CevaWMSQa.Order orderRequest = new CevaWMSQa.Order();
                CevaWMSQa.OrderDetail orderDetail = new CevaWMSQa.OrderDetail();
                List<CevaWMSQa.OrderDetail> orderDetailList = new List<CevaWMSQa.OrderDetail>();
                CevaWMSQa.OrderProperties orderProperties = new CevaWMSQa.OrderProperties();
                List<CevaWMSQa.OrderProperties> orderPropertiesList = new List<CevaWMSQa.OrderProperties>();
                SAPbobsCOM.Documents oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

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

                oRS.DoQuery("Select \"DocEntry\" from \"ORDR\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E') and \"DocEntry\" = '" + salesOrderNo + "'");

                if (oRS.RecordCount > 0)
                {
                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));

                    oBP.GetByKey(oDocuments.CardCode);

                    orderRequest.CompanyCode = "47";

                    orderRequest.WarehouseCode = "01";

                    orderRequest.OrderReferenceNumber = oDocuments.UserFields.Fields.Item("U_SASNO").Value.ToString();

                    orderRequest.MovementCode = "200"; //Direk Sevkiyat

                    if (!string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_IntegrationCode").Value.ToString()))
                    {
                        if (string.Equals(oDocuments.UserFields.Fields.Item("U_IntegrationCode").Value.ToString(), "4"))
                        {
                            orderRequest.MovementCode = "201"; //Almanya (SalesForce)
                        }
                        else
                        {
                            orderRequest.MovementCode = "203"; // Zwilling E-ticaret Sevkiyat (Trendyol,N11 vb.)
                        }
                    }
                    else
                    {
                        //orderRequest.WarehouseCode = "02"; //Online sipariş değilse direk sevkiyat olarak whosale depodan çıkış yapılır.  
                    }

                    orderRequest.OrderNumber = "Y" + orderRequest.MovementCode.ToString() + "-" + oDocuments.DocEntry.ToString();

                    orderRequest.RecordType = CevaWMSQa.RecordType.New;

                    orderRequest.DeliveryDate = oDocuments.DocDate;//oDocuments.DocDate.ToString("yyyy-MM-dd");

                    orderRequest.FromDealerCode = oDocuments.CardCode;

                    orderRequest.ToDealerCode = oDocuments.CardCode;

                    orderRequest.OrderType = CevaWMSQa.OrderType.Delivery; // Sabit gönderilmesi istendi.

                    orderRequest.DeliveryAddress = oDocuments.Address2 + " " + oDocuments.AddressExtension.ShipToStreetNo.ToString() + " " + oDocuments.AddressExtension.ShipToBlock.ToString();

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
                    orderProperties.PropertyCode = 1;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString()) ?
                        oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString() : oDocuments.CardName;

                    orderPropertiesList.Add(orderProperties);
                    //}

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 2;
                    orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToCounty;// ilçe
                    orderPropertiesList.Add(orderProperties);

                    oRSFindState.DoQuery("Select \"Name\" from \"OCST\" where \"Code\" = '" + oDocuments.AddressExtension.ShipToState + "'");

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 3;
                    orderProperties.PropertyValue1 = oRSFindState.Fields.Item(0).Value.ToString();// il
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 4;
                    orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToZipCode;// posta kodu
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

                    if (oDocuments.UserFields.Fields.Item("U_HediyeMi").Value.ToString() == "E")
                    {
                        orderProperties = new CevaWMSQa.OrderProperties();
                        orderProperties.PropertyCode = 9;
                        orderProperties.PropertyValue1 = "Evet";
                        orderPropertiesList.Add(orderProperties);


                        orderProperties = new CevaWMSQa.OrderProperties();
                        orderProperties.PropertyCode = 10;
                        orderProperties.PropertyValue1 = oDocuments.UserFields.Fields.Item("U_HediyeAcklm").Value.ToString();
                        orderPropertiesList.Add(orderProperties);
                    }
                    else
                    {
                        orderProperties = new CevaWMSQa.OrderProperties();
                        orderProperties.PropertyCode = 9;
                        orderProperties.PropertyValue1 = "Hayır";
                        orderPropertiesList.Add(orderProperties);
                    }


                    orderRequest.Properties = orderPropertiesList.ToArray();

                    var orderRequestXML = XmlUtils.SerializeToXml(orderRequest); 

                    using (CevaWMSQa.CEVASoapClient service = new CevaWMSQa.CEVASoapClient())
                    {
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
                            oGeneralData.SetProperty("U_TypeCode", "4");
                            oGeneralData.SetProperty("U_Type", "SalesOrder");
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
                            oGeneralData.SetProperty("U_TypeCode", "4");
                            oGeneralData.SetProperty("U_Type", "SalesOrder");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", orderRequestXML);
                            oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                            oGeneralData.SetProperty("U_Status", "E");

                            oGeneralService.Add(oGeneralData);

                            oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";
                        }
                        results.Add(new SalesOrderResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });
                    }

                    oDocuments.Update();

                }

                LoginCompany.ReleaseConnection(connection.number);
            }
            catch (Exception ex)
            {
                //Logger.addLog
                Logger.addLog("Siparis", string.Format("{0}|{1}|{2}", ">", DateTime.Now.ToString(), "Sipariş gönderilirken hata oluştu: " + ex.Message.ToString() + " - DocEntry: " + salesOrderNo));
            }
            finally
            {
                LoginCompany.ReleaseConnection(connectionNumber);
            }

            return results;
        }

        public List<SalesOrderResponse> sendSalesOrder()
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
                CevaWMSQa.Order orderRequest = new CevaWMSQa.Order();
                CevaWMSQa.OrderDetail orderDetail = new CevaWMSQa.OrderDetail();
                List<CevaWMSQa.OrderDetail> orderDetailList = new List<CevaWMSQa.OrderDetail>();
                CevaWMSQa.OrderProperties orderProperties = new CevaWMSQa.OrderProperties();
                List<CevaWMSQa.OrderProperties> orderPropertiesList = new List<CevaWMSQa.OrderProperties>();
                SAPbobsCOM.Documents oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                CevaWMSQa.Login login = new CevaWMSQa.Login();

                login = new CevaWMSQa.Login();
                orderRequest = new CevaWMSQa.Order();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSFindState = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

                oRS.DoQuery("Select \"DocEntry\" from \"ORDR\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E')");

                while (!oRS.EoF)
                {
                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));
                    oBP.GetByKey(oDocuments.CardCode);

                    orderRequest.CompanyCode = "47";

                    orderRequest.WarehouseCode = "01";

                    orderRequest.OrderNumber = "Y" + orderRequest.MovementCode.ToString() + "-" + oDocuments.DocEntry.ToString();//oDocuments.DocEntry.ToString();

                    orderRequest.OrderReferenceNumber = oDocuments.UserFields.Fields.Item("U_SASNO").Value.ToString();

                    orderRequest.MovementCode = "200"; //Direk Sevkiyat

                    if (!string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_IntegrationCode").Value.ToString()))
                    {
                        if (string.Equals(oDocuments.UserFields.Fields.Item("U_IntegrationCode").Value.ToString(), "4"))
                        {
                            orderRequest.MovementCode = "201"; //Almanya (SalesForce)
                        }
                        else
                        {
                            orderRequest.MovementCode = "203"; // Zwilling E-ticaret Sevkiyat (Trendyol,N11 vb.)
                        }
                    }
                    else
                    { 
                        //orderRequest.WarehouseCode = "02";//Online sipariş değilse direk sevkiyat olarak whosale depodan çıkış yapılır.  
                    }

                    orderRequest.RecordType = CevaWMSQa.RecordType.New;

                    orderRequest.DeliveryDate = oDocuments.DocDate;//oDocuments.DocDate.ToString("yyyy-MM-dd");

                    orderRequest.FromDealerCode = oDocuments.CardCode; //Bu ne olacak sorulacak.

                    orderRequest.ToDealerCode = oDocuments.CardCode;

                    orderRequest.OrderType = CevaWMSQa.OrderType.Delivery; // Sabit gönderilmesi istendi.

                    orderRequest.DeliveryAddress = oDocuments.Address2 + " " + oDocuments.AddressExtension.ShipToStreetNo.ToString() + " " + oDocuments.AddressExtension.ShipToBlock.ToString();

                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        orderDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                        orderDetail.ProductNumber = oDocuments.Lines.ItemCode;

                        orderDetail.LineRecordType = CevaWMSQa.RecordType.New;

                        orderDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                        orderDetail.UnitPrice = Convert.ToDecimal(oDocuments.Lines.UnitPrice);

                        orderDetailList.Add(orderDetail);

                        orderDetail = new CevaWMSQa.OrderDetail();
                    }
                    orderRequest.Detail = orderDetailList.ToArray();

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 1;
                    orderProperties.PropertyValue1 = !string.IsNullOrEmpty(oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString()) ?
                        oDocuments.UserFields.Fields.Item("U_AliciAdi").Value.ToString() : oDocuments.CardName;

                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 2;
                    orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToCounty;// ilçe
                    orderPropertiesList.Add(orderProperties);

                    oRSFindState.DoQuery("Select \"Name\" from \"OCST\" where \"Code\" = '" + oDocuments.AddressExtension.ShipToState + "'");

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 3;
                    orderProperties.PropertyValue1 = oRSFindState.Fields.Item(0).Value.ToString();// il
                    orderPropertiesList.Add(orderProperties);

                    orderProperties = new CevaWMSQa.OrderProperties();
                    orderProperties.PropertyCode = 4;
                    orderProperties.PropertyValue1 = oDocuments.AddressExtension.ShipToZipCode;// posta kodu
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
                        oGeneralData.SetProperty("U_TypeCode", "4");
                        oGeneralData.SetProperty("U_Type", "SalesOrder");
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
                        oGeneralData.SetProperty("U_TypeCode", "4");
                        oGeneralData.SetProperty("U_Type", "SalesOrder");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", orderRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";
                    }

                    oDocuments.Update();

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


    }
}