using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ZwillingCevaWebService.SAPLayer
{
    [Serializable]
    //[XmlRoot(ElementName = "GoodIssue", Namespace = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"")]
    public class GoodIssue
    {
        private string CompanyCodeVal;

        public string CompanyCode
        {
            get { return CompanyCodeVal; }
            set { CompanyCodeVal = value; }
        }

        private string WarehouseCodeVal;

        public string WarehouseCode
        {
            get { return WarehouseCodeVal; }
            set { WarehouseCodeVal = value; }
        }

        private string OrderNumberVal;

        public string OrderNumber
        {
            get { return OrderNumberVal; }
            set { OrderNumberVal = value; }
        }

        private string OrderReferenceNumberVal;

        public string OrderReferenceNumber
        {
            get { return OrderReferenceNumberVal; }
            set { OrderReferenceNumberVal = value; }
        }

        private string MovementCodeVal;

        public string MovementCode
        {
            get { return MovementCodeVal; }
            set { MovementCodeVal = value; }
        }

        private string IntegrationMovementCodeVal;

        public string IntegrationMovementCode
        {
            get { return IntegrationMovementCodeVal; }
            set { IntegrationMovementCodeVal = value; }
        }

        public OrderStatus OrderStatus { get; set; }

        private string TypeVal;

        public string Type
        {
            get { return TypeVal; }
            set { TypeVal = value; }
        }

        private string WaybillNumberVal;

        public string WaybillNumber
        {
            get { return WaybillNumberVal; }
            set { WaybillNumberVal = value; }
        }

        private string ShipmentNumberVal;

        public string ShipmentNumber
        {
            get { return ShipmentNumberVal; }
            set { ShipmentNumberVal = value; }
        }

        private string LoadingNumberVal;

        public string LoadingNumber
        {
            get { return LoadingNumberVal; }
            set { LoadingNumberVal = value; }
        }

        public decimal? WaybillDate { get; set; }

        public decimal? WaybillTime { get; set; }

        public decimal? ProcessDate { get; set; }

        public decimal? ProcessTime { get; set; }

        public decimal? CompleteDate { get; set; }

        public decimal? CompleteTime { get; set; }

        public decimal? DocumentDate { get; set; }

        private string FromDealerCodeVal;

        public string FromDealerCode
        {
            get { return FromDealerCodeVal; }
            set { FromDealerCodeVal = value; }
        }

        private string ToDealerCodeVal;

        public string ToDealerCode
        {
            get { return ToDealerCodeVal; }
            set { ToDealerCodeVal = value; }
        }

        private string VehicleTypeVal;

        public string VehicleType
        {
            get { return VehicleTypeVal; }
            set { VehicleTypeVal = value; }
        }

        private string PlateVal;

        public string Plate
        {
            get { return PlateVal; }
            set { PlateVal = value; }
        }

        private string DriverVal;

        public string Driver
        {
            get { return DriverVal; }
            set { DriverVal = value; }
        }

        public List<OrderDetail> Detail { get; set; }

        public List<OrderProperties> Properties { get; set; }
    }
    [Serializable]
    public class OrderDetail
    {
        private string LineNumberVal;

        public string LineNumber
        {
            get { return LineNumberVal; }
            set { LineNumberVal = value; }
        }

        private string ProductNumberVal;

        public string ProductNumber
        {
            get { return ProductNumberVal; }
            set { ProductNumberVal = value; }
        }

        private string MovementCodeVal;

        public string MovementCode
        {
            get { return MovementCodeVal; }
            set { MovementCodeVal = value; }
        }

        public decimal? CustomerFIFODate { get; set; }

        private string LotNumber1Val;

        public string LotNumber1
        {
            get { return LotNumber1Val; }
            set { LotNumber1Val = value; }
        }

        private string LotNumber2Val;

        public string LotNumber2
        {
            get { return LotNumber2Val; }
            set { LotNumber2Val = value; }
        }

        private string LotNumber3Val;

        public string LotNumber3
        {
            get { return LotNumber3Val; }
            set { LotNumber3Val = value; }
        }

        public decimal? Qty { get; set; }

        public decimal? Sim { get; set; }

        public decimal? TotalQty { get; set; }

        public decimal? OriginalOrderQty { get; set; }

        private string LineStatusVal;

        public string LineStatus
        {
            get { return LineStatusVal; }
            set { LineStatusVal = value; }
        }

        public decimal? LoadingNumber { get; set; }

        private string LineCancellationReasonVal;

        public string LineCancellationReason
        {
            get { return LineCancellationReasonVal; }
            set { LineCancellationReasonVal = value; }
        }

        public decimal? LineCancellationQty { get; set; }

        private string BlockageReasonval;

        public string BlockageReason
        {
            get { return BlockageReasonval; }
            set { BlockageReasonval = value; }
        }


        public decimal? OrderDate { get; set; }

        public decimal? OrderTime { get; set; }

        public decimal? CompleteDate { get; set; }

        public decimal? CompleteTime { get; set; }

        public OrderProperties Properties { get; set; }

        public SerialNumbers SerialNumbers { get; set; }
    }
    [Serializable]
    public class OrderProperties
    {
        public int? PropertyCode { get; set; }

        private string PropertyVal;

        public string PropertyValue1
        {
            get { return PropertyVal; }
            set { PropertyVal = value; }
        } 

        public float? PropertyValue2 { get; set; }

        public decimal? PropertyValue3 { get; set; }
    }
    [Serializable]
    public enum OrderStatus
    {
        FullPick,
        PartialPick,
        Cancel,
        Delete,
        CancellationAfterShipment
    }

    [Serializable]
    public class SerialNumbers
    {

        private string SerialNumber1Val;

        public string SerialNumber1
        {
            get { return SerialNumber1Val; }
            set { SerialNumber1Val = value; }
        }


        private string SerialNumber2Val;

        public string SerialNumber2
        {
            get { return SerialNumber2Val; }
            set { SerialNumber2Val = value; }
        }


        private string SerialNumber3Val;

        public string SerialNumber3
        {
            get { return SerialNumber3Val; }
            set { SerialNumber3Val = value; }
        }

        private string BarcodeNumberVal;

        public string BarcodeNumber
        {
            get { return BarcodeNumberVal; }
            set { BarcodeNumberVal = value; }
        }

        private string MasterSerialNumberVal;

        public string MasterSerialNumber
        {
            get { return MasterSerialNumberVal; }
            set { MasterSerialNumberVal = value; }
        }

        private string MasterLineNumberVal;

        public string MasterLineNumber
        {
            get { return MasterLineNumberVal; }
            set { MasterLineNumberVal = value; }
        }
    }
}
