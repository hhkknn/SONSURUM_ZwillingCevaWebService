using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using ZwillingCevaWebService.Models;
using ZwillingCevaWebService.Utils;

namespace ZwillingCevaWebService
{
    /// <summary>
    /// Summary description for ZwillingCeva
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
  
    public class ZwillingCeva : System.Web.Services.WebService
    {

        //[WebMethod]
        //public string HelloWorld()
        //{
        //    return "Hello World";
        //}


        [WebMethod]
        public List<BusinessPartnerResponse> SendBusinessPartner(string dealerCode)
        {
            List<BusinessPartnerResponse> result = new List<BusinessPartnerResponse>();
            CevaLayer.SendBusinessPartner access = new CevaLayer.SendBusinessPartner();

            if (string.IsNullOrEmpty(dealerCode))
                result = access.SendAllBussinessPartner();
            else
                result = access.SendBusinessPartnerByCode(dealerCode);

            return result;
        }


        [WebMethod]
        public List<ItemMasterDataResponse> SendItemMasterData(string ItemCode)
        {
            List<ItemMasterDataResponse> result = new List<ItemMasterDataResponse>();
            CevaLayer.SendItemMasterData access = new CevaLayer.SendItemMasterData();

            if (string.IsNullOrEmpty(ItemCode))
                result = access.SendAllItemMasterData();
            else
                result = access.SendItemMastarDataByCode(ItemCode);

            return result;
        }


        [WebMethod]
        public List<SalesOrderResponse> SendSalesOrder(string DocumentNumber)
        {
            List<SalesOrderResponse> result = new List<SalesOrderResponse>();
            CevaLayer.SendSalesOrder access = new CevaLayer.SendSalesOrder();

            int docNo = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                docNo = Convert.ToInt32(DocumentNumber);
            }

            if (docNo != 0)
                result = access.sendSalesOrderByCode(docNo);
            else
                result = access.sendSalesOrder();

            return result;
        }

        [WebMethod]
        public List<SalesOrderResponse> SendStokNakliTalebiCikis(string DocumentNumber)
        {
            List<SalesOrderResponse> result = new List<SalesOrderResponse>();
            CevaLayer.SendInventoryTransferRequest access = new CevaLayer.SendInventoryTransferRequest();

            int docNo = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                docNo = Convert.ToInt32(DocumentNumber);
            }

            if (docNo != 0)
                result = access.sendStokNakliTalebiCikis(docNo);

            return result;
        }
        [WebMethod]
        public List<SalesOrderResponse> SendStokNakliTalebiGiris(string DocumentNumber)
        {
            List<SalesOrderResponse> result = new List<SalesOrderResponse>();
            CevaLayer.SendInventoryTransferRequest access = new CevaLayer.SendInventoryTransferRequest();

            int docNo = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                docNo = Convert.ToInt32(DocumentNumber);
            }

            if (docNo != 0)
                result = access.SendStokNakliTalebiGiris(docNo);

            return result;
        }


        [WebMethod]
        public List<InventoryTransferRequestResponse> SendInventoryTransferRequest(string DocumentNumber)
        {
            List<InventoryTransferRequestResponse> result = new List<InventoryTransferRequestResponse>();
            CevaLayer.SendInventoryTransferRequest access = new CevaLayer.SendInventoryTransferRequest();

            int docNo = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                docNo = Convert.ToInt32(DocumentNumber);
            }

            if (docNo != 0)
                result = access.SendInventoryTransferRequestByCode(docNo);
            else
                result = access.SendAllInventoryTransferRequest();

            return result;
        }

        [WebMethod]
        public List<PurchaseOrderResponse> SendPurchaseOrder(string DocumentNumber)
        {
            List<PurchaseOrderResponse> result = new List<PurchaseOrderResponse>();
            CevaLayer.SendPurchaseOrder access = new CevaLayer.SendPurchaseOrder();

            int docNo = 0;
            if (!string.IsNullOrEmpty(DocumentNumber))
            {
                docNo = Convert.ToInt32(DocumentNumber);
            }

            if (docNo != 0)
                result = access.SendPurchaseOrderByCode(docNo);
            else
                result = access.SendAllPurchaseOrder();

            return result;
        }

        //[WebMethod]
        //public List<CevaReturn.GetGoodReceiptResponse> GetGoodReceipt(List<CevaReturn.GetGoodReceiptRequest> GetGoodReceipt)
        //{
        //    //Gelen verileri burada SAP'ye oluşturup cevapları dön.
        //    SAPLayer.CreateGoodReciept access = new SAPLayer.CreateGoodReciept();

        //    List<CevaReturn.GetGoodReceiptResponse> GetGoodReceiptResponseList = new List<CevaReturn.GetGoodReceiptResponse>();
        //    foreach (var item in GetGoodReceipt)
        //    {
        //        GetGoodReceiptResponseList = access.CreateGoodRecieptAll(item);
        //    }

        //    //var yyy = XmlUtils.SerializeToXml(GetGoodReceiptResponseList);
        //    return GetGoodReceiptResponseList;
        //}

        [WebMethod]
        public List<SAPLayer.Response> GetGoodReceipt(SAPLayer.Login login, List<SAPLayer.GoodReceipt> goodReceipt)
        {
            //Gelen verileri burada SAP'ye oluşturup cevapları dön.
            SAPLayer.CreateGoodReciept access = new SAPLayer.CreateGoodReciept();

            List<SAPLayer.Response> Response = new List<SAPLayer.Response>();

            Response = access.CreateGoodRecieptAll(goodReceipt);

            return Response;
        }

        [WebMethod]
        public SAPLayer.Response GetGoodIssue(SAPLayer.Login login, SAPLayer.GoodIssue goodIssue)
        {
            //Gelen verileri burada SAP'ye oluşturup cevapları dön.
            SAPLayer.CreateGoodIssue access = new SAPLayer.CreateGoodIssue();

            SAPLayer.Response Response = new SAPLayer.Response();
            //CevaReturn.GetGoodIssueResponse GetGoodIssueResponseList = new CevaReturn.GetGoodIssueResponse();

            Response = access.CreateGoodIssueAll(goodIssue);
            //GetGoodIssueResponseList = access.CreateGoodIssueAll(GetGoodIssue);

            //var yyy = XmlUtils.SerializeToXml(GetGoodIssueResponseList);
            return Response;
        }

        //[WebMethod]
        //public CevaReturn.GetGoodIssueResponse GetGoodIssue(CevaReturn.GetGoodIssueRequest GetGoodIssue)
        //{
        //    //Gelen verileri burada SAP'ye oluşturup cevapları dön.
        //    SAPLayer.CreateGoodIssue access = new SAPLayer.CreateGoodIssue();

        //    CevaReturn.GetGoodIssueResponse GetGoodIssueResponseList = new CevaReturn.GetGoodIssueResponse();

        //    GetGoodIssueResponseList = access.CreateGoodIssueAll(GetGoodIssue);

        //    //var yyy = XmlUtils.SerializeToXml(GetGoodIssueResponseList);
        //    return GetGoodIssueResponseList;
        //}

        //[WebMethod]
        //public List<CevaReturn.GetStockMovementResponse> GetStockMovement(List<CevaReturn.GetStockMovementRequest> GetStockMovement)
        //{
        //    //Gelen verileri burada SAP'ye oluşturup cevapları dön.
        //    SAPLayer.CreateGoodIssue access = new SAPLayer.CreateGoodIssue();

        //    foreach (var item in GetStockMovement)
        //    {

        //        //access.CreateGoodIssueAll(item);
        //    }

        //    //CevaReturn.GetGoodIssueResponse result = new CevaReturn.GetGoodIssueResponse();
        //    List<CevaReturn.GetStockMovementResponse> resultList = new List<CevaReturn.GetStockMovementResponse>();
        //    //List<CevaReturn.Response> response = new List<CevaReturn.Response>();

        //    //result.Body.GetGoodIssueResult = response.ToArray();

        //    //resultList.Add(result);

        //    return resultList;
        //}

        //[WebMethod]
        //public List<CevaReturn.GetStockReportResponse> GetStockReport(List<CevaReturn.GetStockReportResponse> GetStockMovement)
        //{
        //    //Gelen verileri burada SAP'ye oluşturup cevapları dön.
        //    SAPLayer.GetStockReport access = new SAPLayer.GetStockReport();

        //    foreach (var item in GetStockMovement)
        //    {

        //        //access.CreateGoodIssueAll(item);
        //    }

        //    //CevaReturn.GetGoodIssueResponse result = new CevaReturn.GetGoodIssueResponse();
        //    List<CevaReturn.GetStockReportResponse> resultList = new List<CevaReturn.GetStockReportResponse>();
        //    //List<CevaReturn.Response> response = new List<CevaReturn.Response>();

        //    //result.Body.GetGoodIssueResult = response.ToArray();

        //    //resultList.Add(result);

        //    return resultList;
        //}

        [WebMethod]
        public List<SAPLayer.Response> AddStockTransfer(List<StockTransfer> _StockTransfer)
        {
            //Gelen verileri burada SAP'ye oluşturup cevapları dön.
            SAPLayer.AddStockTransfer access = new SAPLayer.AddStockTransfer();

            List<SAPLayer.Response> Response = new List<SAPLayer.Response>();

            Response = access.AddStockTransferToSAP(_StockTransfer);

            return Response;
        }
    }
}
