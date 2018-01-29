using Android.OS;
using Android.Views;
using Android.Widget;
using Fragment = Android.Support.V4.App.Fragment;

namespace NFCExample
{
    public class Fragment1 : Fragment
    {
        private View view;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            view = inflater.Inflate(Resource.Layout.NFCRead, container, false);
            var readButton = view.FindViewById<Button>(Resource.Id.readData);

            //var nfcContent = view.FindViewById<EditText>(Resource.Id.nfcContent);
            //nfcContent.Text = "";

            var myActivity = (MainActivity)this.Activity;
            readButton.Click += myActivity.ReadFromTag;

            // Use this to return your custom view for this Fragment
            return view;

        }
        public override void OnDestroyView()
        {
            base.OnDestroyView();
            var text = view.FindViewById<EditText>(Resource.Id.nfcContent); // now cleaning up!
            text.Text = "";
        }
    }
}