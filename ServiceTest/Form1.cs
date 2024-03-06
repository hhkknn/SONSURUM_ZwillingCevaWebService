using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ZwillingCevaWebService.CevaReturn.GetGoodReceiptRequest aa = new ZwillingCevaWebService.CevaReturn.GetGoodReceiptRequest();
            //List<ZwillingCevaWebService.CevaReturn.GetGoodReceiptRequest> bb = new List<ZwillingCevaWebService.CevaReturn.GetGoodReceiptRequest>();
            //ZwillingCevaWebService.CevaReturn.GoodReceipt cc = new ZwillingCevaWebService.CevaReturn.GoodReceipt();
            //List<ZwillingCevaWebService.CevaReturn.GoodReceipt> dd = new List<ZwillingCevaWebService.CevaReturn.GoodReceipt>();
            //List<ZwillingCevaWebService.CevaReturn.GetGoodReceiptResponse> resp = new List<ZwillingCevaWebService.CevaReturn.GetGoodReceiptResponse>();

            //aa.Body = new ZwillingCevaWebService.CevaReturn.GetGoodReceiptRequestBody();

            //cc.DealerCode = "V00231";
            ////cc.DocumentDate = Convert.ToDecimal(DateTime.Now);

            //cc.AsnNumber = "3146";
            //cc.LineNumber = "0";
            //cc.Qty = 1;
            //cc.ProductNumber = "000019320";

            //dd.Add(cc);

            //aa.Body.goodReceipt = dd.ToArray();

            //bb.Add(aa);

            ////string requestText = "";
            ////requestText = richTextBox1.Text;

            ////ZwillingCevaWebService.ZwillingCeva nesne = new ZwillingCevaWebService.ZwillingCeva();
            ////resp = nesne.GetGoodReceipt(bb);

            //var yy = XmlUtils.SerializeToXml(resp);
            //listBox1.Text = resp.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ZwillingCevaWebService.CevaReturn.GetGoodIssueRequest nesne = new ZwillingCevaWebService.CevaReturn.GetGoodIssueRequest();
            //ZwillingCevaWebService.CevaReturn.OrderDetail detay = new ZwillingCevaWebService.CevaReturn.OrderDetail();
            //List<ZwillingCevaWebService.CevaReturn.OrderDetail> detayList = new List<ZwillingCevaWebService.CevaReturn.OrderDetail>();
            //ZwillingCevaWebService.CevaReturn.GetGoodIssueResponse resp = new ZwillingCevaWebService.CevaReturn.GetGoodIssueResponse();

            //nesne.Body = new ZwillingCevaWebService.CevaReturn.GetGoodIssueRequestBody();
            //nesne.Body.goodIssue = new ZwillingCevaWebService.CevaReturn.GoodIssue();
            //nesne.Body.goodIssue.ToDealerCode = "C00145";
            //nesne.Body.goodIssue.OrderNumber = "6334";
            //nesne.Body.goodIssue.MovementCode = "200";
            //detay.ProductNumber = "TTT03";
            //detay.LineNumber = "0";
            //detay.Qty = 1;

            //detayList.Add(detay);

            //nesne.Body.goodIssue.Detail = detayList.ToArray();

            //ZwillingCevaWebService.ZwillingCeva service = new ZwillingCevaWebService.ZwillingCeva();
            //resp = service.GetGoodIssue(nesne);

            //var yy = XmlUtils.SerializeToXml(resp);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //ZwillingCevaWebService.CevaReturn.GetGoodIssueRequest nesne = new ZwillingCevaWebService.CevaReturn.GetGoodIssueRequest();
            //ZwillingCevaWebService.CevaReturn.OrderDetail detay = new ZwillingCevaWebService.CevaReturn.OrderDetail();
            //List<ZwillingCevaWebService.CevaReturn.OrderDetail> detayList = new List<ZwillingCevaWebService.CevaReturn.OrderDetail>();
            //ZwillingCevaWebService.CevaReturn.GetGoodIssueResponse resp = new ZwillingCevaWebService.CevaReturn.GetGoodIssueResponse();

            //nesne.Body = new ZwillingCevaWebService.CevaReturn.GetGoodIssueRequestBody();
            //nesne.Body.goodIssue = new ZwillingCevaWebService.CevaReturn.GoodIssue();
            //nesne.Body.goodIssue.ToDealerCode = "C00083";
            //nesne.Body.goodIssue.OrderNumber = "7208";
            //nesne.Body.goodIssue.MovementCode = "204";

            //detay.ProductNumber = "341832010";
            //detay.LineNumber = "0";
            //detay.Qty = 1;

            //detayList.Add(detay);

            //detay = new ZwillingCevaWebService.CevaReturn.OrderDetail();

            //detay.ProductNumber = "361151510";
            //detay.LineNumber = "1";
            //detay.Qty = 1;

            //detayList.Add(detay);

            //nesne.Body.goodIssue.Detail = detayList.ToArray();

            //ZwillingCevaWebService.ZwillingCeva service = new ZwillingCevaWebService.ZwillingCeva();
            ////resp = service.GetGoodIssue(nesne);

            ////var yy = XmlUtils.SerializeToXml(resp);

            //List<ZwillingCevaWebService.Models.StockTransfer> n = new List<ZwillingCevaWebService.Models.StockTransfer>();
        }
    }
}