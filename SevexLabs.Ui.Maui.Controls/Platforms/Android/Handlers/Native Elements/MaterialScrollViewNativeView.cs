using Android.Content;
using Android.Views;
using Microsoft.Maui.Platform;

namespace SevexLabs.Ui.Maui.Controls.Handlers;

internal class MaterialScrollViewNativeView : MauiScrollView
{
    public MaterialScrollViewNativeView(Context context) : base(context)
    {
    }

    public bool IsScrollEnabled { get; set; } = true;

    public override bool OnInterceptTouchEvent(MotionEvent? ev)
    {
        return IsScrollEnabled && base.OnInterceptTouchEvent(ev);
    }

    public override bool OnTouchEvent(MotionEvent? ev)
    {
        return IsScrollEnabled && base.OnTouchEvent(ev);
    }
}
