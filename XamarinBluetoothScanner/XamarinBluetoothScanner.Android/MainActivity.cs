using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.OS;
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
            _adapter = _manager.Adapter;

            listview.Adapter = new DeviceAdapter(this, deviceItems);
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
            deviceItems.Sort((x,y)=> { return -x.RSSI.CompareTo(y.RSSI); });
            listview.Adapter = new DeviceAdapter(this, deviceItems);
        }

        public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
        {
            string tempUUID = BitConverter.ToString(scanRecord);
            string identifierUUID = ExtractBeaconUUID(tempUUID);

            _count = _count + 1;

            Console.WriteLine("\n >> Find A Beacon[{0}] Id:{1}; RSSI:{2}; Record:{3}\n", _count, bleDevice.Address, rssi, identifierUUID);
            if (identifierUUID.Length >= 36)
            {
                if (deviceItems.Exists(x => x.DeviceAddress == bleDevice.Address))
                    deviceItems.RemoveAt(deviceItems.FindIndex(x => x.DeviceAddress == bleDevice.Address));
                deviceItems.Add(new DeviceItem()
                {
                    DeviceName = String.IsNullOrEmpty(bleDevice.Name) ? null : bleDevice.Name.ToString().Trim(),
                    DeviceId = identifierUUID,
                    DeviceAddress = bleDevice.Address,
                    RSSI = rssi,
                    UpdateTime = DateTime.Now
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
            public DateTime UpdateTime { get; set; }
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

        private string ExtractBeaconUUID(string stringAdvertisementSpecificData)
        {
            string[] parse = stringAdvertisementSpecificData.Split("-");

            if (parse.Count() < 60)
            {
                return stringAdvertisementSpecificData;
            }
            else
            {
                var parser = string.Format("{0}{1}{2}{3}-{4}{5}-{6}{7}-{8}{9}-{10}{11}{12}{13}{14}{15}",
                                            parse[9], parse[10], parse[11], parse[12],
                                            parse[13], parse[14],
                                            parse[15], parse[16],
                                            parse[17], parse[18],
                                            parse[19], parse[20], parse[21], parse[22], parse[23], parse[24]);
                return parser.ToString();
            }
        }
    }
}