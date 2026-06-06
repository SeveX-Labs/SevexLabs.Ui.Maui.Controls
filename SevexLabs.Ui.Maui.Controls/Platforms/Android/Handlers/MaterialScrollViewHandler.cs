using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace SevexLabs.Ui.Maui.Controls.Handlers
{
    public class MaterialScrollViewHandler : ScrollViewHandler
    {
        #region properties

        public static IPropertyMapper<IScrollView, MaterialScrollViewHandler> OwnMapper = new PropertyMapper<MaterialScrollView, MaterialScrollViewHandler>(ViewMapper)
        {
            [nameof(MaterialScrollView.Content)] = MapContent,
            [nameof(MaterialScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
            [nameof(MaterialScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
            [nameof(MaterialScrollView.Orientation)] = MapOrientation,
            [nameof(MaterialScrollView.IsScrollEnabled)] = MapIsScrollEnabled
        };

        #endregion

        #region ctor(s)

        public MaterialScrollViewHandler() : base(OwnMapper)
        {
        }

        #endregion

        #region overrides

        protected override MauiScrollView CreatePlatformView()
        {
            return new MaterialScrollViewNativeView(Context!);
        }

        protected override void DisconnectHandler(MauiScrollView platformView)
        {
            platformView.SetOnTouchListener(null);

            if (platformView is MaterialScrollViewNativeView nativeView)
            {
                nativeView.IsScrollEnabled = true;
            }

            base.DisconnectHandler(platformView);
        }

        private static void MapIsScrollEnabled(MaterialScrollViewHandler handler, MaterialScrollView scrollView)
        {
            if (handler.PlatformView is MaterialScrollViewNativeView nativeView)
            {
                nativeView.IsScrollEnabled = scrollView.IsScrollEnabled;
            }
        }

        #endregion
    }
}
