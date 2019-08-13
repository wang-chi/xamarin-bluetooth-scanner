namespace XamarinBluetoothScanner.Models
{
    public class DeviceItem
    {
        public string DeviceName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceAddress { get; set; }
        public int RSSI { get; set; }
        public string UpdateTime { get; set; }
        public string DeviceMajor { get; set; }
        public string DeviceMinor { get; set; }
        public string TXPower { get; set; }
        public string AdvFlags { get; set; }
        public string AdvHeader { get; set; }
        public string CompanyID { get; set; }
        public string BeaconType { get; set; }
        public string BeaconLength { get; set; }
    }
}
