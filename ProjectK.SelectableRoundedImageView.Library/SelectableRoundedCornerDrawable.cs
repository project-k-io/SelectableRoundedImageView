using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Java.Lang;

namespace ProjectK.Imaging
{
    public sealed class SelectableRoundedCornerDrawable : Drawable
    {
        #region Static Fields

        private static readonly string Tag = "SelectableRoundedCornerDrawable";
        private static readonly Color DefaultBorderColor = Color.Black;

        #endregion

        #region Fields
        private readonly RectF _bounds = new RectF();
        private readonly RectF _borderBounds = new RectF();

        private readonly RectF _bitmapRect = new RectF();
        private readonly int _bitmapWidth;
        private readonly int _bitmapHeight;

        private readonly Paint _bitmapPaint;
        private readonly Paint _borderPaint;

        private readonly BitmapShader _bitmapShader;

        private readonly float[] _radii = { 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly float[] _borderRadii = { 0, 0, 0, 0, 0, 0, 0, 0 };

        private bool _oval;

        private float _borderWidth;
        private ColorStateList _borderColor = ColorStateList.ValueOf(DefaultBorderColor);
        // Set default scale type to FIT_CENTER, which is default scale type of
        // original ImageView.
        private ImageView.ScaleType _scaleType = ImageView.ScaleType.FitCenter;

        private readonly Path _path = new Path();
        private readonly Bitmap _bitmap;
        private bool _boundsConfigured;

        #endregion

        #region Constructor

        private SelectableRoundedCornerDrawable(Bitmap bitmap, Resources r)
        {
            _bitmap = bitmap;
            _bitmapShader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

            if (bitmap != null)
            {
                _bitmapWidth = bitmap.GetScaledWidth(r.DisplayMetrics);
                _bitmapHeight = bitmap.GetScaledHeight(r.DisplayMetrics);
            }
            else
            {
                _bitmapWidth = _bitmapHeight = -1;
            }

            _bitmapRect.Set(0, 0, _bitmapWidth, _bitmapHeight);

            _bitmapPaint = new Paint { AntiAlias = true };
            _bitmapPaint.SetStyle(Paint.Style.Fill);
            _bitmapPaint.SetShader(_bitmapShader);

            _borderPaint = new Paint { AntiAlias = true };
            _borderPaint.SetStyle(Paint.Style.Stroke);
            _borderPaint.Color = new Color(_borderColor.GetColorForState(GetState(), DefaultBorderColor));
            _borderPaint.StrokeWidth = _borderWidth;
        }


        #endregion

        #region Private Functions

        private void ConfigureBounds(Canvas canvas)
        {
            // I have discovered a truly marvelous explanation of this,
            // which this comment space is too narrow to contain. :)
            // If you want to understand what's going on here,
            // See http://www.joooooooooonhokim.com/?p=289
            var clipBounds = canvas.ClipBounds;
            var canvasMatrix = canvas.Matrix;

            if (ImageView.ScaleType.Center == _scaleType)
            {
                _bounds.Set(clipBounds);
            }
            else if (ImageView.ScaleType.CenterCrop == _scaleType)
            {
                ApplyScaleToRadii(canvasMatrix);
                _bounds.Set(clipBounds);
            }
            else if (ImageView.ScaleType.FitXy == _scaleType)
            {
                var m = new Matrix();
                m.SetRectToRect(_bitmapRect, new RectF(clipBounds), Matrix.ScaleToFit.Fill);
                _bitmapShader.SetLocalMatrix(m);
                _bounds.Set(clipBounds);
            }
            else if (ImageView.ScaleType.FitStart == _scaleType ||
                     ImageView.ScaleType.FitEnd == _scaleType ||
                     ImageView.ScaleType.FitCenter == _scaleType ||
                     ImageView.ScaleType.CenterInside == _scaleType)
            {
                ApplyScaleToRadii(canvasMatrix);
                _bounds.Set(_bitmapRect);
            }
            else if (ImageView.ScaleType.Matrix == _scaleType)
            {
                ApplyScaleToRadii(canvasMatrix);
                _bounds.Set(_bitmapRect);
            }
        }
        private void ApplyScaleToRadii(Matrix m)
        {
            var values = new float[9];
            m.GetValues(values);
            for (var i = 0; i < _radii.Length; i++)
            {
                _radii[i] = _radii[i] / values[0];
            }
        }
        private void AdjustCanvasForBorder(Canvas canvas)
        {
            var canvasMatrix = canvas.Matrix;
            var values = new float[9];
            canvasMatrix.GetValues(values);

            var scaleFactorX = values[0];
            var scaleFactorY = values[4];
            var translateX = values[2];
            var translateY = values[5];

            var newScaleX = _bounds.Width()
                            / (_bounds.Width() + _borderWidth + _borderWidth);
            var newScaleY = _bounds.Height()
                            / (_bounds.Height() + _borderWidth + _borderWidth);

            canvas.Scale(newScaleX, newScaleY);

            if (ImageView.ScaleType.FitStart == _scaleType ||
                ImageView.ScaleType.FitEnd == _scaleType ||
                ImageView.ScaleType.FitXy == _scaleType ||
                ImageView.ScaleType.FitCenter == _scaleType ||
                ImageView.ScaleType.CenterInside == _scaleType ||
                ImageView.ScaleType.Matrix == _scaleType)
            {
                canvas.Translate(_borderWidth, _borderWidth);
            }
            else if (ImageView.ScaleType.Center == _scaleType ||
                     ImageView.ScaleType.CenterCrop == _scaleType)
            {
                // First, make translate values to 0
                canvas.Translate(-translateX / (newScaleX * scaleFactorX), -translateY / (newScaleY * scaleFactorY));

                // Then, set the final translate values.
                canvas.Translate(-(_bounds.Left - _borderWidth), -(_bounds.Top - _borderWidth));
            }
        }
        private void AdjustBorderWidthAndBorderBounds(Canvas canvas)
        {
            var canvasMatrix = canvas.Matrix;
            var values = new float[9];
            canvasMatrix.GetValues(values);

            var scaleFactor = values[0];

            var viewWidth = _bounds.Width() * scaleFactor;
            _borderWidth = (_borderWidth * _bounds.Width()) / (viewWidth - (2 * _borderWidth));
            _borderPaint.StrokeWidth = _borderWidth;

            _borderBounds.Set(_bounds);
            _borderBounds.Inset(-_borderWidth / 2, -_borderWidth / 2);
        }
        private void SetBorderRadii()
        {
            for (var i = 0; i < _radii.Length; i++)
            {
                if (_radii[i] > 0)
                {
                    _borderRadii[i] = _radii[i];
                    _radii[i] = _radii[i] - _borderWidth;
                }
            }
        }
        private float GetBorderWidth()
        {
            return _borderWidth;
        }
        private int GetBorderColor()
        {
            return _borderColor.DefaultColor;
        }
        private void SetBorderColor(int color)
        {
            SetBorderColor(ColorStateList.ValueOf(new Color(color)));
        }
        private ColorStateList GetBorderColors()
        {
            return _borderColor;
        }
        private bool IsOval()
        {
            return _oval;
        }
        private ImageView.ScaleType GetScaleType()
        {
            return _scaleType;
        }
        private static Bitmap DrawableToBitmap(Drawable drawable)
        {
            if (drawable == null)
            {
                return null;
            }

            if (drawable is BitmapDrawable bitmapDrawable)
            {
                return bitmapDrawable.Bitmap;
            }

            Bitmap bitmap;
            var width = Math.Max(drawable.IntrinsicWidth, 2);
            var height = Math.Max(drawable.IntrinsicHeight, 2);
            try
            {
                bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                var canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);
            }
            catch (IllegalArgumentException e)
            {
                e.PrintStackTrace();
                bitmap = null;
            }
            return bitmap;
        }

        #endregion

        #region Overrides

        public override bool IsStateful => _borderColor.IsStateful;
        protected override bool OnStateChange(int[] state)
        {
            var newColor = _borderColor.GetColorForState(state, new Color(0));
            if (_borderPaint.Color != newColor)
            {
                _borderPaint.Color = new Color(newColor);
                return true;
            }
            else
            {
                return base.OnStateChange(state);
            }
        }
        public override void Draw(Canvas canvas)
        {
            canvas.Save();
            if (!_boundsConfigured)
            {
                ConfigureBounds(canvas);
                if (_borderWidth > 0)
                {
                    AdjustBorderWidthAndBorderBounds(canvas);
                    SetBorderRadii();
                }
                _boundsConfigured = true;
            }

            if (_oval)
            {
                if (_borderWidth > 0)
                {
                    AdjustCanvasForBorder(canvas);
                    _path.AddOval(_bounds, Path.Direction.Cw);
                    canvas.DrawPath(_path, _bitmapPaint);
                    _path.Reset();
                    _path.AddOval(_borderBounds, Path.Direction.Cw);
                    canvas.DrawPath(_path, _borderPaint);
                }
                else
                {
                    _path.AddOval(_bounds, Path.Direction.Cw);
                    canvas.DrawPath(_path, _bitmapPaint);
                }
            }
            else
            {
                if (_borderWidth > 0)
                {
                    AdjustCanvasForBorder(canvas);
                    _path.AddRoundRect(_bounds, _radii, Path.Direction.Cw);
                    canvas.DrawPath(_path, _bitmapPaint);
                    _path.Reset();
                    _path.AddRoundRect(_borderBounds, _borderRadii, Path.Direction.Cw);
                    canvas.DrawPath(_path, _borderPaint);
                }
                else
                {
                    _path.AddRoundRect(_bounds, _radii, Path.Direction.Cw);
                    canvas.DrawPath(_path, _bitmapPaint);
                }
            }
            canvas.Restore();
        }
        public override int Opacity =>
            (int)(_bitmap == null || _bitmap.HasAlpha || _bitmapPaint.Alpha < 255 ? Format.Translucent : Format.Opaque);
        public override void SetAlpha(int alpha)
        {
            _bitmapPaint.Alpha = alpha;
            InvalidateSelf();
        }
        public override void SetColorFilter(ColorFilter cf)
        {
            _bitmapPaint.SetColorFilter(cf);
            InvalidateSelf();
        }
        public override void SetDither(bool dither)
        {
            _bitmapPaint.Dither = dither;
            InvalidateSelf();
        }
        public override void SetFilterBitmap(bool filter)
        {
            _bitmapPaint.FilterBitmap = filter;
            InvalidateSelf();
        }
        public override int IntrinsicWidth => _bitmapWidth;
        public override int IntrinsicHeight => _bitmapHeight;

        #endregion

        #region Public functions

        public static SelectableRoundedCornerDrawable FromBitmap(Bitmap bitmap, Resources r)
        {
            return bitmap != null ? new SelectableRoundedCornerDrawable(bitmap, r) : null;
        }

        public static Drawable FromDrawable(Drawable drawable, Resources r)
        {
            if (drawable == null) return null;

            if (drawable is SelectableRoundedCornerDrawable)
            {
                return drawable;
            }

            if (drawable is LayerDrawable ld)
            {
                var num = ld.NumberOfLayers;
                for (var i = 0; i < num; i++)
                {
                    var d = ld.GetDrawable(i);
                    ld.SetDrawableByLayerId(ld.GetId(i), FromDrawable(d, r));
                }
                return ld;
            }

            var bm = DrawableToBitmap(drawable);
            if (bm != null)
            {
                return new SelectableRoundedCornerDrawable(bm, r);
            }

            Log.Warn(Tag, "Failed to create bitmap from drawable!");
            return drawable;
        }

        public void SetScaleType(ImageView.ScaleType scaleType)
        {
            if (scaleType == null)
            {
                return;
            }
            _scaleType = scaleType;
        }
        public void SetCornerRadii(float[] radii)
        {
            if (radii == null)
                return;

            if (radii.Length != 8)
            {
                throw new ArrayIndexOutOfBoundsException("radii[] needs 8 values");
            }

            for (var i = 0; i < radii.Length; i++)
            {
                _radii[i] = radii[i];
            }
        }

        public void SetBorderWidth(float width)
        {
            _borderWidth = width;
            _borderPaint.StrokeWidth = width;
        }

        /// <summary>
        /// Controls border color of this ImageView. 
        /// </summary>
        /// <param name="colors">The desired border color.If it's null, no border will be drawn.</param>
        public void SetBorderColor(ColorStateList colors)
        {
            if (colors == null)
            {
                _borderWidth = 0;
                _borderColor = ColorStateList.ValueOf(Color.Transparent);
                _borderPaint.Color = Color.Transparent;
            }
            else
            {
                _borderColor = colors;
                _borderPaint.Color = new Color(_borderColor.GetColorForState(GetState(), DefaultBorderColor));
            }
        }

        public void SetOval(bool oval)
        {
            _oval = oval;
        }

        #endregion
    }
}