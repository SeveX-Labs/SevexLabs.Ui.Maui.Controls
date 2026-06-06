using Microsoft.Maui.Handlers;
using UIKit;

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

        protected override UIScrollView CreatePlatformView()
        {
            var platformView = base.CreatePlatformView();

            if (VirtualView is not null && VirtualView is MaterialScrollView materialScrollView)
            {
                platformView.ScrollEnabled = materialScrollView.IsScrollEnabled;
            }

            return platformView;
        }

        public override void UpdateValue(string property)
        {
            if (property == "IsScrollEnabled" && VirtualView is MaterialScrollView materialScrollView && PlatformView is not null)
            {

                PlatformView.ScrollEnabled = materialScrollView.IsScrollEnabled;
            }

            base.UpdateValue(property);
        }

        private static void MapIsScrollEnabled(MaterialScrollViewHandler arg1, MaterialScrollView arg2)
        {
            arg1.PlatformView.ScrollEnabled = arg2.IsScrollEnabled;
        }

        #endregion
    }
}