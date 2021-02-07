using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Uri = Android.Net.Uri;

namespace ProjectK.Imaging
{
    public sealed class SelectableRoundedImageView : ImageView
    {
        #region Static Fields

        public static string TAG = "SelectableRoundedImageView";

        private static readonly ScaleType[] SScaleTypeArray =
        {
            ScaleType.Matrix,
            ScaleType.FitXy,
            ScaleType.FitStart,
            ScaleType.FitCenter,
            ScaleType.FitEnd,
            ScaleType.Center,
            ScaleType.CenterCrop,
            ScaleType.CenterInside
        };


        #endregion

        #region Fields

        private int _mResource;

        // Set default scale type to FIT_CENTER, which is default scale type of
        // original ImageView.
        private ScaleType _mScaleType = ScaleType.FitCenter;

        private readonly float _leftTopCornerRadius;

        private float _mBorderWidth;
        private static readonly Color DefaultBorderColor = Color.Black;
        private ColorStateList _mBorderColor = ColorStateList.ValueOf(DefaultBorderColor);

        private bool _isOval;
        private Drawable _mDrawable;
        private float[] _mRadii = {0, 0, 0, 0, 0, 0, 0, 0};

        #endregion

        #region Constructors


        public SelectableRoundedImageView(Context context) : base(context)
        {
        }

        public SelectableRoundedImageView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public SelectableRoundedImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.SelectableRoundedImageView, defStyle, 0);

            var index = a.GetInt(Resource.Styleable.SelectableRoundedImageView_android_scaleType, -1);
            if (index >= 0)
            {
                SetScaleType(SScaleTypeArray[index]);
            }

            _leftTopCornerRadius = a.GetDimensionPixelSize(Resource.Styleable.SelectableRoundedImageView_sriv_left_top_corner_radius, 0);
            float rightTopCornerRadius = a.GetDimensionPixelSize(Resource.Styleable.SelectableRoundedImageView_sriv_right_top_corner_radius, 0);
            float mLeftBottomCornerRadius = a.GetDimensionPixelSize(Resource.Styleable.SelectableRoundedImageView_sriv_left_bottom_corner_radius, 0);
            float mRightBottomCornerRadius = a.GetDimensionPixelSize(Resource.Styleable.SelectableRoundedImageView_sriv_right_bottom_corner_radius, 0);

            if (_leftTopCornerRadius < 0.0f || rightTopCornerRadius < 0.0f
                                            || mLeftBottomCornerRadius < 0.0f || mRightBottomCornerRadius < 0.0f)
            {
                throw new IllegalArgumentException("radius values cannot be negative.");
            }

            _mRadii = new float[]
            {
                _leftTopCornerRadius, _leftTopCornerRadius,
                rightTopCornerRadius, rightTopCornerRadius,
                mRightBottomCornerRadius, mRightBottomCornerRadius,
                mLeftBottomCornerRadius, mLeftBottomCornerRadius
            };

            _mBorderWidth = a.GetDimensionPixelSize(Resource.Styleable.SelectableRoundedImageView_sriv_border_width, 0);
            if (_mBorderWidth < 0)
            {
                throw new IllegalArgumentException("border width cannot be negative.");
            }

            _mBorderColor = a.GetColorStateList(Resource.Styleable.SelectableRoundedImageView_sriv_border_color);
            if (_mBorderColor == null)
            {
                _mBorderColor = ColorStateList.ValueOf(DefaultBorderColor);
            }

            _isOval = a.GetBoolean(Resource.Styleable.SelectableRoundedImageView_sriv_oval, false);
            a.Recycle();
            UpdateDrawable();
        }

        public SelectableRoundedImageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }


        #endregion

        #region Overrides

        protected override void DrawableStateChanged()
        {
            base.DrawableStateChanged();
            Invalidate();
        }

        public override ScaleType GetScaleType()
        {
            return _mScaleType;
        }

        public override void SetScaleType(ScaleType scaleType)
        {
            base.SetScaleType(scaleType);
            _mScaleType = scaleType;
            UpdateDrawable();
    }

        public override void SetImageDrawable(Drawable drawable)
        {
            _mDrawable = SelectableRoundedCornerDrawable.FromDrawable(drawable, Resources);
            base.SetImageDrawable(drawable);
            UpdateDrawable();
    }

        public override void SetImageBitmap(Bitmap bm)
        {
            _mResource = 0;
            _mDrawable = SelectableRoundedCornerDrawable.FromBitmap(bm, Resources);
            base.SetImageBitmap(bm);
            UpdateDrawable();
        }

        public override void SetImageResource(int resId)
        {
            if (_mResource != resId)
            {
                _mResource = resId;
                _mDrawable = ResolveResource();
                base.SetImageDrawable(_mDrawable);
                UpdateDrawable();
            }
        }

        public override void SetImageURI(Uri uri)
        {
            base.SetImageURI(uri);
            SetImageDrawable(Drawable);
        }

        #endregion

        #region Private Functions

        private Drawable ResolveResource()
        {
            var rsrc = Resources;
            if (rsrc == null)
            {
                return null;
            }

            Drawable d = null;

            if (_mResource != 0)
            {
                try
                {
                    d = rsrc.GetDrawable(_mResource);
                }
                catch (Resources.NotFoundException e)
                {
                    Log.Warn(TAG, "Unable to find resource: " + _mResource, e);
                    // Don't try again.
                    _mResource = 0;
                }
            }

            return SelectableRoundedCornerDrawable.FromDrawable(d, Resources);
        }

        private void UpdateDrawable()
        {
            if (_mDrawable == null)
            {
                return;
            }

            ((SelectableRoundedCornerDrawable) _mDrawable).SetScaleType(_mScaleType);
            ((SelectableRoundedCornerDrawable) _mDrawable).SetCornerRadii(_mRadii);
            ((SelectableRoundedCornerDrawable) _mDrawable).SetBorderWidth(_mBorderWidth);
            ((SelectableRoundedCornerDrawable) _mDrawable).SetBorderColor(_mBorderColor);
            ((SelectableRoundedCornerDrawable) _mDrawable).SetOval(_isOval);
        }

        private float GetCornerRadius()
        {
            return _leftTopCornerRadius;
        }

        private float GetBorderWidth()
        {
            return _mBorderWidth;
        }

        /// <summary>
        /// Set border width.
        /// </summary>
        /// <param name="width">The desired width in dip.</param>
        private void SetBorderWidthDp(float width)
        {
            var scaledWidth = Resources.DisplayMetrics.Density * width;
            if (_mBorderWidth == scaledWidth)
            {
                return;
            }

            _mBorderWidth = scaledWidth;
            UpdateDrawable();
            Invalidate();
        }

        private int GetBorderColor()
        {
            return _mBorderColor.DefaultColor;
        }

        private void SetBorderColor(int color)
        {
            SetBorderColor(ColorStateList.ValueOf(new Color(color)));
        }

        private ColorStateList GetBorderColors()
        {
            return _mBorderColor;
        }

        private void SetBorderColor(ColorStateList colors)
        {
            if (_mBorderColor.Equals(colors))
            {
                return;
            }

            _mBorderColor = (colors != null)
                ? colors
                : ColorStateList
                    .ValueOf(DefaultBorderColor);
            UpdateDrawable();
            if (_mBorderWidth > 0)
            {
                Invalidate();
            }
        }

        private bool IsOval()
        {
            return _isOval;
        }

        #endregion

        #region Public Functions
        /// <summary>
        /// set radii for each corner
        /// </summary>
        /// <param name="leftTop">The desired radius for left-top corner in dip.</param>
        /// <param name="rightTop">The desired desired radius for right-top corner in dip</param>
        /// <param name="leftBottom">Left bottom the desired radius for left-bottom corner in dip</param>
        /// <param name="rightBottom">Right bottom the desired radius for right-bottom corner in dip</param>
        public void SetCornerRadiiDp(float leftTop, float rightTop, float leftBottom, float rightBottom)
        {
            var density = Resources.DisplayMetrics.Density;

            var lt = leftTop * density;
            var rt = rightTop * density;
            var lb = leftBottom * density;
            var rb = rightBottom * density;

            _mRadii = new float[] { lt, lt, rt, rt, rb, rb, lb, lb };
            UpdateDrawable();
        }

        public void SetOval(bool oval)
        {
            _isOval = oval;
            UpdateDrawable();
            Invalidate();
        }

        #endregion

    }
}