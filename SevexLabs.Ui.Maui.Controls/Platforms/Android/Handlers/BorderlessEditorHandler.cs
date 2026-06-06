using Android.Content.Res;
using Android.Graphics;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Android.Views.ViewGroup;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace SevexLabs.Ui.Maui.Controls.Handlers
{
    public class BorderlessEditorHandler : EditorHandler
    {
        #region fields

        private bool _isFocused;

        #endregion

        #region properties

        public static readonly IPropertyMapper<IEditor, BorderlessEditorHandler> OwnMapper =
            new PropertyMapper<IEditor, BorderlessEditorHandler>(EditorHandler.Mapper)
            {
                [nameof(IView.Background)] = MapBackground,
                [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
                [nameof(IPlaceholder.PlaceholderColor)] = MapPlaceholderColor,
                [nameof(BorderlessEditor.PlaceholderFontFamily)] = MapPlaceholderFontFamily,
            };

        #endregion

        #region ctor(s)

        public BorderlessEditorHandler() : base(OwnMapper)
        {
        }

        #endregion

        #region overrides

        protected override MauiAppCompatEditText CreatePlatformView()
        {
            var platformView = base.CreatePlatformView();

            ApplyBorderlessStyle(platformView);
            ApplyEditorBehavior(platformView);

            platformView.ImeOptions = (ImeAction)ImeFlags.NoExtractUi;

            var layoutParams = new MarginLayoutParams(
                LayoutParams.MatchParent,
                LayoutParams.MatchParent);

            if (platformView.LayoutParameters is not null)
            {
                layoutParams = new MarginLayoutParams(platformView.LayoutParameters)
                {
                    Width = LayoutParams.MatchParent,
                    Height = LayoutParams.MatchParent
                };
            }

            layoutParams.SetMargins(0, 0, 0, 0);
            platformView.LayoutParameters = layoutParams;

            return platformView;
        }

        protected override void ConnectHandler(MauiAppCompatEditText platformView)
        {
            base.ConnectHandler(platformView);

            platformView.FocusChange += OnPlatformViewFocusChange;
            platformView.Touch += OnPlatformViewTouch;

            ApplyBorderlessStyle(platformView);
            ApplyEditorBehavior(platformView);
            UpdatePlaceholder(platformView, VirtualView);
        }

        protected override void DisconnectHandler(MauiAppCompatEditText platformView)
        {
            platformView.Parent?.RequestDisallowInterceptTouchEvent(false);

            platformView.FocusChange -= OnPlatformViewFocusChange;
            platformView.Touch -= OnPlatformViewTouch;

            base.DisconnectHandler(platformView);
        }

        #endregion

        #region mapper methods

        private static new void MapBackground(BorderlessEditorHandler handler, IEditor editor)
        {
            /*
             * Non richiamiamo EditorHandler.MapBackground(handler, editor),
             * perché su Android può riapplicare background/tint nativi
             * e far ricomparire la linea viola dell'EditText.
             */

            if (handler.PlatformView is null)
                return;

            ApplyBorderlessStyle(handler.PlatformView);
            ApplyEditorBehavior(handler.PlatformView);
        }

        private static void MapPlaceholder(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholder(handler.PlatformView, editor);
        }

        private static void MapPlaceholderColor(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholder(handler.PlatformView, editor);
        }

        private static void MapPlaceholderFontFamily(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholder(handler.PlatformView, editor);
        }

        #endregion

        #region event handlers

        private void OnPlatformViewFocusChange(object? sender, View.FocusChangeEventArgs e)
        {
            if (sender is not AppCompatEditText platformView)
                return;

            _isFocused = e.HasFocus;

            ApplyBorderlessStyle(platformView);
            ApplyEditorBehavior(platformView);

            if (!_isFocused)
                platformView.Parent?.RequestDisallowInterceptTouchEvent(false);
        }

        private void OnPlatformViewTouch(object? sender, View.TouchEventArgs e)
        {
            if (sender is not AppCompatEditText platformView)
                return;

            if (e.Event is null)
                return;

            switch (e.Event.ActionMasked)
            {
                case MotionEventActions.Down:
                    /*
                     * Il primo DOWN può essere quello che dà focus all'Editor.
                     * Lo blocchiamo subito per evitare che la ScrollView padre
                     * intercetti il gesto prima dell'EditText.
                     */
                    platformView.Parent?.RequestDisallowInterceptTouchEvent(true);
                    break;

                case MotionEventActions.Move:
                    /*
                     * Durante il movimento lasciamo gestire lo scroll interno
                     * solo se l'Editor è effettivamente in focus.
                     */
                    platformView.Parent?.RequestDisallowInterceptTouchEvent(_isFocused);
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    /*
                     * Appena il gesto termina, restituiamo lo scroll al parent.
                     */
                    platformView.Parent?.RequestDisallowInterceptTouchEvent(false);
                    break;
            }

            /*
             * Non impostare true:
             * l'EditText deve continuare a gestire cursore, selezione,
             * focus, tastiera e scroll interno.
             */
            e.Handled = false;
        }

        #endregion

        #region helper methods

        private static void ApplyBorderlessStyle(AppCompatEditText platformView)
        {
            RemoveNativeUnderline(platformView);

            platformView.SetPadding(0, 0, 0, 0);
            platformView.SetMinimumHeight(0);
            platformView.SetMinHeight(0);
            platformView.SetIncludeFontPadding(true);
            platformView.StateListAnimator = null;

            if (platformView.LayoutParameters is MarginLayoutParams marginLayoutParams)
            {
                marginLayoutParams.Width = LayoutParams.MatchParent;
                marginLayoutParams.Height = LayoutParams.MatchParent;
                marginLayoutParams.SetMargins(0, 0, 0, 0);
                platformView.LayoutParameters = marginLayoutParams;
            }

            platformView.Invalidate();
            platformView.RequestLayout();
        }

        private static void ApplyEditorBehavior(AppCompatEditText platformView)
        {
            if (platformView.LayoutParameters is MarginLayoutParams marginLayoutParams)
            {
                marginLayoutParams.Width = LayoutParams.MatchParent;
                marginLayoutParams.Height = LayoutParams.MatchParent;
                marginLayoutParams.SetMargins(0, 0, 0, 0);
                platformView.LayoutParameters = marginLayoutParams;
            }
            else
            {
                platformView.LayoutParameters = new MarginLayoutParams(
                    LayoutParams.MatchParent,
                    LayoutParams.MatchParent);
            }

            platformView.SetSingleLine(false);
            platformView.SetHorizontallyScrolling(false);

            platformView.SetMinLines(1);

            /*
             * Non limitiamo le righe visibili.
             * L'Editor deve poter contenere tante righe e scrollarle internamente.
             */
            platformView.SetMaxLines(int.MaxValue);

            platformView.Gravity = GravityFlags.Top | GravityFlags.Start;

            /*
             * Scroll interno dell'EditText.
             */
            platformView.VerticalScrollBarEnabled = true;
            platformView.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
            platformView.OverScrollMode = OverScrollMode.IfContentScrolls;

            platformView.SetScroller(new Scroller(platformView.Context));
            platformView.MovementMethod = ScrollingMovementMethod.Instance;

            /*
             * Necessario per ricevere correttamente eventi touch, focus e tastiera.
             */
            platformView.Focusable = true;
            platformView.FocusableInTouchMode = true;
            platformView.Clickable = true;
            platformView.LongClickable = true;

            /*
             * Il padding visivo lo gestisce MaterialEditor.
             */
            platformView.SetPadding(0, 0, 0, 0);
            platformView.SetIncludeFontPadding(true);

            platformView.SetMinimumHeight(0);
            platformView.SetMinHeight(0);

            platformView.Invalidate();
            platformView.RequestLayout();
        }

        private static void RemoveNativeUnderline(AppCompatEditText platformView)
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
        }

        private static void UpdatePlaceholder(AppCompatEditText platformView, IEditor editor)
        {
            if (editor is IPlaceholder placeholder)
            {
                platformView.Hint = placeholder.Placeholder ?? string.Empty;

                if (placeholder.PlaceholderColor is not null)
                    platformView.SetHintTextColor(placeholder.PlaceholderColor.ToPlatform());
            }

            if (editor is BorderlessEditor borderlessEditor &&
                !string.IsNullOrWhiteSpace(borderlessEditor.PlaceholderFontFamily))
            {
                try
                {
                    var typeface = Typeface.Create(
                        borderlessEditor.PlaceholderFontFamily,
                        TypefaceStyle.Normal);

                    if (typeface is not null)
                        platformView.Typeface = typeface;
                }
                catch
                {
                    /*
                     * Ignoro font non risolvibili.
                     */
                }
            }
        }

        #endregion
    }
}
