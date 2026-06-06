using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace SevexLabs.Ui.Maui.Controls.Handlers
{
    public class BorderlessEntryHandler : EntryHandler
    {
        #region properties

        public static readonly IPropertyMapper<IEntry, BorderlessEntryHandler> OwnMapper =
            new PropertyMapper<IEntry, BorderlessEntryHandler>(EntryHandler.Mapper)
        {
            [nameof(IView.Background)] = MapBackground,
            [nameof(IEntry.Text)] = MapTextPreserveSelection,
            [nameof(BorderlessEntry.PlaceholderFontFamily)] = MapPlaceholderFontFamily
        };

        #endregion

        #region ctor(s)

        public BorderlessEntryHandler() : base(OwnMapper)
        {
        }

        #endregion

        #region overrides

        protected override MauiTextField CreatePlatformView()
        {
            var platformView = base.CreatePlatformView();

            platformView.AutocorrectionType = UIKit.UITextAutocorrectionType.No;
            ApplyBorderlessStyle(platformView);

            return platformView;
        }

        protected override void ConnectHandler(MauiTextField platformView)
        {
            base.ConnectHandler(platformView);

            ApplyBorderlessStyle(platformView);
        }

        #endregion

        #region mapper methods

        private static void MapBackground(BorderlessEntryHandler handler, IEntry entry)
        {
            if (handler.PlatformView is null)
                return;

            ApplyBorderlessStyle(handler.PlatformView);
        }

        #endregion

        #region helper methods

        private static void ApplyBorderlessStyle(MauiTextField platformView)
        {
            platformView.BorderStyle = UIKit.UITextBorderStyle.None;
            platformView.Layer.BorderWidth = 0;
            platformView.BackgroundColor = UIColor.Clear;
        }

        private static void MapPlaceholderFontFamily(BorderlessEntryHandler arg1, IEntry entry)
        {
            if (Application.Current is null) return;

            if (entry is not BorderlessEntry arg2)
                return;

            if (!string.IsNullOrEmpty(arg2.PlaceholderFontFamily) && arg2.PlaceholderFontFamily != arg2.FontFamily)
            {
                // var descriptor = new UIFontDescriptor().CreateWithFamily(arg2.PlaceholderFontFamily);
                // var placeholderFont = UIFont.FromDescriptor(descriptor2, (float)arg2.FontSize);

                var placeholderFont = UIFont.FromName(arg2.PlaceholderFontFamily, (float)arg2.FontSize);
                if (placeholderFont is null)
                {
                    bool isFontFound = Application.Current.Resources.TryGetValue(arg2.PlaceholderFontFamily, out object fontResource);
                    if (isFontFound && fontResource is OnPlatform<string> onPlatformFontNames)
                    {
                        string? fontName = onPlatformFontNames.Platforms?.Where(platform => platform.Platform.Contains(nameof(DevicePlatform.iOS))).Select(platform => platform.Value as string).FirstOrDefault();
                        if (!string.IsNullOrEmpty(fontName))
                        {
                            placeholderFont = UIFont.FromName(fontName, (float)arg2.FontSize);
                        }
                    }
                }

                arg1.PlatformView.AttributedPlaceholder = new NSAttributedString(arg2.Placeholder, placeholderFont, arg2.PlaceholderColor.ToPlatform());

            }
        }

        private static void MapTextPreserveSelection(BorderlessEntryHandler handler, IEntry entry)
        {
            var tf = handler.PlatformView;
            if (tf is null)
            {
                EntryHandler.MapText(handler, entry);
                return;
            }

            // Se non e focused, non serve preservare caret
            if (!tf.IsFirstResponder)
            {
                EntryHandler.MapText(handler, entry);
                return;
            }

            // Salva selezione attuale
            var range = tf.SelectedTextRange;
            var b = tf.BeginningOfDocument;

            var start = 0;
            var end = 0;

            if (range != null)
            {
                start = (int)tf.GetOffsetFromPosition(b, range.Start);
                end = (int)tf.GetOffsetFromPosition(b, range.End);
            }

            // Aggiorna testo (questa e la chiamata che normalmente "butta" il cursore in fondo)
            EntryHandler.MapText(handler, entry);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var textLen = tf.Text?.Length ?? 0;
                    if (textLen < 0) textLen = 0;

                    // Clamp
                    start = Math.Max(0, Math.Min(start, textLen));
                    end = Math.Max(0, Math.Min(end, textLen));

                    var from = tf.GetPosition(tf.BeginningOfDocument, start);
                    var to = tf.GetPosition(tf.BeginningOfDocument, end);

                    if (from != null && to != null)
                        tf.SelectedTextRange = tf.GetTextRange(from, to);
                }
                catch
                {
                    // ignore
                }
            });
        }

        #endregion
    }
}
