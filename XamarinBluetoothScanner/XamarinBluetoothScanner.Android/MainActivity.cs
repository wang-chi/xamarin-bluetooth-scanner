using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XamarinBluetoothScanner.Droid
{
    [Activity(Label = "XamarinBluetoothScanner", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity, BluetoothAdapter.ILeScanCallback
    {
        Button _btnScanBle, _btnClean, _btnSort;
        protected BluetoothAdapter _adapter;
        protected BluetoothManager _manager;
        List<DeviceItem> deviceItems = new List<DeviceItem>();
        ListView listview;

        private bool isScan = false;
        private int _count = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
           
            listview = FindViewById<ListView>(Resource.Id.myListView);
            _btnScanBle = FindViewById<Button>(Resource.Id.btn_Search);
            _btnClean = FindViewById<Button>(Resource.Id.btn_Clean);
            _btnSort = FindViewById<Button>(Resource.Id.btn_Sort);

            _btnScanBle.Click += btnScanDevice;
            _btnClean.Click += btnCleanRecord;
            _btnSort.Click += btnSortRecord;

            var appContext = Android.App.Application.Context;
            _manager = (BluetoothManager)appContext.GetSystemService("bluetooth");
            _manager.Adapter.Enable();
            _adapter = _manager.Adapter;

            listview.Adapter = new DeviceAdapter(this, deviceItems);
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Permission Granted!!!");
            }

            listview.ItemClick += Listview_ItemClick;
        }

        private void Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DeviceItem deviceItem = deviceItems[e.Position];
            string msg = "Device Id: \n" + deviceItem.DeviceId + "\n" +
                         "Device Name: " + deviceItem.DeviceName + "\n" +
                         "Device Address: " + deviceItem.DeviceAddress + "\n" +
                         "Device Major: " + deviceItem.DeviceMajor + "\n" +
                         "Device Minor: " + deviceItem.DeviceMinor + "\n" +
                         "Device TXPower: " + deviceItem.TXPower + "\n" +
                         "Device CompanyID: " + deviceItem.CompanyID + "\n" +
                         "Device AdvFlags: " + deviceItem.AdvFlags + "\n" +
                         "Device AdvHeader: " + deviceItem.AdvHeader + "\n" +
                         "Device BeaconType: " + deviceItem.BeaconType + "\n" +
                         "Device BeaconLength: " + deviceItem.BeaconLength + "\n" +
                         "Device Time: \n" + deviceItem.UpdateTime + "\n";
            AlertDialog.Builder alert = new AlertDialog.Builder(this)
                .SetTitle("Device info")
                .SetMessage(msg)
                .SetPositiveButton("OK", (senderAlert, args) => { });
            alert.Create().Show();
        }

        public void btnScanDevice(object sender, EventArgs e)
        {

            if (isScan)
            {
                _adapter.StopLeScan(this);
                _btnScanBle.Text = "Start Scan";
            }
            else
            {
                _adapter.StartLeScan(this);
                deviceItems.Clear();
                _btnScanBle.Text = "Stop Scan";

            }
            isScan = !isScan;
        }

        public void btnCleanRecord(object sender, EventArgs e)
        {
            deviceItems.Clear();
            listview.Adapter = new DeviceAdapter(this, deviceItems);
        }

        public void btnSortRecord(object sender, EventArgs e)
        {
            deviceItems.Sort((x, y) => { return -x.RSSI.CompareTo(y.RSSI); });
            listview.Adapter = new DeviceAdapter(this, deviceItems);
        }

        public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
        {
            string tempUUID = BitConverter.ToString(scanRecord);
            DeviceItem deviceItem = ExtractBeaconUUID(tempUUID);

            _count = _count + 1;

            Console.WriteLine("\n >> Find A Beacon[{0}] Id:{1}; RSSI:{2}; Record:{3}\n", _count, bleDevice.Address, rssi, deviceItem.DeviceId);
            if (deviceItem.DeviceId.Length >= 36)
            {
                if (deviceItems.Exists(x => x.DeviceAddress == bleDevice.Address))
                    deviceItems.RemoveAt(deviceItems.FindIndex(x => x.DeviceAddress == bleDevice.Address));
                deviceItems.Add(new DeviceItem()
                {
                    DeviceName = String.IsNullOrEmpty(bleDevice.Name) ? null : bleDevice.Name.ToString().Trim(),
                    DeviceId = deviceItem.DeviceId,
                    DeviceAddress = bleDevice.Address,
                    DeviceMajor = deviceItem.DeviceMajor,
                    DeviceMinor = deviceItem.DeviceMinor,
                    TXPower = Int32.Parse(deviceItem.TXPower, System.Globalization.NumberStyles.HexNumber).ToString(),
                    AdvFlags = deviceItem.AdvFlags,
                    AdvHeader = deviceItem.AdvHeader,
                    CompanyID = deviceItem.CompanyID,
                    BeaconType = deviceItem.BeaconType,
                    BeaconLength = deviceItem.BeaconLength,
                    RSSI = rssi,
                    UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss：ffff")
                });
                listview.Adapter = new DeviceAdapter(this, deviceItems);
            }
        }

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
            public Android.Graphics.Color Color { get; set; }
        }

        public class DeviceAdapter : BaseAdapter<DeviceItem>
        {
            List<DeviceItem> items;
            Activity context;
            public DeviceAdapter(Activity context, List<DeviceItem> items)
                : base()
            {
                this.context = context;
                this.items = items;
            }
            public override long GetItemId(int position)
            {
                return position;
            }
            public override DeviceItem this[int position]
            {
                get { return items[position]; }
            }
            public override int Count
            {
                get { return items.Count; }
            }
            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = items[position];

                View view = convertView;
                if (view == null) // no view to re-use, create new
                    view = context.LayoutInflater.Inflate(Resource.Layout.list_item, null);
                view.FindViewById<TextView>(Resource.Id.tv_Name).Text = item.DeviceName;
                view.FindViewById<TextView>(Resource.Id.tv_UUID).Text = item.DeviceId;
                view.FindViewById<TextView>(Resource.Id.tv_Address).Text = item.DeviceAddress;
                view.FindViewById<TextView>(Resource.Id.tv_RSSI).Text = Math.Abs(item.RSSI).ToString();
                view.FindViewById<TextView>(Resource.Id.tv_UpdateTime).Text = item.UpdateTime;
                return view;
            }
        }

        private DeviceItem ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            DeviceItem deviceItem = new DeviceItem();
            string[] parse = stringAdvertisementSpecificData.Split("-");

            if (parse.Count() < 60)
            {
                deviceItem.DeviceId = stringAdvertisementSpecificData;
            }
            else
            {
                deviceItem.AdvFlags = string.Format("{0}{1}{2}", parse[0], parse[1], parse[2]);
                deviceItem.AdvHeader = string.Format("{0}{1}", parse[3], parse[4]);
                deviceItem.CompanyID = string.Format("{0}{1}", parse[5], parse[6]);
                deviceItem.BeaconType = string.Format("{0}", parse[7]);
                deviceItem.BeaconLength = string.Format("{0}", parse[8]);
                var parser = string.Format("{0}{1}{2}{3}-{4}{5}-{6}{7}-{8}{9}-{10}{11}{12}{13}{14}{15}",
                                            parse[9], parse[10], parse[11], parse[12],
                                            parse[13], parse[14],
                                            parse[15], parse[16],
                                            parse[17], parse[18],
                                            parse[19], parse[20], parse[21], parse[22], parse[23], parse[24]);
                deviceItem.DeviceId = parser.ToString();
                deviceItem.DeviceMajor = string.Format("{0}{1}", parse[25], parse[26]);
                deviceItem.DeviceMinor = string.Format("{0}{1}", parse[27], parse[28]);
                deviceItem.TXPower = string.Format("{0}", parse[29]);
            }
            return deviceItem;
        }

    }
}