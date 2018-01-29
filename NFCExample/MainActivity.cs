using Android.App;
using Android.OS;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.Content;
using Android.Widget;
using System;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;

namespace NFCExample
{
    [Activity(Label = "NFCExample", MainLauncher = true, Icon = "@drawable/nfcicon")]
    [IntentFilter(new[] { NfcAdapter.ActionTagDiscovered, NfcAdapter.ActionTechDiscovered, NfcAdapter.ActionNdefDiscovered },
      Categories = new[] { Intent.CategoryDefault },
      DataScheme = "vnd.android.nfc",
      DataPathPrefix = "letypetype",
      DataHost = "ext")]
    public class MainActivity : AppCompatActivity
    {
        public NfcAdapter _nfcAdapter;

        // For foreground nfc handling.
        String TAG = "MainActivity";

        EditText nfcContent;

        PendingIntent nfcPi;
        IntentFilter nfcFilter;

        string newLine = System.Environment.NewLine;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Neue Instanz des NfcAdapters dieses Smartphones
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(ApplicationContext);

            // Ansicht von der "Main"-Layout-Ressource darstellen.
            SetContentView(Resource.Layout.Main);

            TabLayout tabLayout = (TabLayout)FindViewById(Resource.Id.tablayout_navigation);

            ViewPager viewPager = (ViewPager)FindViewById(Resource.Id.pager);
            SetupViewPager(viewPager);

            tabLayout.SetupWithViewPager(viewPager);

            var intent = new Intent(this, this.Class);
            intent.AddFlags(ActivityFlags.SingleTop);
            nfcPi = PendingIntent.GetActivity(this, 0, intent, 0);
            nfcFilter = new IntentFilter(NfcAdapter.ActionTechDiscovered);
            nfcFilter.AddCategory(Intent.CategoryDefault);

        }

        protected override void OnResume()
        {
            base.OnResume();

            // Es konnte kein NFC-Chip auf dem Smartphone erkannt werden
            if (_nfcAdapter == null)
            {
                // Aufruf der Methode zur Ausgabe der Fehlermeldung (NFC-Tag ist auf diesem Gerät nicht verfügbar)
                setAlert("NFC nicht verfügbar", "NFC ist auf diesem Gerät leider nicht verfügbar!");
            }
            // Das Gerät ist NFC-fähig
            else
            {
                var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                var filters = new[] { tagDetected };
                var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);
                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
            }
        }


        // Zusätzliche Methode zum Lesen eines NFC-Tag
        public void ReadFromTag(object sender, EventArgs e)
        {
            try
            {
                // Aufruf der Methode zur Ausgabe der Fehlermeldung (Fehler beim Lesen des NFC-Tags)
                ProcessIntent(Intent);
            }
            catch (Exception ex)
            {
                // Aufruf der Methode zur Ausgabe der Fehlermeldung (Fehler beim Lesen des NFC-Tags)
                setAlert("NFC-Fehler", "Fehler beim Lesen des NFC-Tags: \n \n" + ex);
            }
        }

        public void WriteToTag(object sender, EventArgs e)
        {
            //Zuweisung der NFC - Tag Daten an das Content-Steuerelement auf der Hauptseite
            EditText nfcContent = FindViewById<EditText>(Resource.Id.nfcNewContent);

            // Aufruf der Methode zum Schreiben des Textinhaltes auf das NFC-Tag
            if (WriteToTag(Intent, nfcContent.Text))
            {
                // Der Schreibvorgang verlief erfolgreich
                Toast.MakeText(this, "NFC-Tag erfolgreich beschrieben!", duration: ToastLength.Long).Show();
            }
            else
            {
                // Beim schreiben auf das NFC-Tag ist ein Fehler aufgetreten
                Toast.MakeText(this, "Daten konnten nicht übertragen werden!", duration: ToastLength.Long).Show();
            }
        }


        // Zusätzliche Methode zum Schreiben auf ein NFC-Tag
        public bool WriteToTag(Intent intent, string content)
        {
            // Neue Instanz des NfcAdapters dieses Smartphones
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            // Eine Instanz des NFC-Tag Objekts anhand der Intent-Daten
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

            // Prüfung des Tags == ist es vorhanden?
            if (tag != null)
            {
                Ndef ndef = Ndef.Get(tag);
                if (ndef != null && ndef.IsWritable)
                {
                    var payload = System.Text.Encoding.ASCII.GetBytes(content);
                    var mimeBytes = System.Text.Encoding.ASCII.GetBytes("text/plain");
                    var record = new NdefRecord(NdefRecord.TnfWellKnown, mimeBytes, new byte[0], payload);
                    var ndefMessage = new NdefMessage(new[] { record });

                    ndef.Connect();
                    ndef.WriteNdefMessage(ndefMessage);
                    ndef.Close();
                    return true;
                }
            }
            return false;
        }

        protected override void OnNewIntent(Intent intent)
        {
            Intent = intent;

            if (NfcAdapter.ActionTagDiscovered == intent.Action)
            {
                // Ein NFC-Tag konnte gefunden werden
                Toast.MakeText(this, "NFC-Tag gefunden", duration: ToastLength.Long).Show();
                ProcessIntent(Intent);
            }

        }

        protected override void OnPause()
        {
            base.OnPause();
            // App is paused, so no need to keep an eye out for NFC tags.
            if (_nfcAdapter != null)
                _nfcAdapter.DisableForegroundDispatch(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_nfcAdapter != null)
            {
                _nfcAdapter.Dispose();
                _nfcAdapter = null;
            }
        }

        private void ProcessIntent(Intent intent)
        {
            try
            {
                // Eine Instanz des NFC-Tag Objekts anhand der Intent-Daten
                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

                // Der Cast zu einem Tag-Objekt lief schief, also handelt es sich nicht um ein selbiges 
                if (tag != null)
                {
                    // Alle "Ndef Nachrichten" abrufen
                    var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
                    if (rawMessages != null)
                    {
                        var msg = (NdefMessage)rawMessages[0];

                        // Den Ndef Datensatz ausgeben, welcher die aktuellen Daten beinhaltet
                        var record = msg.GetRecords()[0];
                        if (record != null)
                        {
                            if (record.Tnf == NdefRecord.TnfWellKnown)
                            // Die Datenspezifikation des RTD (Record Type Definition) kann unter http://members.nfc-forum.org/specs/spec_list/ nachgelesen werden
                            {
                                // Die übermittelten Daten zuweisen
                                var data = System.Text.Encoding.ASCII.GetString(record.GetPayload());

                                //Zuweisung der NFC - Tag Daten an das Content-Steuerelement auf der Hauptseite
                                EditText nfcContent = FindViewById<EditText>(Resource.Id.nfcContent);
                                nfcContent.Text = data;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                // Aufruf der Methode zur Ausgabe der Fehlermeldung (Fehler bei der Verarbeitung des NFC-Tags)
                setAlert("NFC-Fehler", "Fehler bei der Verarbeitung des NFC-Tags! \n \n" + ex);
            }
        }

        private void setAlert(string title, string message)
        {
            var alert = new Android.App.AlertDialog.Builder(this).Create();
            alert.SetMessage(message);
            alert.SetTitle(title);
            alert.Show();
        }
        //==============================================================================================================================================

        private void SetupViewPager(ViewPager viewPager)
        {
            viewPager.OffscreenPageLimit = 3;

            PageAdapter adapter = new PageAdapter(SupportFragmentManager);
            adapter.AddFragment(new Fragment1(), "Read data");
            adapter.AddFragment(new Fragment2(), "Write data");
            adapter.AddFragment(new Fragment3(), "About the App");

            viewPager.Adapter = adapter;

        }
    }
}

