using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.SAPLayer
{
    public class AddStockTransfer
    {
        public List<SAPLayer.Response> AddStockTransferToSAP(List<StockTransfer> _StockTransfer)
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

            oCompService = oCompany.GetCompanyService();
            SAPbobsCOM.Recordset oRSMaxCodeForLog = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            SAPbobsCOM.StockTransfer oStockTransfer = (SAPbobsCOM.StockTransfer)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);
            SAPbobsCOM.Items oItems = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);
            #endregion

            SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            oRS.DoQuery("Select \"U_TransactionType\",\"U_FromWhs\",\"U_ToWhs\" from \"@AIF_ZW_CV_WHSMATCH\"");

            string WSMatchXML = oRS.GetAsXML();
            List<SAPLayer.Response> resp = new List<SAPLayer.Response>();
            foreach (var item in _StockTransfer)
            {
                if (string.IsNullOrEmpty(item.TransactionType))
                {
                    resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "-10 TransactionType alanı boş geçilemez." });
                    continue;
                }

                DateTime _stocktransferDate = DateTime.Now;
                if (!string.IsNullOrEmpty(item.StockTransferDate))
                {
                    _stocktransferDate = ReplaceDecimalToDatetime(item.StockTransferDate);
                    _stocktransferDate = ReplaceDecimalToDatetime(item.StockTransferDate);
                    _stocktransferDate = ReplaceDecimalToDatetime(item.StockTransferDate);
                }

                var stocktransfertableList = (from x in XDocument.Parse(WSMatchXML).Descendants("row")
                                              select new
                                              {
                                                  CevaIslemTipi = x.Element("U_TransactionType").Value,
                                                  FromWarehouse = x.Element("U_FromWhs").Value,
                                                  ToWarehouse = x.Element("U_ToWhs").Value
                                              }).ToList();


                oStockTransfer.DocDate = _stocktransferDate;
                oStockTransfer.DueDate = _stocktransferDate;
                oStockTransfer.TaxDate = _stocktransferDate;

                //oStockTransfer.UserFields.Fields.Item("").Value = item.CevaTransactionNumber;



                oStockTransfer.FromWarehouse = stocktransfertableList.Where(x => x.CevaIslemTipi == item.TransactionType).Select(y => y.FromWarehouse).FirstOrDefault();

                if (string.IsNullOrEmpty(oStockTransfer.FromWarehouse))
                {
                    resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "-20 " + item.CevaTransactionNumber.ToString() + " numaralı transaction için SAP'de tanımlı kaynak depo bulunmamamıştır." });
                    continue;
                }
                oStockTransfer.ToWarehouse = stocktransfertableList.Where(x => x.CevaIslemTipi == item.TransactionType).Select(y => y.ToWarehouse).FirstOrDefault();


                if (string.IsNullOrEmpty(oStockTransfer.ToWarehouse))
                {
                    resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "-30 " + item.CevaTransactionNumber.ToString() + " numaralı transaction için SAP'de tanımlı hedef depo bulunmamamıştır." });
                    continue;
                }

                foreach (var itemx in item.Detail)
                {

                    if (!oItems.GetByKey(itemx.ProductNumber))
                    {
                        resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "-40 " + itemx.ProductNumber.ToString() + " kodlu ürün SAP'de bulunamamıştır." });
                        break;
                    }

                    oStockTransfer.Lines.ItemCode = itemx.ProductNumber;

                    if (itemx.Quantity == 0)
                    {
                        resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "-50" + itemx.ProductNumber.ToString() + " kodlu ürün için miktar 0'dan büyük olmalıdır." });
                        break;
                    }

                    oStockTransfer.Lines.Quantity = itemx.Quantity;

                    oStockTransfer.Lines.Add();

                }

                int retval = oStockTransfer.Add();

                var StockTransferXML = XmlUtils.SerializeToXml(item);


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
                oGeneralData.SetProperty("U_TypeCode", "10");
                oGeneralData.SetProperty("U_Type", "StockTansferFromWHS");
                oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                oGeneralData.SetProperty("U_RequestXML", StockTransferXML);

                if (retval != 0)
                {
                    oGeneralData.SetProperty("U_SAPResp", oCompany.GetLastErrorCode() + " " + oCompany.GetLastErrorDescription());
                    oGeneralData.SetProperty("U_Status", "E");

                    resp.Add(new SAPLayer.Response { Successful = false, TransactionNumber = item.CevaTransactionNumber, ResultDescription = retval + " Stok Nakli belgesi oluşturulamadı." + oCompany.GetLastErrorDescription() });

                }
                else
                {
                    oGeneralData.SetProperty("U_SAPResp", "Success");
                    oGeneralData.SetProperty("U_Status", "S");

                    resp.Add(new SAPLayer.Response { Successful = true, TransactionNumber = item.CevaTransactionNumber, ResultDescription = "Stok Nakli belgesi başarıyla oluşturuldu." + oCompany.GetLastErrorDescription(), SAPNumber = oCompany.GetNewObjectKey() });
                }
                oGeneralService.Add(oGeneralData);
            }

            return resp;
        }

        private DateTime ReplaceDecimalToDatetime(string value)
        {
            DateTime dt = new DateTime(Convert.ToInt32(value.ToString().Substring(0, 4)),
                Convert.ToInt32(value.ToString().Substring(4, 2)),
                Convert.ToInt32(value.ToString().Substring(6, 2)));

            return dt;

        }
    }
}