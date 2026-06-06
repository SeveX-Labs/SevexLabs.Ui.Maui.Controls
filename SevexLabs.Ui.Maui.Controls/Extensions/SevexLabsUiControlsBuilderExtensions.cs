using SevexLabs.Ui.Maui.Controls.Handlers;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SevexLabs.Ui.Maui.Controls.Extensions
{
    public static class SevexLabsUiControlsBuilderExtensions
    {
        public static MauiAppBuilder UseSevexLabsUiControls(this MauiAppBuilder builder)
        {
            builder
                .UseSkiaSharp()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(BorderlessEntry), typeof(BorderlessEntryHandler));
                    handlers.AddHandler(typeof(BorderlessEditor), typeof(BorderlessEditorHandler));
                    handlers.AddHandler(typeof(MaterialScrollView), typeof(MaterialScrollViewHandler));
                    handlers.AddHandler(typeof(FastBorder), typeof(FastBorderHandler));

#if IOS
                    handlers.AddHandler(typeof(MaterialPicker), typeof(MaterialPickerHandler));
#endif
                });

            return builder;

        }
    }
}
