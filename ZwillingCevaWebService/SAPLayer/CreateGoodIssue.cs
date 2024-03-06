using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.SAPLayer
{
    public class CreateGoodIssue
    {
        public SAPLayer.Response CreateGoodIssueAll(SAPLayer.GoodIssue GoodIssue)
        {
            SAPLayer.Response response = new Response();
            int connectionNumber = 0;
            try
            {
                #region Şirket bağlantısı ve değişkenler.

                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralService oGeneralService;
                SAPbobsCOM.CompanyService oCompService = null;
                ConnectionList connection = new ConnectionList();
                ZwillingCevaWebService.SAPLayer.LoginCompany log = new SAPLayer.LoginCompany();
                List<SAPLayer.Response> responseList = new List<SAPLayer.Response>();

                connection = log.setLogin();

                SAPbobsCOM.Company oCompany = connection.oCompany;
                connectionNumber = connection.number;

                SAPbobsCOM.Documents oDocuments = null;
                SAPbobsCOM.Documents oInvoice = null;
                SAPbobsCOM.StockTransfer oStockTransfer = null;

                oCompService = oCompany.GetCompanyService();
                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                #endregion Şirket bağlantısı ve değişkenler.

                #region Iade işlemlerinde tabloya veri atılıyor

                //if (string.Equals(GoodIssue.MovementCode, "103") || string.Equals(GoodIssue.MovementCode, "104") || string.Equals(GoodIssue.MovementCode, "105")) //Iade işlemlerinde tabloya veri atılıyor.
                //{
                //    string ret = "";
                //    ret = addReturnDetails(oCompany, GoodIssue);

                //    if (string.IsNullOrEmpty(ret))
                //    {
                //        response = new SAPLayer.Response();
                //        response.TransactionNumber = GoodIssue.OrderNumber;
                //        response.Successful = true;
                //        response.ResultDescription = "Veri Gönderimi Başarılı";

                //        LoginCompany.ReleaseConnection(connection.number);

                //        return response;
                //    }
                //    else
                //    {
                //        response = new SAPLayer.Response();
                //        response.TransactionNumber = GoodIssue.OrderNumber;
                //        response.Successful = false;
                //        response.ResultDescription = oCompany.GetLastErrorDescription();

                //        LoginCompany.ReleaseConnection(connection.number);

                //        return response;
                //    }
                //}

                #endregion Iade işlemlerinde tabloya veri atılıyor

                bool isLineExist = false;

                #region Teslimat ve Stok nakilleri oluşuyor.

                if (GoodIssue.MovementCode != "204" && GoodIssue.MovementCode != "202" && GoodIssue.MovementCode != "107") //Teslimat oluşur.
                {
                    #region Teslimat

                    oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                    oInvoice = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    oDocuments.BPL_IDAssignedToInvoice = 1;

                    oDocuments.CardCode = GoodIssue.ToDealerCode;

                    oDocuments.DocDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());

                    oDocuments.DocDueDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());

                    oDocuments.TaxDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());

                    int tempBaseEntry = 0;
                    foreach (var item in GoodIssue.Detail)
                    {
                        if (item.Qty == 0) //0 Miktarlı bir veri ise o satır dahil edilmiyor.
                        {
                            continue;
                        }

                        oDocuments.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oOrders;

                        try
                        {
                            if (GoodIssue.OrderNumber.Contains("-")) //Bu kontrol daha önceki başına 200-201-202 koyulmamış olan veriler için yapıldı.
                            {
                                //GoodIssue.OrderNumber = GoodIssue.OrderNumber.Remove(0, 4).ToString();
                                GoodIssue.OrderNumber = GoodIssue.OrderNumber.Remove(0, 5).ToString(); // Başına yeni bir ek geldiği için 5 e çıkarıldı. Yeni Ek ('Y').
                                oDocuments.Lines.BaseEntry = Convert.ToInt32(GoodIssue.OrderNumber);
                            }
                            else
                                oDocuments.Lines.BaseEntry = Convert.ToInt32(GoodIssue.OrderNumber.Replace("Y", ""));

                            tempBaseEntry = oDocuments.Lines.BaseEntry;
                        }
                        catch (Exception ex)
                        {
                            response = new SAPLayer.Response();
                            response.TransactionNumber = GoodIssue.OrderNumber;
                            response.Successful = false;
                            response.ResultDescription = oCompany.GetLastErrorDescription();
                        }

                        oDocuments.Lines.BaseLine = Convert.ToInt32(item.LineNumber);

                        oDocuments.Lines.Quantity = Convert.ToDouble(item.Qty);

                        isLineExist = true;

                        oDocuments.Lines.ItemCode = item.ProductNumber;

                        oDocuments.Lines.Add();
                    }

                    if (tempBaseEntry > 0)
                    {
                        SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                        oRS.DoQuery("Select T0.\"LineNum\" from \"RDR1\" as T0 INNER JOIN OITM as T1 ON T0.\"ItemCode\" = T1.\"ItemCode\" where T0.\"DocEntry\" = '" + tempBaseEntry + "' and T1.\"InvntItem\" = 'N'");

                        while (!oRS.EoF)
                        {
                            oDocuments.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oOrders;
                            oDocuments.Lines.BaseEntry = Convert.ToInt32(tempBaseEntry);
                            oDocuments.Lines.BaseLine = Convert.ToInt32(oRS.Fields.Item(0).Value);
                            oDocuments.Lines.Add();
                            oRS.MoveNext();
                        }
                    }

                    if (isLineExist)
                    {
                        if (GoodIssue.WaybillNumber != null)
                        {
                            oDocuments.UserFields.Fields.Item("U_IRSALIYE").Value = GoodIssue.WaybillNumber.ToString();
                        }

                        int retval = oDocuments.Add();

                        var GoodIssueXML = XmlUtils.SerializeToXml(GoodIssue);
                        if (retval != 0)
                        {
                            oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                            oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            string maxcode = "";
                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("Code", maxcode);
                            oGeneralData.SetProperty("U_TypeCode", "7");
                            oGeneralData.SetProperty("U_Type", "GoodIssue");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                            oGeneralData.SetProperty("U_SAPResp", oCompany.GetLastErrorCode() + " " + oCompany.GetLastErrorDescription());
                            oGeneralData.SetProperty("U_Status", "E");

                            oGeneralService.Add(oGeneralData);

                            response = new SAPLayer.Response();
                            response.TransactionNumber = GoodIssue.OrderNumber;
                            response.Successful = false;
                            response.ResultDescription = oCompany.GetLastErrorDescription();

                            //responseList.Add(response);
                            //GetGoodIssueResponse.Body = new CevaReturn.GetGoodIssueResponseBody();
                            //GetGoodIssueResponse.Body.GetGoodIssueResult = new CevaReturn.Response();
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.TransactionNumber = GoodIssue.Body.goodIssue.OrderNumber;
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.Successful = false;
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.ResultDescription = oCompany.GetLastErrorDescription();
                        }
                        else
                        {
                            oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                            oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            string maxcode = "";
                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("Code", maxcode);
                            oGeneralData.SetProperty("U_TypeCode", "7");
                            oGeneralData.SetProperty("U_Type", "GoodIssue");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                            oGeneralData.SetProperty("U_SAPResp", "Success");
                            oGeneralData.SetProperty("U_Status", "S");

                            oGeneralService.Add(oGeneralData);

                            int OrderNo = Convert.ToInt32(oCompany.GetNewObjectKey());

                            string tempOrderNo = "";
                            SAPbobsCOM.Documents oDocs = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                            #region Kargo Gönderimi

                            string integrationType = "";
                            try
                            {
                                oDocs.GetByKey(OrderNo);
                                integrationType = oDocs.UserFields.Fields.Item("U_IntegrationCode").Value.ToString();
                                tempOrderNo = oDocs.UserFields.Fields.Item("U_SASNO").Value.ToString();
                                AIFCargoService.AIFCargoWebServicesSoapClient AIFWS = new AIFCargoService.AIFCargoWebServicesSoapClient();
                                AIFCargoService.YKSendOrderRequest request = new AIFCargoService.YKSendOrderRequest();
                                List<AIFCargoService.YKSendOrderRequest> requestlist = new List<AIFCargoService.YKSendOrderRequest>();

                                AIFCargoService.YKLogin login = new AIFCargoService.YKLogin();
                                login.userLanguage = "TR";
                                login.wsUserName = "9060N435272833G";
                                login.wsPassword = "va5N012zpxVU81FJ";
                                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                                string ORDR = "select  T0.DocEntry,T0.DocNum,T0.DocType,CONVERT(VARCHAR, T0.DocDate, 112) as DocDate,CONVERT(VARCHAR, T0.DocDueDate, 112) as DocDueDate,T0.DocCur,T0.DocRate,T0.DocStatus,T0.CardCode,T0.CardName,T0.Address,T0.Address2,T0.NumAtCard,T0.DiscPrcnt,T0.DiscSum,T0.DiscSumFC,T0.Ref1,T0.Ref2,T0.Comments,T0.TrnspCode,CONVERT(VARCHAR, T0.TaxDate, 112) as TaxDate,T0.ShipToCode,T0.U_AliciAdi,T0.U_AliciTelefon, (T1.StreetS + ' ' + T1.CountyS +'/' + T1.CityS) Adres,T1.CityS as \"İl\",T1.CountyS as \"İlce\" from ORDR AS T0 INNER JOIN RDR12 T1 ON T0.[DocEntry] = T1.[DocEntry] Where ISNULL(T0.U_AktarimDurum,'')!='2' and T0.DocEntry='" + OrderNo + "' and ISNULL(T0.U_TrackingNo,'')='' order by T0.DocEntry Desc ";
                                oRS.DoQuery(ORDR);

                                request.cargoKey = "Y" + OrderNo.ToString();
                                request.invoiceKey = "Y" + OrderNo.ToString();
                                request.receiverAddress = oRS.Fields.Item("Adres").Value.ToString();
                                request.receiverCustName = oRS.Fields.Item("U_AliciAdi").Value.ToString();
                                request.receiverPhone1 = oRS.Fields.Item("U_AliciTelefon").Value.ToString();
                                request.cityName = oRS.Fields.Item("İl").Value.ToString();
                                request.townName = oRS.Fields.Item("İlce").Value.ToString();

                                requestlist.Add(request);

                                var resp2 = AIFWS.SendOrderToYK(login, requestlist.ToArray());

                                //if (resp2[0].errCode == null && resp2[0].OutResult == "Başarılı.")
                                if (resp2[0].errCode == null)
                                {
                                    for (int i = 0; i < oDocs.Lines.Count; i++)
                                    {
                                        oDocs.Lines.SetCurrentLine(i);
                                        oDocs.Lines.UserFields.Fields.Item("U_KargoNumarasi").Value = resp2[0].jobId.ToString();
                                        oDocs.Lines.UserFields.Fields.Item("U_KargoFirmasi").Value = "Yurtiçi Kargo";
                                    }
                                    oDocs.UserFields.Fields.Item("U_AktarimDurum").Value = "2";
                                    oDocs.UserFields.Fields.Item("U_TrackingNo").Value = resp2[0].jobId.ToString();
                                    oDocs.UserFields.Fields.Item("U_CargoFirm").Value = "YK";
                                    var r = oDocs.Update();

                                    if (r != 0)
                                    {
                                        setLog("4", "E", tempOrderNo + " numaralı sipariş belgesi güncellenirken hata oluştu." + oCompany.GetLastErrorDescription(), "", tempOrderNo, "", "", oCompany);
                                    }
                                    else
                                    {
                                        //string retval = SendMailToBarCode(Convert.ToString(oDocs.DocNum), oDocs.UserFields.Fields.Item("U_AliciAdi").Value.ToString());

                                        //if (string.IsNullOrEmpty(retval))
                                        //{
                                        //    //setLog("4", "E", tempOrderNo + " numaralı sipariş belgesi güncellenirken hata oluştu." + Program.oCompany.GetLastErrorDescription(), "", tempOrderNo, "", "");
                                        //}
                                        //else
                                        //{
                                        //    setLog("4", "E", tempOrderNo + " numaralı sipariş belgesi için mail gönderimi sırasında hata oluştu." + retval, "", tempOrderNo, "", "");
                                        //}
                                    }
                                }
                                else
                                {
                                    oDocs.UserFields.Fields.Item("U_AktarimDurum").Value = "1";
                                    var r = oDocs.Update();

                                    setLog("4", "E", tempOrderNo + " numaralı sipariş kargoya gönderilirken hata aldı." + resp2[0].OutResult.ToString(), "", tempOrderNo, "", "", oCompany);
                                }
                                //AIFCargoService.YKSendOrderRequest
                                //AIFWS.SendOrderToYK()
                                //YurticiWS.YurticiIntegrationSoapClient YKWs = new YurticiWS.YurticiIntegrationSoapClient();
                                //var result = YKWs.CreateShipment(belgeNo);
                                //if (result.errorCode != "0")
                                //{
                                //    setLog("4", "E", tempOrderNo + " numaralı sipariş kargoya gönderilirken hata aldı." + result.errorDesc.ToString(), "", tempOrderNo, "", "");
                                //}
                            }
                            catch (Exception ex)
                            {
                                oDocs.UserFields.Fields.Item("U_AktarimDurum").Value = "1";
                                var r = oDocs.Update();
                                setLog("4", "E", tempOrderNo + " numaralı sipariş kargoya gönderilirken hata aldı." + ex.Message.ToString(), "", tempOrderNo, "", "", oCompany);
                            }

                            #endregion Kargo Gönderimi

                            #region Mail ile bildirim

                            try
                            {
                                if (integrationType == "5")
                                {
                                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                                    oDocs.GetByKey(OrderNo);

                                    SmtpClient SMTP = new SmtpClient();
                                    MailMessage Mail = new MailMessage();

                                    string body = string.Empty;
                                    string bodybase = "";

                                    using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\kargosiparis.html"))
                                    {
                                        body = reader.ReadToEnd();
                                        bodybase = body;
                                    }

                                    SMTP = new SmtpClient();
                                    Mail = new MailMessage();

                                    Mail.From = new MailAddress("marketplace.info@zwilling.com.tr");
                                    Mail.Subject = "Amazon siparişi kargo bildirimi";
                                    Mail.IsBodyHtml = true;

                                    body = mailGovdesiOlustur(oDocs.UserFields.Fields.Item("U_SASNO").Value.ToString(), oDocs.UserFields.Fields.Item("U_AliciAdi").Value.ToString(), oDocs.DocTotal);

                                    Mail.Body = body;

                                    string To = "hakan.yildiz@aifteam.com";

                                    Mail.To.Add(To);

                                    SMTP.Host = "smtp-mail.outlook.com";
                                    SMTP.DeliveryMethod = SmtpDeliveryMethod.Network;
                                    SMTP.EnableSsl = true;
                                    SMTP.UseDefaultCredentials = false;
                                    SMTP.Credentials = new System.Net.NetworkCredential("marketplace.info@zwilling.com.tr", "1DYihNiW@dsb");
                                    SMTP.Port = 587;
                                    SMTP.Send(Mail);
                                }
                            }
                            catch (Exception ex)
                            {
                                setLog("4", "E", tempOrderNo + " numaralı sipariş için mail gönderilirken hata oluştu." + ex.Message.ToString(), "", tempOrderNo, "", "", oCompany);

                            }

                            #endregion Mail ile bildirim

                            //responseList.Add(response);

                            //GetGoodIssueResponse.Body = new CevaReturn.GetGoodIssueResponseBody();
                            //GetGoodIssueResponse.Body.GetGoodIssueResult = new CevaReturn.Response();
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.TransactionNumber = GoodIssue.Body.goodIssue.OrderNumber;
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.Successful = true;
                            //GetGoodIssueResponse.Body.GetGoodIssueResult.ResultDescription = "Veri Gönderimi Başarılı";
                            if (GoodIssue.MovementCode != "204" && GoodIssue.MovementCode != "202")
                            //if (GoodIssue.MovementCode != "200")
                            {
                                oDocuments.GetByKey(Convert.ToInt32(oCompany.GetNewObjectKey()));
                                oInvoice = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
                                oInvoice.CardCode = oDocuments.CardCode;
                                oInvoice.BPL_IDAssignedToInvoice = oDocuments.BPL_IDAssignedToInvoice;
                                oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = oDocuments.UserFields.Fields.Item("U_eBelgeTipi").Value;
                                oInvoice.UserFields.Fields.Item("U_Aliass").Value = oDocuments.UserFields.Fields.Item("U_Aliass").Value;

                                for (int i = 0; i < oDocuments.Lines.Count; i++)
                                {
                                    oDocuments.Lines.SetCurrentLine(i);

                                    oInvoice.Lines.BaseEntry = oDocuments.DocEntry;

                                    oInvoice.Lines.BaseLine = oDocuments.Lines.LineNum;

                                    oInvoice.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oDeliveryNotes;

                                    oInvoice.Lines.Add();
                                }

                                #region E-Belge Tipi alanı güncelleniyor.

                                if (oDocuments.UserFields.Fields.Item("U_eBelgeTipi").Value.ToString() == "")
                                {
                                    SAPbobsCOM.Recordset oRecordMukellefListesi = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                    SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                                    string CardCode = oDocuments.CardCode;
                                    oBP.GetByKey(CardCode);
                                    string condition = oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "IFNULL" : "ISNULL";
                                    string lictradNum = "";
                                    lictradNum = oBP.UnifiedFederalTaxID;

                                    if (lictradNum != "")
                                    {
                                        oRecordMukellefListesi.DoQuery("Select " + condition + "(\"U_Alias\",'') from \"@DON_EINVCUSTLIST\" where \"U_identifier\" = '" + lictradNum + "' and \"U_Alias\" like '%pk%' and \"U_CustomerType\" = 'FT'");

                                        if (oRecordMukellefListesi.RecordCount > 0)
                                        {
                                            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "F";
                                            oInvoice.UserFields.Fields.Item("U_Aliass").Value = oRecordMukellefListesi.Fields.Item(0).Value.ToString();
                                        }
                                        else
                                        {
                                            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                        }
                                    }
                                    else
                                    {
                                        oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                    }
                                }

                                #endregion E-Belge Tipi alanı güncelleniyor.

                                #region old

                                //SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                //SAPbobsCOM.Recordset oRecordMukellefListesi = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                                //SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                                //string CardCode = oDocuments.CardCode;
                                //oBP.GetByKey(CardCode);
                                //string lictradNum = "";
                                //string condition = oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB ? "IFNULL" : "ISNULL";
                                //if (oBP.get_Properties(63) == SAPbobsCOM.BoYesNoEnum.tYES)
                                //{
                                //    oRS.DoQuery("SELECT DISTINCT \"U_LicTradNum\" FROM \"@CUSTOMER\" WHERE \"Code\" = '" + oInvoice.UserFields.Fields.Item("U_CustomerCode").Value.ToString() + "'");

                                //    lictradNum = oRS.Fields.Item(0).Value.ToString();

                                //    if (lictradNum != "")
                                //    {
                                //        oRecordMukellefListesi.DoQuery("Select " + condition + "(\"U_Alias\",'') from \"@DON_EINVCUSTLIST\" where \"U_identifier\" = '" + lictradNum + "' and \"U_Alias\" like '%pk%' and \"U_CustomerType\" = 'FT'");

                                //        if (oRecordMukellefListesi.RecordCount > 0)
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "F";
                                //            oInvoice.UserFields.Fields.Item("U_Aliass").Value = oRecordMukellefListesi.Fields.Item(0).Value.ToString();
                                //        }
                                //        else
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //        }
                                //    }
                                //    else
                                //    {
                                //        oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //    }
                                //}
                                //else if (oBP.get_Properties(61) == SAPbobsCOM.BoYesNoEnum.tYES)
                                //{
                                //    lictradNum = oInvoice.UserFields.Fields.Item("U_TcknVkn").Value.ToString();

                                //    if (lictradNum != "")
                                //    {
                                //        oRecordMukellefListesi.DoQuery("Select " + condition + "(\"U_Alias\",'') from \"@DON_EINVCUSTLIST\" where \"U_identifier\" = '" + lictradNum + "' and \"U_Alias\" like '%pk%' and \"U_CustomerType\" = 'FT'");

                                //        if (oRecordMukellefListesi.RecordCount > 0)
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "F";
                                //            oInvoice.UserFields.Fields.Item("U_Aliass").Value = oRecordMukellefListesi.Fields.Item(0).Value.ToString();
                                //        }
                                //        else
                                //        {
                                //            if (oInvoice.UserFields.Fields.Item("U_IntegrationCode").Value.ToString() != "")
                                //            {
                                //                oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "I";
                                //            }
                                //            else
                                //            {
                                //                oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //            }
                                //        }
                                //    }
                                //    else
                                //    {
                                //        if (oInvoice.UserFields.Fields.Item("U_IntegrationCode").Value.ToString() != "")
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "I";
                                //        }
                                //        else
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    lictradNum = oBP.UnifiedFederalTaxID;

                                //    if (lictradNum != "")
                                //    {
                                //        oRecordMukellefListesi.DoQuery("Select " + condition + "(\"U_Alias\",'') from \"@DON_EINVCUSTLIST\" where \"U_identifier\" = '" + lictradNum + "' and \"U_Alias\" like '%pk%' and \"U_CustomerType\" = 'FT'");

                                //        if (oRecordMukellefListesi.RecordCount > 0)
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "F";
                                //            oInvoice.UserFields.Fields.Item("U_Aliass").Value = oRecordMukellefListesi.Fields.Item(0).Value.ToString();
                                //        }
                                //        else
                                //        {
                                //            oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //        }
                                //    }
                                //    else
                                //    {
                                //        oInvoice.UserFields.Fields.Item("U_eBelgeTipi").Value = "A";
                                //    }
                                //}

                                #endregion old

                                retval = oInvoice.Add();
                            }

                            response = new SAPLayer.Response();
                            response.TransactionNumber = GoodIssue.OrderNumber;
                            response.Successful = true;
                            response.ResultDescription = "Veri Gönderimi Başarılı";

                            try
                            {
                                oGeneralService = oCompService.GetGeneralService("DON_CARRIER");
                                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                                maxcode = "";
                                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@DON_CARRIER\"");
                                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                    maxcode = "1";
                                else
                                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                                oGeneralData.SetProperty("DocNum", maxcode);
                                oGeneralData.SetProperty("U_CarrierType", "1");
                                oGeneralData.SetProperty("U_BaseDocType", "15");
                                oGeneralData.SetProperty("U_BaseDocEntry", OrderNo);

                                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                                oRS.DoQuery("Select \"U_CargoFirm\",ISNULL(\"QryGroup61\",'N') as \"QryGroup61\"  from \"ODLN\" as T0 INNER JOIN OCRD as T1 ON T0.\"CardCode\" = T1.\"CardCode\"  where T0.\"DocEntry\"='" + OrderNo + "' ");

                                if (oRS.Fields.Item("QryGroup61").Value.ToString() == "Y")
                                {
                                    if (oRS.Fields.Item(0).Value.ToString() == "UPS")
                                    {
                                        oGeneralData.SetProperty("U_CarrierVKN", "8400105133");
                                        oGeneralData.SetProperty("U_CarrierTitle", "UPS Hızlı Kargo Taşımacılığı A.Ş.");

                                        oGeneralService.Add(oGeneralData);
                                    }
                                    else
                                    {
                                        oGeneralData.SetProperty("U_CarrierVKN", "9860008925");
                                        oGeneralData.SetProperty("U_CarrierTitle", "YURTİÇİ KARGO SERVİSİ A.Ş.");

                                        oGeneralService.Add(oGeneralData);
                                    }
                                }
                                else
                                {
                                    if (oRS.Fields.Item(0).Value.ToString() == "YK")
                                    {
                                        oGeneralData.SetProperty("U_CarrierVKN", "9860008925");
                                        oGeneralData.SetProperty("U_CarrierTitle", "YURTİÇİ KARGO SERVİSİ A.Ş.");

                                        oGeneralService.Add(oGeneralData);
                                    }
                                    else if (oRS.Fields.Item(0).Value.ToString() == "UPS")
                                    {
                                        oGeneralData.SetProperty("U_CarrierVKN", "8400105133");
                                        oGeneralData.SetProperty("U_CarrierTitle", "UPS Hızlı Kargo Taşımacılığı A.Ş.");

                                        oGeneralService.Add(oGeneralData);
                                    }
                                    else
                                    {
                                        oGeneralData.SetProperty("U_CarrierVKN", "7770407106");
                                        oGeneralData.SetProperty("U_CarrierTitle", "Ceva Lojistik Ltd. Şti");

                                        oGeneralService.Add(oGeneralData);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                    maxcode = "1";
                                else
                                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                                oGeneralData.SetProperty("Code", maxcode);
                                oGeneralData.SetProperty("U_TypeCode", "7");
                                oGeneralData.SetProperty("U_Type", "GoodIssue");
                                oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                                oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                                oGeneralData.SetProperty("U_SAPResp", "Taşıyıcı bilgileri hata aldı" + ex.Message);
                                oGeneralData.SetProperty("U_Status", "E");

                                oGeneralService.Add(oGeneralData);
                            }
                        }
                    }
                    else
                    {
                        response = new SAPLayer.Response();
                        response.TransactionNumber = GoodIssue.OrderNumber;
                        response.Successful = true;
                        response.ResultDescription = "Veri Gönderimi Başarılı";

                        #region Özge Hanımlar ile konuşularak tamamı 0 gönderilen satış siparişi açıkta bırakıldı.

                        //string satisSiparisiDocEntry = "";

                        //try
                        //{
                        //    satisSiparisiDocEntry = GoodIssue.OrderNumber.Remove(0, 5).ToString();

                        //    SAPbobsCOM.Documents oOrders = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                        //    bool ret = false;
                        //    int retval = 0;

                        //    ret = oOrders.GetByKey(Convert.ToInt32(satisSiparisiDocEntry));

                        //    if (ret)
                        //    {
                        //        retval = oOrders.Close();

                        //        if (retval == 0)
                        //        {
                        //            response = new SAPLayer.Response();
                        //            response.TransactionNumber = GoodIssue.OrderNumber;
                        //            response.Successful = true;
                        //            response.ResultDescription = "Veri Gönderimi Başarılı";
                        //        }
                        //        else
                        //        {
                        //            response = new SAPLayer.Response();
                        //            response.TransactionNumber = GoodIssue.OrderNumber;
                        //            response.Successful = true;
                        //            response.ResultDescription = "Belge kapatılırken Hata Oluştu. " + oCompany.GetLastErrorDescription();
                        //        }
                        //    }
                        //    else
                        //    {
                        //        response = new SAPLayer.Response();
                        //        response.TransactionNumber = GoodIssue.OrderNumber;
                        //        response.Successful = true;
                        //        response.ResultDescription = "Hata Oluştu. " + satisSiparisiDocEntry + " numaralı belge bulunamadı.";
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    response = new SAPLayer.Response();
                        //    response.TransactionNumber = GoodIssue.OrderNumber;
                        //    response.Successful = false;
                        //    response.ResultDescription = "Hata Oluştu. " + ex.Message;
                        //}

                        #endregion Özge Hanımlar ile konuşularak tamamı 0 gönderilen satış siparişi açıkta bırakıldı.
                    }

                    #endregion Teslimat
                }
                else //if (GoodIssue.Body.goodIssue.MovementCode == "204") //Stok Nakli oluşur.
                {
                    #region Stok Nakli

                    var quantity = GoodIssue.Detail.Select(u => u.Qty).Sum();

                    if (quantity == 0) //Bütün stok nakli talebi 0 toplanmışsa.
                    {
                        var _GoodIssueXML = XmlUtils.SerializeToXml(GoodIssue);

                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "8");
                        oGeneralData.SetProperty("U_Type", "GoodIssueStock");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", _GoodIssueXML);
                        oGeneralData.SetProperty("U_SAPResp", "");
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);

                        response = new SAPLayer.Response();
                        response.TransactionNumber = GoodIssue.OrderNumber;
                        response.Successful = true;
                        response.ResultDescription = "Veri Gönderimi Başarılı";

                        LoginCompany.ReleaseConnection(connection.number);

                        return response;
                    }

                    oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                    oStockTransfer.CardCode = GoodIssue.ToDealerCode;

                    oStockTransfer.DocDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());
                    oStockTransfer.TaxDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());
                    oStockTransfer.DueDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                        GoodIssue.ProcessDate.Value.ToString());

                    //string sapbelgeNo = "";

                    //if (GoodIssue.MovementCode == "107")
                    //{
                    //    sapbelgeNo = GoodIssue.OrderReferenceNumber.ToString().Replace("107-G-", "");
                    //}
                    //else
                    //{
                    //    sapbelgeNo = GoodIssue.OrderNumber;
                    //}

                    foreach (var item in GoodIssue.Detail)
                    {
                        if (Convert.ToDouble(item.Qty) == 0) //Bütün belge 0 toplanmamış ve satırlarda 0 var ise o satırı almıyoruz.
                        {
                            continue;
                        }

                        oStockTransfer.Lines.BaseType = SAPbobsCOM.InvBaseDocTypeEnum.InventoryTransferRequest;

                        oStockTransfer.Lines.BaseEntry = Convert.ToInt32(GoodIssue.OrderNumber.Replace("Y", ""));

                        oStockTransfer.Lines.BaseLine = Convert.ToInt32(item.LineNumber);

                        oStockTransfer.Lines.Quantity = Convert.ToDouble(item.Qty);

                        oStockTransfer.Lines.ItemCode = item.ProductNumber;

                        oStockTransfer.UserFields.Fields.Item("U_DonRef").Value = GoodIssue.OrderNumber.ToString();

                        oStockTransfer.Lines.Add();
                    }

                    if (GoodIssue.WaybillNumber != null)
                    {
                        oStockTransfer.UserFields.Fields.Item("U_IRSALIYE").Value = GoodIssue.WaybillNumber.ToString();
                    }

                    int retval = oStockTransfer.Add();

                    var GoodIssueXML = XmlUtils.SerializeToXml(GoodIssue);
                    if (retval != 0)
                    {
                        oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "8");
                        oGeneralData.SetProperty("U_Type", "GoodIssueStock");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                        oGeneralData.SetProperty("U_SAPResp", oCompany.GetLastErrorCode() + " " + oCompany.GetLastErrorDescription());
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        response = new SAPLayer.Response();
                        response.TransactionNumber = GoodIssue.OrderNumber;
                        response.Successful = false;
                        response.ResultDescription = oCompany.GetLastErrorDescription();

                        //responseList.Add(response);

                        //GetGoodIssueResponse.Body = new CevaReturn.GetGoodIssueResponseBody();
                        //GetGoodIssueResponse.Body.GetGoodIssueResult = new CevaReturn.Response();
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.TransactionNumber = GoodIssue.Body.goodIssue.OrderNumber;
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.Successful = false;
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.ResultDescription = oCompany.GetLastErrorDescription();
                    }
                    else
                    {
                        oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                        oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                        string maxcode = "";
                        oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                        if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                            maxcode = "1";
                        else
                            maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                        oGeneralData.SetProperty("Code", maxcode);
                        oGeneralData.SetProperty("U_TypeCode", "8");
                        oGeneralData.SetProperty("U_Type", "GoodIssueStockTransfer");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                        oGeneralData.SetProperty("U_SAPResp", "Success");
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);

                        int StockTransferNo = Convert.ToInt32(oCompany.GetNewObjectKey());

                        response = new SAPLayer.Response();
                        response.TransactionNumber = GoodIssue.OrderNumber;
                        response.Successful = true;
                        response.ResultDescription = "Veri Gönderimi Başarılı";

                        try
                        {
                            oGeneralService = oCompService.GetGeneralService("DON_CARRIER");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            maxcode = "";
                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@DON_CARRIER\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("DocNum", maxcode);
                            oGeneralData.SetProperty("U_CarrierType", "1");
                            oGeneralData.SetProperty("U_BaseDocType", "67");
                            oGeneralData.SetProperty("U_BaseDocEntry", StockTransferNo);
                            oGeneralData.SetProperty("U_CarrierVKN", "7770407106");
                            oGeneralData.SetProperty("U_CarrierTitle", "Ceva Lojistik Ltd. Şti");

                            oGeneralService.Add(oGeneralData);
                        }
                        catch (Exception ex)
                        {
                            oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("Code", maxcode);
                            oGeneralData.SetProperty("U_TypeCode", "8");
                            oGeneralData.SetProperty("U_Type", "GoodIssueStockTransfer");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                            oGeneralData.SetProperty("U_SAPResp", "Taşıyıcı bilgileri hata aldı" + ex.Message);
                            oGeneralData.SetProperty("U_Status", "E");

                            oGeneralService.Add(oGeneralData);
                        }

                        //responseList.Add(response);

                        //GetGoodIssueResponse.Body = new CevaReturn.GetGoodIssueResponseBody();
                        //GetGoodIssueResponse.Body.GetGoodIssueResult = new CevaReturn.Response();
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.TransactionNumber = GoodIssue.Body.goodIssue.OrderNumber;
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.Successful = true;
                        //GetGoodIssueResponse.Body.GetGoodIssueResult.ResultDescription = "Veri Gönderimi Başarılı";
                    }

                    #endregion Stok Nakli
                }

                #endregion Teslimat ve Stok nakilleri oluşuyor.

                LoginCompany.ReleaseConnection(connection.number);
            }
            catch (Exception)
            {
            }
            finally
            {
                LoginCompany.ReleaseConnection(connectionNumber);
            }

            return response;
        }

        private string addReturnDetails(SAPbobsCOM.Company oCompany, SAPLayer.GoodIssue GoodIssue)
        {
            try
            {
                SAPbobsCOM.GeneralDataCollection oChildren;
                SAPbobsCOM.GeneralData oChild;
                SAPbobsCOM.Items oItem = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);
                SAPbobsCOM.BusinessPartners oBP = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);
                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralService oGeneralService;
                SAPbobsCOM.CompanyService oCompService = null;
                oCompService = oCompany.GetCompanyService();
                oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_RET");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string maxcode = "";
                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_RET\"");
                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                    maxcode = "1";
                else
                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                oBP.GetByKey(GoodIssue.ToDealerCode);

                oGeneralData.SetProperty("Code", maxcode);
                oGeneralData.SetProperty("U_DocDate", ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                    GoodIssue.ProcessDate.Value.ToString()));
                oGeneralData.SetProperty("U_CardCode", oBP.CardCode);
                oGeneralData.SetProperty("U_CardName", oBP.CardName);
                oGeneralData.SetProperty("U_Status", "1");
                oGeneralData.SetProperty("U_Type", GoodIssue.MovementCode.ToString());
                oGeneralData.SetProperty("U_CevaType", GoodIssue.MovementCode.ToString());

                foreach (var item in GoodIssue.Detail)
                {
                    oItem.GetByKey(item.ProductNumber);
                    oChildren = oGeneralData.Child("AIF_ZW_CV_RET1");
                    oChild = oChildren.Add();

                    oChild.SetProperty("U_ItemCode", item.ProductNumber);
                    oChild.SetProperty("U_ItemName", oItem.ItemName);
                    oChild.SetProperty("U_Quantity", Convert.ToInt32(item.Qty));
                    oChild.SetProperty("U_LineId", item.LineNumber.ToString());
                }

                oGeneralService.Add(oGeneralData);
            }
            catch (Exception ex)
            {
                return ex.StackTrace.ToString();
            }

            return "";
        }

        private DateTime ReplaceDecimalToDatetime(string value)
        {
            DateTime dt = new DateTime(Convert.ToInt32(value.ToString().Substring(0, 4)),
                Convert.ToInt32(value.ToString().Substring(4, 2)),
                Convert.ToInt32(value.ToString().Substring(6, 2)));

            return dt;
        }

        private SAPLayer.Response StokNaklineDonustur(CevaWMSQa.Asn GoodIssue, SAPbobsCOM.Company oCompany)
        {
            #region Stok Nakli

            SAPLayer.Response response = new Response();

            SAPbobsCOM.GeneralData oGeneralData;
            SAPbobsCOM.GeneralService oGeneralService;
            SAPbobsCOM.CompanyService oCompService = null;

            SAPbobsCOM.StockTransfer oStockTransfer = null;

            SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            var quantity = GoodIssue.Detail.Select(u => u.Qty).Sum();

            if (quantity == 0) //Bütün stok nakli talebi 0 toplanmışsa.
            {
                var _GoodIssueXML = XmlUtils.SerializeToXml(GoodIssue);

                oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                string maxcode = "";
                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                    maxcode = "1";
                else
                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                oGeneralData.SetProperty("Code", maxcode);
                oGeneralData.SetProperty("U_TypeCode", "8");
                oGeneralData.SetProperty("U_Type", "GoodIssueStock");
                oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                oGeneralData.SetProperty("U_RequestXML", _GoodIssueXML);
                oGeneralData.SetProperty("U_SAPResp", "");
                oGeneralData.SetProperty("U_Status", "S");

                oGeneralService.Add(oGeneralData);

                response = new SAPLayer.Response();
                response.TransactionNumber = GoodIssue.AsnNumber;
                response.Successful = true;
                response.ResultDescription = "Veri Gönderimi Başarılı";
            }

            oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

            oStockTransfer.CardCode = GoodIssue.DealerCode;

            oStockTransfer.DocDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                GoodIssue.DeliveryDate.Value.ToString());
            oStockTransfer.TaxDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                GoodIssue.DeliveryDate.Value.ToString());
            oStockTransfer.DueDate = ReplaceDecimalToDatetime(GoodIssue.WaybillDate != null ? GoodIssue.WaybillDate.Value.ToString() :
                GoodIssue.DeliveryDate.Value.ToString());

            foreach (var item in GoodIssue.Detail)
            {
                if (Convert.ToDouble(item.Qty) == 0) //Bütün belge 0 toplanmamış ve satırlarda 0 var ise o satırı almıyoruz.
                {
                    continue;
                }

                string sapbelgeNo = GoodIssue.AsnNumber.ToString().Replace("207-G-", "");

                oStockTransfer.Lines.BaseType = SAPbobsCOM.InvBaseDocTypeEnum.InventoryTransferRequest;

                oStockTransfer.Lines.BaseEntry = Convert.ToInt32(sapbelgeNo);

                oStockTransfer.Lines.BaseLine = Convert.ToInt32(item.LineNumber);

                oStockTransfer.Lines.Quantity = Convert.ToDouble(item.Qty);

                oStockTransfer.Lines.ItemCode = item.ProductNumber;

                oStockTransfer.UserFields.Fields.Item("U_DonRef").Value = sapbelgeNo.ToString();

                oStockTransfer.Lines.Add();
            }

            if (GoodIssue.WaybillNumber != null)
            {
                oStockTransfer.UserFields.Fields.Item("U_IRSALIYE").Value = GoodIssue.WaybillNumber.ToString();
            }

            int retval = oStockTransfer.Add();

            var GoodIssueXML = XmlUtils.SerializeToXml(GoodIssue);
            if (retval != 0)
            {
                oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                string maxcode = "";
                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                    maxcode = "1";
                else
                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                oGeneralData.SetProperty("Code", maxcode);
                oGeneralData.SetProperty("U_TypeCode", "8");
                oGeneralData.SetProperty("U_Type", "GoodIssueStock");
                oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                oGeneralData.SetProperty("U_SAPResp", oCompany.GetLastErrorCode() + " " + oCompany.GetLastErrorDescription());
                oGeneralData.SetProperty("U_Status", "E");

                oGeneralService.Add(oGeneralData);

                response = new SAPLayer.Response();
                response.TransactionNumber = GoodIssue.AsnNumber;
                response.Successful = false;
                response.ResultDescription = oCompany.GetLastErrorDescription();

                //responseList.Add(response);

                //GetGoodIssueResponse.Body = new CevaReturn.GetGoodIssueResponseBody();
                //GetGoodIssueResponse.Body.GetGoodIssueResult = new CevaReturn.Response();
                //GetGoodIssueResponse.Body.GetGoodIssueResult.TransactionNumber = GoodIssue.Body.goodIssue.OrderNumber;
                //GetGoodIssueResponse.Body.GetGoodIssueResult.Successful = false;
                //GetGoodIssueResponse.Body.GetGoodIssueResult.ResultDescription = oCompany.GetLastErrorDescription();
            }
            else
            {
                oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                string maxcode = "";
                oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                    maxcode = "1";
                else
                    maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                oGeneralData.SetProperty("Code", maxcode);
                oGeneralData.SetProperty("U_TypeCode", "8");
                oGeneralData.SetProperty("U_Type", "GoodIssueStockTransfer");
                oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                oGeneralData.SetProperty("U_SAPResp", "Success");
                oGeneralData.SetProperty("U_Status", "S");

                oGeneralService.Add(oGeneralData);

                int StockTransferNo = Convert.ToInt32(oCompany.GetNewObjectKey());

                response = new SAPLayer.Response();
                response.TransactionNumber = GoodIssue.AsnNumber;
                response.Successful = true;
                response.ResultDescription = "Veri Gönderimi Başarılı";

                try
                {
                    oGeneralService = oCompService.GetGeneralService("DON_CARRIER");
                    oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                    maxcode = "";
                    oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@DON_CARRIER\"");
                    if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                        maxcode = "1";
                    else
                        maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                    oGeneralData.SetProperty("DocNum", maxcode);
                    oGeneralData.SetProperty("U_CarrierType", "1");
                    oGeneralData.SetProperty("U_BaseDocType", "67");
                    oGeneralData.SetProperty("U_BaseDocEntry", StockTransferNo);

                    SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    oRS.DoQuery("Select \"U_CargoFirm\",ISNULL(\"QryGroup61\",'N') as \"QryGroup61\"  from \"OWTR\" as T0 INNER JOIN OCRD as T1 ON T0.\"CardCode\" = T1.\"CardCode\" where T0.\"DocEntry\"='" + StockTransferNo + "' ");

                    if (oRS.Fields.Item("QryGroup61").Value.ToString() == "Y")
                    {
                        if (oRS.Fields.Item(0).Value.ToString() == "UPS")
                        {
                            oGeneralData.SetProperty("U_CarrierVKN", "8400105133");
                            oGeneralData.SetProperty("U_CarrierTitle", "UPS Hızlı Kargo Taşımacılığı A.Ş.");

                            oGeneralService.Add(oGeneralData);
                        }
                        else
                        {
                            oGeneralData.SetProperty("U_CarrierVKN", "9860008925");
                            oGeneralData.SetProperty("U_CarrierTitle", "YURTİÇİ KARGO SERVİSİ A.Ş.");

                            oGeneralService.Add(oGeneralData);
                        }
                    }
                    else
                    {
                        if (oRS.Fields.Item(0).Value.ToString() == "YK")
                        {
                            oGeneralData.SetProperty("U_CarrierVKN", "9860008925");
                            oGeneralData.SetProperty("U_CarrierTitle", "YURTİÇİ KARGO SERVİSİ A.Ş.");

                            oGeneralService.Add(oGeneralData);
                        }
                        else if (oRS.Fields.Item(0).Value.ToString() == "UPS")
                        {
                            oGeneralData.SetProperty("U_CarrierVKN", "8400105133");
                            oGeneralData.SetProperty("U_CarrierTitle", "UPS Hızlı Kargo Taşımacılığı A.Ş.");

                            oGeneralService.Add(oGeneralData);
                        }
                        else
                        {
                            oGeneralData.SetProperty("U_CarrierVKN", "7770407106");
                            oGeneralData.SetProperty("U_CarrierTitle", "Ceva Lojistik Ltd. Şti");

                            oGeneralService.Add(oGeneralData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                    oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                    oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                    if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                        maxcode = "1";
                    else
                        maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                    oGeneralData.SetProperty("Code", maxcode);
                    oGeneralData.SetProperty("U_TypeCode", "8");
                    oGeneralData.SetProperty("U_Type", "GoodIssueStockTransfer");
                    oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                    oGeneralData.SetProperty("U_RequestXML", GoodIssueXML);
                    oGeneralData.SetProperty("U_SAPResp", "Taşıyıcı bilgileri hata aldı." + ex.Message);
                    oGeneralData.SetProperty("U_Status", "E");

                    oGeneralService.Add(oGeneralData);
                }
            }

            return response;

            #endregion Stok Nakli
        }

        private static void setLog(string appId, string type, string Description, string ItemCode, string orderNumber, string ReWork, string RequestXML, SAPbobsCOM.Company oCompany)
        {
            //Farklı yerlerden farklı şekilde çağırılacağı için method parametre alır ve ona göre işlem yapar.
            string Code = "";
            var oCompanyServiceData = oCompany.GetCompanyService(); //Bu satır SAP'den bağlanılmış olan şirketten yani Veritabanından instance alır.
            var oGeneralServiceData = oCompanyServiceData.GetGeneralService("AIF_ENT_LOG"); //Bu satır instance alınmış nesneden Log tablosunun bir adı var o isimle tabloyu çeker.
            var oGeneralData = ((SAPbobsCOM.GeneralData)(oGeneralServiceData.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData))); //Bu satır verilerin propertylerini doldurmak için kullanılır.
            Code = GetCodeGeneration("[@AIF_ENT_LOG]", oCompany).ToString(); //GetCodeGeneration methodu tabloya ait son sıra numarasını verir.

            //Bu satırlar oGeneralServiceData nesnesinin içine çektiğimiz alanlar ile doldurulup kayıt eden satırlardır.
            oGeneralData.SetProperty("Code", Code);
            oGeneralData.SetProperty("U_AppId", appId);
            oGeneralData.SetProperty("U_AppItemCode", ItemCode);
            oGeneralData.SetProperty("U_Status", type);
            oGeneralData.SetProperty("U_Description", Description);
            oGeneralData.SetProperty("U_OrderNumber", orderNumber);
            oGeneralData.SetProperty("U_Rework", ReWork);
            oGeneralData.SetProperty("U_RequestXML", RequestXML);
            //Bu satırlar oGeneralServiceData nesnesinin içine çektiğimiz alanlar ile doldurulup kayıt eden satırlardır.

            oGeneralServiceData.Add(oGeneralData); //Kayıdı gerçekleştiren satırdır.
        }

        private static int GetCodeGeneration(string TableName, SAPbobsCOM.Company oCompany)
        {
            try
            {
                SAPbobsCOM.Recordset rsetCode = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                string strCode = "Select ISNULL(Max(ISNULL(DocEntry,0)),0) + 1 Code From " + TableName + "";
                rsetCode.DoQuery(strCode);
                return Convert.ToInt32(rsetCode.Fields.Item("Code").Value);
            }
            catch (Exception ex)
            {
                //oApplication.StatusBar.SetText("GetCodeGeneration Function Failed:" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                return 0;
            }
            finally
            {
            }
        }

        private string mailGovdesiOlustur(string sipno, string musteriadi, double tutar)
        {
            string raporTablosu = "";

            raporTablosu += "<p>Aşağıdaki sipariş amazon tarafında onayınızı beklemektedir. </p>";
            raporTablosu += "<table class=\"table table-striped\" width=\"770px\" border=\"1\" style=\"border: 1px solid #000; box-shadow: 3px 3px 3px 3px #666; border-radius: 5px; margin: 0px auto; margin-top: 20px;\">" + Environment.NewLine;
            raporTablosu += "<thead>" + Environment.NewLine;
            raporTablosu += "<tr class=\"ust\">" + Environment.NewLine;
            raporTablosu += "<th scope=\"col\">Sipariş Numarası</th>" + Environment.NewLine;
            raporTablosu += "<th scope=\"col\">Müşteri Adı</th>" + Environment.NewLine;
            raporTablosu += "<th scope=\"col\">Tutar</th>" + Environment.NewLine;
            raporTablosu += "</tr>" + Environment.NewLine;
            raporTablosu += "</thead>" + Environment.NewLine;
            raporTablosu += "<tbody> " + Environment.NewLine;
            //row
            raporTablosu += "<tr class=\"alt\">" + Environment.NewLine;
            raporTablosu += "<td scope=\"row\">" + sipno + "</td> " + Environment.NewLine;
            raporTablosu += "<td>" + musteriadi + "</td> " + Environment.NewLine;
            raporTablosu += "<td>" + tutar.ToString("N2") + "</td> " + Environment.NewLine;

            raporTablosu += "</tr>" + Environment.NewLine;
            raporTablosu += "</tbody>" + Environment.NewLine;
            raporTablosu += "</table>" + Environment.NewLine;

            return raporTablosu;
        }
    }
}