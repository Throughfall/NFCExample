using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Fragment = Android.Support.V4.App.Fragment;

namespace NFCExample
{
    public class Fragment2 : Fragment
    {

        private View view;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            view = inflater.Inflate(Resource.Layout.NFCWrite, container, false);
            var writeButton = view.FindViewById<Button>(Resource.Id.writeData);

            var myActivity = (MainActivity)this.Activity;
            writeButton.Click += myActivity.WriteToTag;


            //var nfcContent = view.FindViewById<EditText>(Resource.Id.nfcNewContent);
            //nfcContent.Text = "";
            // Use this to return your custom view for this Fragment
            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            var text = view.FindViewById<EditText>(Resource.Id.nfcNewContent); // now cleaning up!
            text.Text = "";
        }

    }
}