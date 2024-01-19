using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Avalonia.Layout;
using System;

namespace ColorMC.Android.UI.GameButton;

public class ButtonView : View
{
    private ButtonData _data;
    private IButtonFuntion _func;
    private Paint paint;
    private Bitmap? bitmap;
    private Matrix? matrix;

    private int RenderWidth, RenderHeight;

    public ButtonView(ButtonData data, IButtonFuntion func, Context? context) : base(context)
    {
        _data = data;
        _func = func;

        RenderWidth = DpToPx(_data.Width);
        RenderHeight = DpToPx(_data.Height);

        LoadContent();

        Click += GameButton_Click;
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();

        var layoutParams = new RelativeLayout.LayoutParams(RenderWidth, RenderHeight);

        if (_data.Horizontal == HorizontalAlignment.Center
            && _data.Vertical == VerticalAlignment.Center)
        {
            layoutParams.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
        }
        else
        {
            switch (_data.Horizontal)
            {
                case HorizontalAlignment.Left:
                    layoutParams.AddRule(LayoutRules.AlignParentStart, (int)LayoutRules.True);
                    break;
                case HorizontalAlignment.Center:
                    layoutParams.AddRule(LayoutRules.CenterHorizontal, (int)LayoutRules.True);
                    break;
                case HorizontalAlignment.Right:
                    layoutParams.AddRule(LayoutRules.AlignParentEnd, (int)LayoutRules.True);
                    break;
            }
            switch (_data.Vertical)
            {
                case VerticalAlignment.Top:
                    layoutParams.AddRule(LayoutRules.AlignParentTop, (int)LayoutRules.True);
                    break;
                case VerticalAlignment.Center:
                    layoutParams.AddRule(LayoutRules.CenterVertical, (int)LayoutRules.True);
                    break;
                case VerticalAlignment.Bottom:
                    layoutParams.AddRule(LayoutRules.AlignParentBottom, (int)LayoutRules.True);
                    break;
            }
        }

        layoutParams.SetMargins(DpToPx(_data.Margin.Left), DpToPx(_data.Margin.Top),
            DpToPx(_data.Margin.Right), DpToPx(_data.Margin.Bottom));

        LayoutParameters = layoutParams;
        Alpha = _data.Alpha;
    }

    private int DpToPx(float dp)
    {
        float density = Context?.Resources?.DisplayMetrics?.Density ?? 1f;
        return (int)Math.Round(dp * density);
    }

    private void GameButton_Click(object? sender, EventArgs e)
    {
        switch (_data.Type)
        {
            case ButtonData.ButtonType.Setting:
                _func.ShowSetting();
                break;
        }
    }

    private void LoadContent()
    {
        paint = new Paint(PaintFlags.AntiAlias)
        {
            Color = Color.ParseColor(_data.Foreground ?? "#FFFFFF"),
            TextSize = DpToPx(_data.TextSize)
        };
        paint.TextAlign = Paint.Align.Center;

        SetBackgroundColor(Color.ParseColor(_data.BackGroud ?? "#000000"));

        if (!string.IsNullOrWhiteSpace(_data.Image))
        {
            try
            {
                var bytes = Convert.FromBase64String(_data.Image);
                bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            }
            catch
            {

            }
        }
        else if (_data.Type == ButtonData.ButtonType.Setting)
        {
            bitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon);
        }

        if (bitmap != null)
        {
            int down = DpToPx(1);
            // 计算缩放比例和图片位置
            int viewWidth = RenderWidth - down * 2; // 减去两侧的间距
            int viewHeight = RenderHeight - down * 2; // 减去上下的间距
            if (viewHeight < 0 || viewHeight < 0)
            {
                return;
            }

            float scaleWidth = viewWidth / (float)bitmap.Width;
            float scaleHeight = viewHeight / (float)bitmap.Height;
            float scale = Math.Min(scaleWidth, scaleHeight); // 保持图片比例不变

            matrix = new();
            matrix.SetScale(scale, scale);

            // 计算图片居中的偏移量
            float dx = (viewWidth - bitmap.Width * scale) / 2;
            float dy = (viewHeight - bitmap.Height * scale) / 2;
            matrix.PostTranslate(dx + down, dy + down); // 添加边框间距
        }
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        if (bitmap != null && matrix != null)
        {
            canvas.DrawBitmap(bitmap, matrix, null);
        }

        if (_data.Content != null)
        {
            float baseLineY = RenderHeight / 2 - (paint.Descent() + paint.Ascent()) / 2;
            canvas.DrawText(_data.Content, RenderWidth / 2, baseLineY, paint);
        }
    }
}
