using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;

namespace XamarinBluetoothScanner.Droid
{
    [Activity(Label = "XamarinBluetoothScanner", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : AppCompatActivity
    {
        IBluetoothLE _bluetooth = CrossBluetoothLE.Current;
        Plugin.BLE.Abstractions.Contracts.IAdapter _adapter = CrossBluetoothLE.Current.Adapter;

        private int _rssiThreshold = -50;

        List<DeviceItem> deviceItems = new List<DeviceItem>();
        ListView listview;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            listview = FindViewById<ListView>(Resource.Id.myListView);
            scan();

            deviceItems.Add(new DeviceItem()
            {
                DeviceName = "null",
                DeviceId = "000000"
            });
            deviceItems.Add(new DeviceItem()
            {
                DeviceName = "null",
                DeviceId = "000001"
            });
            listview.Adapter = new DeviceAdapter(this, deviceItems);

        }
        public async void scan()
        {
            this._adapter.ScanTimeout = 3000;
            this._adapter.DeviceDiscovered += this.DeviceDiscovered;
            if (!this._adapter.IsScanning)
            {
                await this._adapter.StartScanningForDevicesAsync();
            }
        }

        private void DeviceDiscovered(object sender, DeviceEventArgs args)
        {
            this._adapter.DeviceDiscovered += (s, a) =>
            {

                if (a.Device.Rssi > _rssiThreshold && a.Device.Rssi < 0)
                {
                    Console.WriteLine(">> BeaconScan(): Find device UUID: {0} RSSI: {1}", a.Device.Id, a.Device.Rssi);
                    deviceItems.Add(new DeviceItem()
                    {
                        DeviceName = a.Device.Name.ToString(),
                        DeviceId = a.Device.Id.ToString()
                    });
                }

            };
        }

        public class DeviceItem
        {
            public string DeviceName { get; set; }
            public string DeviceId { get; set; }
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
                view.FindViewById<TextView>(Resource.Id.textView1).Text = item.DeviceName;
                view.FindViewById<TextView>(Resource.Id.textView2).Text = item.DeviceId;
                view.FindViewById<ImageView>(Resource.Id.imageView1).SetBackgroundColor(item.Color);

                return view;
            }
        }
    }
}