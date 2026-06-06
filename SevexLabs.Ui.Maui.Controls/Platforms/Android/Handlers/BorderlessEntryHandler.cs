using Android.Content.Res;
using Android.Graphics;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Android.Views.ViewGroup;
using Color = Android.Graphics.Color;

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

        protected override MauiAppCompatEditText CreatePlatformView()
        {
            var platformView = base.CreatePlatformView();

            ApplyBorderlessStyle(platformView);
            platformView.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;

            return platformView;
        }

        protected override void ConnectHandler(MauiAppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);

            ApplyBorderlessStyle(platformView);
        }

        #endregion

        #region mapper methods

        private static void MapBackground(BorderlessEntryHandler handler, IEntry entry)
        {
            /*
             * Non richiamiamo il mapper base del background:
             * su Android può riapplicare background/tint nativi e far
             * ricomparire l'underline dell'EditText.
             */

            if (handler.PlatformView is null)
                return;

            ApplyBorderlessStyle(handler.PlatformView);
        }

        #endregion

        #region helper methods

        private static void ApplyBorderlessStyle(AppCompatEditText platformView)
        {
            platformView.SetBackgroundResource(0);
            platformView.Background = null;

            if (OperatingSystem.IsAndroidVersionAtLeast(21))
            {
                platformView.BackgroundTintList = ColorStateList.ValueOf(Color.Transparent);
                platformView.BackgroundTintMode = PorterDuff.Mode.Clear;
            }

            ViewCompat.SetBackgroundTintList(
                platformView,
                ColorStateList.ValueOf(Color.Transparent));

            MarginLayoutParams layoutParams = new MarginLayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            if (platformView.LayoutParameters is not null)
            {
                layoutParams = new MarginLayoutParams(platformView.LayoutParameters);
            }

            layoutParams.SetMargins(0, 0, 0, 0);
            platformView.LayoutParameters = layoutParams;
            platformView.SetPadding(0, 0, 0, 0);
            platformView.SetMinimumHeight(0);
            platformView.SetMinHeight(0);

            platformView.Invalidate();
            platformView.RequestLayout();
        }

        private static void MapPlaceholderFontFamily(BorderlessEntryHandler arg1, IEntry arg2)
        {
        }

        private static void MapTextPreserveSelection(BorderlessEntryHandler handler, IEntry entry)
        {
            var edit = handler.PlatformView; // MauiAppCompatEditText / AppCompatEditText
            if (edit is null)
            {
                EntryHandler.MapText(handler, entry);
                return;
            }

            // Se non è focused, non serve preservare caret
            if (!edit.HasFocus)
            {
                EntryHandler.MapText(handler, entry);
                return;
            }

            // Salva selezione attuale
            var start = edit.SelectionStart;
            var end = edit.SelectionEnd;

            // Aggiorna testo (questa � la chiamata che spesso sposta il cursore)
            EntryHandler.MapText(handler, entry);

            // Ripristina selezione sul thread UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var len = edit.Text?.Length ?? 0;

                    // Clamp (Android pu� dare -1 in alcuni casi)
                    start = Math.Max(0, Math.Min(start, len));
                    end = Math.Max(0, Math.Min(end, len));

                    if (start == end)
                        edit.SetSelection(start);
                    else
                        edit.SetSelection(start, end);
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
