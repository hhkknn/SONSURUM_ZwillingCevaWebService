using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.SAPLayer
{
    public class CreateGoodReciept
    {
        //
        public List<SAPLayer.Response> CreateGoodRecieptAll(List<SAPLayer.GoodReceipt> GoodReceipt)
        {
            #region Değişkenler ve nesneler
            SAPbobsCOM.GeneralData oGeneralData;
            SAPbobsCOM.GeneralService oGeneralService;
            SAPbobsCOM.CompanyService oCompService = null;
            ConnectionList connection = new ConnectionList();
            ZwillingCevaWebService.SAPLayer.LoginCompany log = new SAPLayer.LoginCompany();
            SAPLayer.Response response = new Response();
            List<SAPLayer.Response> responseList = new List<SAPLayer.Response>();
            int connectionNumber = 0;
            #endregion
            try
            {

                #region Şirket bağlantısı ve nesneler
                connection = log.setLogin();
                connectionNumber = connection.number;
                SAPbobsCOM.Company oCompany = connection.oCompany;
                SAPbobsCOM.StockTransfer oStockTransfer = null;
                SAPbobsCOM.Documents oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);

                oCompService = oCompany.GetCompanyService();

                string AsnNumber = "";

                SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                #endregion

                var list = new List<GoodReceipt>();
                var groups = GoodReceipt.GroupBy(x => new { x.AsnNumber });

                //Iade işlemine ait satırların hepsi yapıldıktan sonra bu listeden çıkarılıyor ve liste aşağıda yeniden gruplanıyor.
                #region Iade verileri tabloya atılıyor.
                foreach (var item in groups)
                {
                    var AsnNumber1 = item.Key.AsnNumber;

                    list = GoodReceipt.Where(y => y.AsnNumber == AsnNumber1).ToList();

                    var distincItems = GoodReceipt.GroupBy(x => new { x.AsnNumber, x.LineNumber }).Select(y => y.First());

                    List<GoodReceipt> _GoodReceipt = new List<GoodReceipt>();
                    foreach (var item2 in distincItems.Where(x => (x.MovementCode == "103" || x.MovementCode == "104" || x.MovementCode == "105") && x.AsnNumber == AsnNumber1)) //Iadeler listeye dolduruluyor.
                    {
                        _GoodReceipt.Add(item2);
                    }

                    string _retval = "";
                    if (_GoodReceipt.Count > 0)
                    {

                        _retval = addReturnDetails(oCompany, _GoodReceipt); //Iade listesi tabloya atılıyor.

                        if (string.IsNullOrEmpty(_retval)) //Kaydetme durumuna göre dönüş yapılıyor.
                        {
                            response = new SAPLayer.Response();
                            response.TransactionNumber = _GoodReceipt[0].AsnNumber;
                            response.Successful = true;
                            response.ResultDescription = "Veri Gönderimi Başarılı";

                            responseList.Add(response);

                            var GoodReceiptXML = XmlUtils.SerializeToXml(_GoodReceipt);
                            oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            string maxcode = "";
                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("Code", maxcode);
                            oGeneralData.SetProperty("U_TypeCode", "9");
                            oGeneralData.SetProperty("U_Type", "Return");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodReceiptXML);
                            oGeneralData.SetProperty("U_Status", "S");


                            oGeneralService.Add(oGeneralData);



                        }
                        else
                        {

                            response = new SAPLayer.Response();
                            response.TransactionNumber = _GoodReceipt[0].AsnNumber;
                            response.Successful = false;
                            response.ResultDescription = "Iade detayları aktarılamadı." + _retval;

                            responseList.Add(response);


                            var GoodReceiptXML = XmlUtils.SerializeToXml(_GoodReceipt);
                            oGeneralService = oCompService.GetGeneralService("AIF_ZW_CV_LOG");
                            oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);

                            string maxcode = "";
                            oRSMaxCodeForLog.DoQuery("Select MAX(\"DocEntry\") + 1 from \"@AIF_ZW_CV_LOG\"");
                            if (oRSMaxCodeForLog.Fields.Item(0).Value.ToString() == "0")
                                maxcode = "1";
                            else
                                maxcode = oRSMaxCodeForLog.Fields.Item(0).Value.ToString();

                            oGeneralData.SetProperty("Code", maxcode);
                            oGeneralData.SetProperty("U_TypeCode", "9");
                            oGeneralData.SetProperty("U_Type", "Return");
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodReceiptXML);
                            oGeneralData.SetProperty("U_SAPResp", "Iade detayları aktarılamadı." + _retval.ToString());
                            oGeneralData.SetProperty("U_Status", "E");


                            oGeneralService.Add(oGeneralData);
                        }

                        GoodReceipt.RemoveAll(x => x.AsnNumber == AsnNumber1);
                    }

                    _GoodReceipt = new List<GoodReceipt>();

                }
                #endregion


                groups = GoodReceipt.GroupBy(x => new { x.AsnNumber });

                bool isLineExist = false;

                oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                string stokNakli = "";
                #region Satınalma oluşturma
                foreach (var item in groups)
                {
                    stokNakli = "";

                    isLineExist = false;
                    var AsnNumber1 = item.Key.AsnNumber;

                    list = GoodReceipt.Where(y => y.AsnNumber == AsnNumber1).ToList();

                    var distincItems = GoodReceipt.Where(y => y.AsnNumber == AsnNumber1).GroupBy(x => new { x.AsnNumber, x.LineNumber }).Select(y => y.First());

                    foreach (var item2 in distincItems.Where(x => x.MovementCode != "103" && x.MovementCode != "104" && x.MovementCode != "105"))
                    {

                        AsnNumber = item2.AsnNumber;
                        if (Convert.ToDouble(list.Where(y => y.LineNumber == item2.LineNumber && y.AsnNumber == AsnNumber1).Sum(z => z.Qty)) == 0)
                        {
                            continue;
                        }

                        if (item2.MovementCode == "107")
                        {
                            stokNakli = "Evet";


                            oStockTransfer.CardCode = item2.DealerCode;

                            oStockTransfer.DocDate = ReplaceDecimalToDatetime(item2.DocumentDate.ToString());
                            oStockTransfer.TaxDate = ReplaceDecimalToDatetime(item2.DocumentDate.ToString());
                            oStockTransfer.DueDate = ReplaceDecimalToDatetime(item2.DocumentDate.ToString());


                            string sapbelgeNo = item2.AsnNumber.ToString().Replace("107-G-", "");

                            oStockTransfer.Lines.BaseType = SAPbobsCOM.InvBaseDocTypeEnum.InventoryTransferRequest;

                            oStockTransfer.Lines.BaseEntry = Convert.ToInt32(sapbelgeNo);

                            oStockTransfer.Lines.BaseLine = Convert.ToInt32(item2.LineNumber);

                            oStockTransfer.Lines.Quantity = Convert.ToDouble(item2.Qty);

                            //oStockTransfer.Lines.ItemCode = item2.ProductNumber;

                            oStockTransfer.UserFields.Fields.Item("U_DonRef").Value = sapbelgeNo.ToString();

                            oStockTransfer.Lines.Add();

                            isLineExist = true;
                        }
                        else
                        {
                            oDocuments.CardCode = item2.DealerCode;

                            oDocuments.DocDate = ReplaceDecimalToDatetime(item2.DocumentDate.Value.ToString());

                            oDocuments.DocDueDate = ReplaceDecimalToDatetime(item2.ProductionDate.Value.ToString());

                            oDocuments.TaxDate = ReplaceDecimalToDatetime(item2.ProductionDate.Value.ToString());

                            oDocuments.BPL_IDAssignedToInvoice = 1;

                            oDocuments.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oPurchaseOrders;

                            oDocuments.Lines.BaseEntry = Convert.ToInt32(AsnNumber.Replace("Y", ""));

                            oDocuments.Lines.BaseLine = Convert.ToInt32(item2.LineNumber);

                            oDocuments.Lines.Quantity = Convert.ToDouble(list.Where(y => y.LineNumber == item2.LineNumber && y.AsnNumber == AsnNumber).Sum(z => z.Qty));

                            oDocuments.Lines.ItemCode = item2.ProductNumber;

                            oDocuments.Lines.Add();

                            isLineExist = true;
                        }


                        //foreach (var item2 in distincItems)
                        //{


                    }



                    if (isLineExist) //Eklenecek 0 dan büyük satır varsa SAP'ye belge eklenir.
                    {
                        int retval = -1;

                        if (stokNakli == "") //Yani Satınalma siaparişi oluşturulacaksa
                        {
                            //retval = oDocuments.Add(); //Satınalma siparişi oluşturulmasın istendiği için bu kod kapatıldı.
                            retval = 0;
                        }
                        else
                        {
                            retval = oStockTransfer.Add();
                        }

                        var GoodReceiptXML = XmlUtils.SerializeToXml(distincItems.Where(x => x.MovementCode != "103" && x.MovementCode != "104" && x.MovementCode != "105").ToList());
                        if (retval != 0)
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
                            if (stokNakli == "")
                            {
                                oGeneralData.SetProperty("U_TypeCode", "6");
                                oGeneralData.SetProperty("U_Type", "GoodReciept");
                            }
                            else
                            {
                                oGeneralData.SetProperty("U_TypeCode", "15");
                                oGeneralData.SetProperty("U_Type", "StokTransfer");
                            }
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodReceiptXML);
                            oGeneralData.SetProperty("U_SAPResp", oCompany.GetLastErrorCode() + " " + oCompany.GetLastErrorDescription());
                            oGeneralData.SetProperty("U_Status", "E");


                            oGeneralService.Add(oGeneralData);


                            response = new SAPLayer.Response();
                            response.TransactionNumber = AsnNumber;
                            response.Successful = false;
                            response.ResultDescription = oCompany.GetLastErrorDescription();



                            responseList.Add(response);

                            oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
                            oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
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
                            if (stokNakli == "")
                            {
                                oGeneralData.SetProperty("U_TypeCode", "6");
                                oGeneralData.SetProperty("U_Type", "GoodReciept");
                            }
                            else
                            {
                                oGeneralData.SetProperty("U_TypeCode", "15");
                                oGeneralData.SetProperty("U_Type", "StokTransfer");
                            }
                            oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                            oGeneralData.SetProperty("U_RequestXML", GoodReceiptXML);
                            oGeneralData.SetProperty("U_SAPResp", "Success");
                            oGeneralData.SetProperty("U_Status", "S");


                            oGeneralService.Add(oGeneralData);

                            response = new SAPLayer.Response();
                            response.TransactionNumber = AsnNumber;
                            response.Successful = true;
                            response.ResultDescription = "Veri Gönderimi Başarılı";


                            responseList.Add(response);

                            oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
                            oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                        }
                    }
                    else
                    {
                        if (stokNakli == "") //Yani Satınalma siaparişi oluşturulacaksa
                        {
                            //retval = oDocuments.Add(); //Satınalma siparişi oluşturulmasın istendiği için bu kod kapatıldı.
                        }
                        else
                        {

                            string stoknaklitalebiNo = "";

                            try
                            {
                                stoknaklitalebiNo = AsnNumber;

                                oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                                bool ret = false;
                                int retval = 0;

                                ret = oStockTransfer.GetByKey(Convert.ToInt32(stoknaklitalebiNo));



                                if (ret)
                                {
                                    retval = oDocuments.Close();

                                    if (retval == 0)
                                    {
                                        response = new SAPLayer.Response();
                                        response.TransactionNumber = AsnNumber;
                                        response.Successful = true;
                                        response.ResultDescription = "Veri Gönderimi Başarılı";
                                    }
                                    else
                                    {
                                        response = new SAPLayer.Response();
                                        response.TransactionNumber = AsnNumber;
                                        response.Successful = false;
                                        response.ResultDescription = "Belge kapatılırken Hata Oluştu. " + oCompany.GetLastErrorDescription();
                                    }
                                }
                                else
                                {
                                    response = new SAPLayer.Response();
                                    response.TransactionNumber = AsnNumber;
                                    response.Successful = false;
                                    response.ResultDescription = "Hata Oluştu. " + AsnNumber + " numaralı belge bulunamadı.";
                                }
                            }
                            catch (Exception ex)
                            {
                                response = new SAPLayer.Response();
                                response.TransactionNumber = AsnNumber;
                                response.Successful = false;
                                response.ResultDescription = "Hata Oluştu. " + ex.Message;
                            }

                            responseList.Add(response);

                            oDocuments = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);
                            oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
                        }
                    }

                }
                LoginCompany.ReleaseConnection(connection.number);

                return responseList;
                #endregion
            }
            catch (Exception ex)
            {
                response = new Response();
                response.TransactionNumber = "";
                response.Successful = false;
                response.ResultDescription = ex.ToString();

                responseList.Add(response);

                LoginCompany.ReleaseConnection(connection.number);

                return responseList;
            }
            finally
            {
                LoginCompany.ReleaseConnection(connectionNumber);
            }
        }

        private DateTime ReplaceDecimalToDatetime(string value)
        {
            DateTime dt = new DateTime(Convert.ToInt32(value.ToString().Substring(0, 4)),
                Convert.ToInt32(value.ToString().Substring(4, 2)),
                Convert.ToInt32(value.ToString().Substring(6, 2)));

            return dt;

        }

        private string addReturnDetails(SAPbobsCOM.Company oCompany, List<GoodReceipt> GoodReceipt)
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


                oBP.GetByKey(GoodReceipt[0].DealerCode);


                oGeneralData.SetProperty("Code", maxcode);
                oGeneralData.SetProperty("U_DocDate", ReplaceDecimalToDatetime(GoodReceipt[0].DocumentDate.Value.ToString()));
                oGeneralData.SetProperty("U_CardCode", oBP.CardCode);
                oGeneralData.SetProperty("U_CardName", oBP.CardName);
                oGeneralData.SetProperty("U_RetCardCode", oBP.CardCode);
                oGeneralData.SetProperty("U_RetCardName", oBP.CardName);
                oGeneralData.SetProperty("U_Status", "1");
                oGeneralData.SetProperty("U_Type", GoodReceipt[0].MovementCode);
                oGeneralData.SetProperty("U_CevaType", GoodReceipt[0].MovementCode);


                foreach (var item in GoodReceipt)
                {
                    oItem.GetByKey(item.ProductNumber);
                    oChildren = oGeneralData.Child("AIF_ZW_CV_RET1");
                    oChild = oChildren.Add();

                    oChild.SetProperty("U_ItemCode", item.ProductNumber);
                    oChild.SetProperty("U_ItemName", oItem.ItemName);
                    oChild.SetProperty("U_Quantity", Convert.ToInt32(item.Qty));
                    oChild.SetProperty("U_LineId", item.LineNumber.ToString());
                }

                var GoodReceiptXML = XmlUtils.SerializeToXml(GoodReceipt);
                oGeneralData.SetProperty("U_XML", GoodReceiptXML);
                oGeneralService.Add(oGeneralData);

                string ret = AddReturnToSAPDocument(oCompany, GoodReceipt);


                return ret;


            }
            catch (Exception ex)
            {
                return ex.StackTrace.ToString();
            }

            return "";
        }

        private string AddReturnToSAPDocument(SAPbobsCOM.Company oCompany, List<GoodReceipt> GoodReceipt)
        {
            SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            SAPbobsCOM.Documents oReturn = (SAPbobsCOM.Documents)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDrafts);

            oReturn.DocObjectCode = SAPbobsCOM.BoObjectTypes.oReturns;

            oReturn.CardCode = GoodReceipt[0].DealerCode;
            oReturn.DocDate = ReplaceDecimalToDatetime(GoodReceipt[0].DocumentDate.Value.ToString());
            oReturn.DocDueDate = ReplaceDecimalToDatetime(GoodReceipt[0].ProductionDate.Value.ToString());
            oReturn.TaxDate = ReplaceDecimalToDatetime(GoodReceipt[0].ProductionDate.Value.ToString());
            oReturn.NumAtCard = GoodReceipt[0].DocumentNumber;
            string WhsCode = "";
            string sql = "Select WhsCode from \"OWHS\" where \"U_CevaRetType\" = '" + GoodReceipt[0].MovementCode + "'";
            oRS.DoQuery(sql);

            oReturn.BPL_IDAssignedToInvoice = 1;
            WhsCode = oRS.Fields.Item(0).Value.ToString();
            foreach (var item in GoodReceipt)
            {
                if (item.Qty == 0)
                    continue;

                oReturn.Lines.ItemCode = item.ProductNumber;
                oReturn.Lines.Quantity = Convert.ToDouble(item.Qty);
                oReturn.Lines.WarehouseCode = WhsCode;


                try
                {
                    oRS.DoQuery("Select \"VatGourpSa\" from \"OITM\" where \"ItemCode\" = '" + item.ProductNumber + "'");

                    var satiskdvturu = oRS.Fields.Item("VatGourpSa").Value.ToString();

                    if (satiskdvturu != "")
                    {
                        oRS.DoQuery("Select \"VatCrctn\" from \"OVTG\" where \"Code\" = '" + satiskdvturu + "'");

                        var duzeltmekdvkodu = oRS.Fields.Item("VatCrctn").Value.ToString();

                        if (duzeltmekdvkodu != "")
                        {
                            oReturn.Lines.VatGroup = duzeltmekdvkodu;
                        }
                    }
                }
                catch (Exception)
                {
                }


                oRS.DoQuery("Select \"Price\" from \"ITM1\" where \"ItemCode\" = '" + item.ProductNumber + "' and \"PriceList\" = '16'");

                if (Convert.ToDouble(oRS.Fields.Item(0).Value) > 0)
                {
                    //İadelerde otomatik masraf ayarını etkinleştir işretli gelmesi için  kullanılır. Cost otomatik gelmesi için bakılması gerkeiyor. 
                    oReturn.Lines.EnableReturnCost = SAPbobsCOM.BoYesNoEnum.tYES;
                    oReturn.Lines.ReturnCost = Convert.ToDouble(oRS.Fields.Item(0).Value, System.Globalization.CultureInfo.InvariantCulture);
                }

                oReturn.Lines.Add();
            }

            int retval = oReturn.Add();

            var GoodReceiptXML = XmlUtils.SerializeToXml(GoodReceipt);
            if (retval != 0)
            {
                return oCompany.GetLastErrorDescription();
            }

            return "";
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
    }
}