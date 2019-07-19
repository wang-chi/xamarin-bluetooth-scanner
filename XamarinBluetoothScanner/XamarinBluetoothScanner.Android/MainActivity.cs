using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;

namespace XamarinBluetoothScanner.Droid
{
    [Activity(Label = "XamarinBluetoothScanner", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private int _rssiThreshold = -50;
        List<DeviceItem> deviceItems = new List<DeviceItem>();
        ListView listview;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            listview = FindViewById<ListView>(Resource.Id.myListView);

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