using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.SAPLayer;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.CevaLayer
{
    public class SendPurchaseOrder
    {
        public List<PurchaseOrderResponse> SendPurchaseOrderByCode(int PurchaseOrderNo)
        {
            List<PurchaseOrderResponse> results = new List<PurchaseOrderResponse>();
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
                SAPbobsCOM.Documents oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);

                CevaWMSQa.Login login = new CevaWMSQa.Login();


                login = new CevaWMSQa.Login();
                PurchaseOrder = new CevaWMSQa.Asn();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";
                SAPbobsCOM.Items oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"DocEntry\" from \"OPOR\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E') and \"DocNum\" = '" + PurchaseOrderNo + "'");


                if (oRS.RecordCount > 0)
                {

                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));

                    PurchaseOrder.CompanyCode = "47";

                    if (oDocuments.Lines.WarehouseCode == "99")
                    {
                        PurchaseOrder.WarehouseCode = "02";
                    }
                    else
                    { 
                        PurchaseOrder.WarehouseCode = "01";
                    }

                    PurchaseOrder.AsnNumber = "Y" + oDocuments.DocEntry.ToString();

                    PurchaseOrder.AsnReferenceNumber = "Y" + oDocuments.DocEntry.ToString();

                    if (string.Equals(oDocuments.AddressExtension.ShipToCountry, "TR"))
                        PurchaseOrder.MovementCode = "101";
                    else
                        PurchaseOrder.MovementCode = "102";


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
                        oGeneralData.SetProperty("U_TypeCode", "5");
                        oGeneralData.SetProperty("U_Type", "PurchaseOrder");
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
                        oGeneralData.SetProperty("U_TypeCode", "5");
                        oGeneralData.SetProperty("U_Type", "PurchaseOrder");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", PurchaseOrderXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                    }

                    int a = oDocuments.Update();

                    string errorr = oCompany.GetLastErrorDescription();

                    results.Add(new PurchaseOrderResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });

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

        public List<PurchaseOrderResponse> SendAllPurchaseOrder()
        {
            List<PurchaseOrderResponse> results = new List<PurchaseOrderResponse>();
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
                SAPbobsCOM.Documents oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);

                CevaWMSQa.Login login = new CevaWMSQa.Login();

                PurchaseOrder = new CevaWMSQa.Asn();
                login = new CevaWMSQa.Login();
                PurchaseOrder = new CevaWMSQa.Asn();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";


                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"DocEntry\" from \"OPOR\" where (ISNULL(\"U_E_STATUS\",'') = 'N' or ISNULL(\"U_E_STATUS\",'') = 'E')");

                while (!oRS.EoF)
                {

                    oDocuments.GetByKey(Convert.ToInt32(oRS.Fields.Item(0).Value));

                    PurchaseOrder.CompanyCode = "47";

                    if (oDocuments.Lines.WarehouseCode == "99")
                    {
                        PurchaseOrder.WarehouseCode = "02";
                    }
                    else
                    {
                        PurchaseOrder.WarehouseCode = "01";
                    }

                    PurchaseOrder.AsnNumber = "Y" + oDocuments.DocEntry.ToString();

                    PurchaseOrder.AsnReferenceNumber = "Y" + oDocuments.DocEntry.ToString();

                    //PurchaseOrder.MovementCode = "101";
                    if (string.Equals(oDocuments.AddressExtension.ShipToCountry, "TR"))
                        PurchaseOrder.MovementCode = "101";
                    else
                        PurchaseOrder.MovementCode = "102";

                    PurchaseOrder.RecordType = CevaWMSQa.RecordType.New;

                    PurchaseOrder.AsnDate = oDocuments.DocDate;

                    PurchaseOrder.DeliveryDate = oDocuments.DocDate;

                    PurchaseOrder.DealerCode = oDocuments.CardCode;


                    for (int i = 0; i < oDocuments.Lines.Count; i++)
                    {
                        oDocuments.Lines.SetCurrentLine(i);

                        PurchaseOrderDetail.LineNumber = string.IsNullOrEmpty(oDocuments.Lines.LineNum.ToString()) ? "0" : oDocuments.Lines.LineNum.ToString();

                        PurchaseOrderDetail.ProductNumber = oDocuments.Lines.ItemCode;

                        PurchaseOrderDetail.LineRecordType = CevaWMSQa.RecordType.New;

                        PurchaseOrderDetail.Qty = Convert.ToDecimal(oDocuments.Lines.Quantity);

                        PurchaseOrderDetailList.Add(PurchaseOrderDetail);

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
                        oGeneralData.SetProperty("U_TypeCode", "5");
                        oGeneralData.SetProperty("U_Type", "PurchaseOrder");
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
                        oGeneralData.SetProperty("U_TypeCode", "5");
                        oGeneralData.SetProperty("U_Type", "PurchaseOrder");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", PurchaseOrderXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oDocuments.UserFields.Fields.Item("U_E_STATUS").Value = "E";

                    }

                    oDocuments.Update();

                    results.Add(new PurchaseOrderResponse { DocumentNumber = oCompany.GetNewObjectKey(), response = response.ResultDescription });

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