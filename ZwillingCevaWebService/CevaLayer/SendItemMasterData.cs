using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.SAPLayer;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService.CevaLayer
{
    public class SendItemMasterData
    {
        public List<ItemMasterDataResponse> SendItemMastarDataByCode(string ItemCode)
        {
            List<ItemMasterDataResponse> results = new List<ItemMasterDataResponse>();
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
                CevaWMSQa.Product productRequest = new CevaWMSQa.Product();
                SAPbobsCOM.Items oItem = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);


                List<CevaWMSQa.ProductBarcode> listBarcode = new List<CevaWMSQa.ProductBarcode>();
                CevaWMSQa.ProductBarcode barcode = new CevaWMSQa.ProductBarcode();
                List<CevaWMSQa.ProductPallet> listpallet = new List<CevaWMSQa.ProductPallet>();
                CevaWMSQa.ProductPallet pallet = new CevaWMSQa.ProductPallet();

                CevaWMSQa.Login login = new CevaWMSQa.Login();

                login = new CevaWMSQa.Login();
                productRequest = new CevaWMSQa.Product();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";


                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSFindBrand = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"ItemCode\" from \"OITM\" where ISNULL(\"U_E_STATUS\",'') <> 'S' and \"ItemCode\" = '" + ItemCode + "'");

                if (oRS.RecordCount > 0)
                {
                    listBarcode = new List<CevaWMSQa.ProductBarcode>();
                    barcode = new CevaWMSQa.ProductBarcode();
                    listpallet = new List<CevaWMSQa.ProductPallet>();
                    pallet = new CevaWMSQa.ProductPallet();

                    oItem.GetByKey(oRS.Fields.Item(0).Value.ToString());

                    productRequest.CompanyCode = "47"; //Sabit göndermemiz istendi.

                    productRequest.WarehouseCode = "01"; //Sabit göndermemiz istendi.

                    productRequest.ProductNumber = oItem.ItemCode;

                    productRequest.ProductDescription = oItem.ItemName;

                    productRequest.GroupCode = oItem.ItemsGroupCode.ToString();

                    productRequest.SmallUnit = oItem.InventoryUOM;

                    productRequest.BigUnit = oItem.InventoryUOM;

                    productRequest.Sim = 1;

                    productRequest.BrutWeight = oItem.SalesWeightUnit;

                    productRequest.NetWeight = oItem.SalesWeightUnit;

                    productRequest.Height = Convert.ToDecimal(oItem.SalesUnitHeight);

                    productRequest.Length = Convert.ToDecimal(oItem.SalesUnitLength);

                    productRequest.Width = Convert.ToDecimal(oItem.SalesUnitWidth);

                    productRequest.Desi = Convert.ToInt32((Convert.ToDouble(oItem.SalesUnitHeight) * Convert.ToDouble(oItem.SalesUnitLength) * Convert.ToDouble(oItem.SalesUnitWidth)) / 3000);

                    productRequest.Lot1Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.Lot2Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.Lot3Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.TypeOfPallet = "EU";

                    productRequest.UnitUsage = CevaWMSQa.UnitUsage.SmallUnit;

                    productRequest.ReceiveDateType = CevaWMSQa.ReceiveDateType.ProductionDate;

                    productRequest.SerialUsage = CevaWMSQa.SerialUsage.NonUsage;

                    productRequest.VariableSim = CevaWMSQa.VariableSim.NonUsage;

                    productRequest.RotationType = CevaWMSQa.RotationType.NonUsage;

                    productRequest.TransactionCode = CevaWMSQa.TransactionCode.Active;

                    oRSFindBrand.DoQuery("Select ISNULL(\"Name\",'') from \"@U_BRAND\" where \"Code\" = '" + oItem.UserFields.Fields.Item("U_BRAND").Value.ToString() + "'");

                    productRequest.ClassificationCode = oRSFindBrand.Fields.Item(0).Value.ToString();

                    pallet.PalletType = "10";

                    pallet.PalletQty = 999;

                    listpallet.Add(pallet);

                    productRequest.Pallet = listpallet.ToArray();

                    for (int i = 0; i < oItem.BarCodes.Count; i++)
                    {
                        oItem.BarCodes.SetCurrentLine(i);

                        barcode.BarcodeNumber = oItem.BarCodes.BarCode;

                        barcode.DefaultBarcode = true;

                        listBarcode.Add(barcode);

                        barcode = new CevaWMSQa.ProductBarcode();
                    }

                    productRequest.Barcode = listBarcode.ToArray();

                    var productRequestXML = XmlUtils.SerializeToXml(productRequest);

                    var response = service.CreateProduct(login, productRequest);

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
                        oGeneralData.SetProperty("U_TypeCode", "2");
                        oGeneralData.SetProperty("U_Type", "ItemMasterData");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", productRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);

                        oItem.UserFields.Fields.Item("U_E_STATUS").Value = "S";
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
                        oGeneralData.SetProperty("U_TypeCode", "2");
                        oGeneralData.SetProperty("U_Type", "ItemMasterData");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", productRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oItem.UserFields.Fields.Item("U_E_STATUS").Value = "E";
                    }

                    oItem.Update();

                    results.Add(new ItemMasterDataResponse { productionCode = oCompany.GetNewObjectKey(), response = response.ResultDescription });

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

        public List<ItemMasterDataResponse> SendAllItemMasterData()
        {
            List<ItemMasterDataResponse> results = new List<ItemMasterDataResponse>();
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
                CevaWMSQa.Product productRequest = new CevaWMSQa.Product();
                SAPbobsCOM.Items oItem = (SAPbobsCOM.Items)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems);


                List<CevaWMSQa.ProductBarcode> listBarcode = new List<CevaWMSQa.ProductBarcode>();
                CevaWMSQa.ProductBarcode barcode = new CevaWMSQa.ProductBarcode();
                List<CevaWMSQa.ProductPallet> listpallet = new List<CevaWMSQa.ProductPallet>();
                CevaWMSQa.ProductPallet pallet = new CevaWMSQa.ProductPallet();

                CevaWMSQa.Login login = new CevaWMSQa.Login();
                 
                login = new CevaWMSQa.Login();
                productRequest = new CevaWMSQa.Product();
                login.Language = new CevaWMSQa.Language();
                login.Language = CevaWMSQa.Language.TR;
                login.Company = "ZWILLING";
                login.UserName = "ZWILLING";
                login.Password = "5t*f2h8k";

                SAPbobsCOM.Recordset oRS = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                SAPbobsCOM.Recordset oRSFindBrand = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                oRS.DoQuery("Select \"ItemCode\" from \"OITM\" where ISNULL(\"U_E_STATUS\",'') <> 'S'");

                while (!oRS.EoF)
                {
                    listBarcode = new List<CevaWMSQa.ProductBarcode>();
                    barcode = new CevaWMSQa.ProductBarcode();
                    listpallet = new List<CevaWMSQa.ProductPallet>();
                    pallet = new CevaWMSQa.ProductPallet();

                    oItem.GetByKey(oRS.Fields.Item(0).Value.ToString());

                    productRequest.CompanyCode = "47"; //Sabit göndermemiz istendi.

                    productRequest.WarehouseCode = "01"; //Sabit göndermemiz istendi.

                    productRequest.ProductNumber = oItem.ItemCode;

                    productRequest.ProductDescription = oItem.ItemName;

                    productRequest.GroupCode = oItem.ItemsGroupCode.ToString();

                    productRequest.SmallUnit = oItem.InventoryUOM;

                    productRequest.BigUnit = oItem.InventoryUOM;

                    productRequest.Sim = 1;

                    productRequest.BrutWeight = oItem.SalesWeightUnit;

                    productRequest.NetWeight = oItem.SalesWeightUnit;

                    productRequest.Height = Convert.ToDecimal(oItem.SalesUnitHeight);

                    productRequest.Length = Convert.ToDecimal(oItem.SalesUnitLength);

                    productRequest.Width = Convert.ToDecimal(oItem.SalesUnitWidth);

                    productRequest.Desi = Convert.ToInt32((Convert.ToDouble(oItem.SalesUnitHeight) * Convert.ToDouble(oItem.SalesUnitLength) * Convert.ToDouble(oItem.SalesUnitWidth)) / 3000);

                    productRequest.Lot1Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.Lot2Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.Lot3Usage = CevaWMSQa.LotUsage.NonUsage;

                    productRequest.TypeOfPallet = "EU";

                    productRequest.UnitUsage = CevaWMSQa.UnitUsage.SmallUnit;

                    productRequest.ReceiveDateType = CevaWMSQa.ReceiveDateType.ProductionDate;

                    productRequest.SerialUsage = CevaWMSQa.SerialUsage.NonUsage;

                    productRequest.VariableSim = CevaWMSQa.VariableSim.NonUsage;

                    productRequest.RotationType = CevaWMSQa.RotationType.NonUsage;

                    productRequest.TransactionCode = CevaWMSQa.TransactionCode.Active;

                    oRSFindBrand.DoQuery("Select ISNULL(\"Name\",'') from \"@U_BRAND\" where \"Code\" = '" + oItem.UserFields.Fields.Item("U_BRAND").Value.ToString() + "'");

                    productRequest.ClassificationCode = oRSFindBrand.Fields.Item(0).Value.ToString();

                    pallet.PalletType = "10";

                    pallet.PalletQty = 999;

                    listpallet.Add(pallet);

                    productRequest.Pallet = listpallet.ToArray();

                    for (int i = 0; i < oItem.BarCodes.Count; i++)
                    {
                        oItem.BarCodes.SetCurrentLine(i);

                        barcode.BarcodeNumber = oItem.BarCodes.BarCode;

                        barcode.DefaultBarcode = true;

                        listBarcode.Add(barcode);

                        barcode = new CevaWMSQa.ProductBarcode();
                    }

                    productRequest.Barcode = listBarcode.ToArray();

                    var productRequestXML = XmlUtils.SerializeToXml(productRequest);

                    var response = service.CreateProduct(login, productRequest);

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
                        oGeneralData.SetProperty("U_TypeCode", "2");
                        oGeneralData.SetProperty("U_Type", "ItemMasterData");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", productRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "S");

                        oGeneralService.Add(oGeneralData);

                        oItem.UserFields.Fields.Item("U_E_STATUS").Value = "S";
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
                        oGeneralData.SetProperty("U_TypeCode", "2");
                        oGeneralData.SetProperty("U_Type", "ItemMasterData");
                        oGeneralData.SetProperty("U_TransactionDate", DateTime.Now.ToString());
                        oGeneralData.SetProperty("U_RequestXML", productRequestXML);
                        oGeneralData.SetProperty("U_CevaResp", response.ResultDescription);
                        oGeneralData.SetProperty("U_Status", "E");

                        oGeneralService.Add(oGeneralData);

                        oItem.UserFields.Fields.Item("U_E_STATUS").Value = "E";
                    }

                    results.Add(new ItemMasterDataResponse { productionCode = oCompany.GetNewObjectKey(), response = response.ResultDescription });

                    oItem.Update();

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