using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Square.Picasso;

namespace com.joooonho.Sample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // All properties can be set in xml.
            var iv0 = FindViewById<SelectableRoundedImageView>(Resource.Id.image0);

            // You can set image with resource id.
            var iv1 = FindViewById<SelectableRoundedImageView>(Resource.Id.image1);
            iv1.SetScaleType(ImageView.ScaleType.CenterCrop);
            iv1.SetOval(true);
            iv1.SetImageResource(Resource.Drawable.photo_cheetah);

            // Also, You can set image with Picasso.
            // This is a normal rectangle imageview.
            var iv2 = FindViewById<SelectableRoundedImageView>(Resource.Id.image2);
            iv1.SetScaleType(ImageView.ScaleType.Center);
            Picasso.With(this).Load(Resource.Drawable.photo2).Into(iv2);

            // Of course, you can set round radius in code.
            var iv3 = FindViewById<SelectableRoundedImageView>(Resource.Id.image3);
            iv3.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.photo3));
            ((SelectableRoundedImageView)iv3).SetCornerRadiiDp(4, 4, 0, 0);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}