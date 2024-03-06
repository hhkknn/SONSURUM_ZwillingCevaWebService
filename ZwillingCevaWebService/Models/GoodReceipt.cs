using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ZwillingCevaWebService.SAPLayer
{
    [Serializable]
    public class GoodReceipt
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

        private string AsnNumberVal;

        public string AsnNumber
        {
            get { return AsnNumberVal; }
            set { AsnNumberVal = value; }
        }


        private string DocumentNumberVal;

        public string DocumentNumber
        {
            get { return DocumentNumberVal; }
            set { DocumentNumberVal = value; }
        }

        public decimal? DocumentDate { get; set; }


        private string DealerCodeVal;

        public string DealerCode
        {
            get { return DealerCodeVal; }
            set { DealerCodeVal = value; }
        }


        public decimal? PalletNumber { get; set; }

        public decimal? ReferencePalletNumber { get; set; }

        public decimal? PlacementPalletNumber { get; set; }

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

        public decimal? ReceiptYear { get; set; }

        public decimal? ReceiptNumber { get; set; }

        public decimal? CompleteDate { get; set; }

        public decimal? CompleteTime { get; set; }

        public decimal? ProductionDate { get; set; }

        public decimal? ProductionTime { get; set; }


        private string ShiftVal;

        public string Shift
        {
            get { return ShiftVal; }
            set { ShiftVal = value; }
        }

        private string ReturnReasonVal;

        public string ReturnReason
        {
            get { return ReturnReasonVal; }
            set { ReturnReasonVal = value; }
        }

        private string PlateVal;

        public string Plate
        {
            get { return PlateVal; }
            set { PlateVal = value; }
        }

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


        private string ProductionPlaceVal;

        public string ProductionPlace
        {
            get { return ProductionPlaceVal; }
            set { ProductionPlaceVal = value; }
        }

        private string MachineCodeVal;

        public string MachineCode
        {
            get { return MachineCodeVal; }
            set { MachineCodeVal = value; }
        }

        private string FacilityCodeVal;

        public string FacilityCode
        {
            get { return FacilityCodeVal; }
            set { FacilityCodeVal = value; }
        }

        private bool GINVal;

        public bool GIN
        {
            get { return GINVal; }
            set { GINVal = value; }
        }

        private string OwnershipVal;

        public string Ownership
        {
            get { return OwnershipVal; }
            set { OwnershipVal = value; }
        }


        public decimal? Qty { get; set; }

        public decimal? Sim { get; set; }

        public decimal? TotalQty { get; set; }

        public float? Desi { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Volume { get; set; }

        public decimal? ExpirationDate { get; set; }

        public int? AtfNumber { get; set; }

        private string SequenceNumberVal;

        public string SequenceNumber
        {
            get { return SequenceNumberVal; }
            set { SequenceNumberVal = value; }
        }

        public List<GoodReceiptProperties> Properties { get; set; }
    }

    [Serializable]
    public class GoodReceiptProperties
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


}