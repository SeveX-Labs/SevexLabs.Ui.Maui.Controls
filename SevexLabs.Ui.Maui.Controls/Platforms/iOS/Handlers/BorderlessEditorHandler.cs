using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace SevexLabs.Ui.Maui.Controls.Handlers
{
    public class BorderlessEditorHandler : EditorHandler
    {
        #region properties

        public static readonly IPropertyMapper<IEditor, BorderlessEditorHandler> OwnMapper =
            new PropertyMapper<IEditor, BorderlessEditorHandler>(EditorHandler.Mapper)
            {
                [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
                [nameof(IPlaceholder.PlaceholderColor)] = MapPlaceholderColor,
                [nameof(BorderlessEditor.PlaceholderFontFamily)] = MapPlaceholderFontFamily,
                [nameof(ITextStyle.Font)] = MapFont
            };

        #endregion

        #region ctor(s)

        public BorderlessEditorHandler() : base(OwnMapper)
        {
        }

        #endregion

        #region overrides

        protected override void ConnectHandler(MauiTextView platformView)
        {
            base.ConnectHandler(platformView);

            RemoveNativeBorder(platformView);
            UpdatePlaceholderLabel(platformView, VirtualView);

            platformView.Changed += OnPlatformViewChanged;
        }

        protected override void DisconnectHandler(MauiTextView platformView)
        {
            platformView.Changed -= OnPlatformViewChanged;

            base.DisconnectHandler(platformView);
        }

        #endregion

        #region mapper methods

        private static void MapPlaceholder(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholderLabel(handler.PlatformView, editor);
        }

        private static void MapPlaceholderColor(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholderLabel(handler.PlatformView, editor);
        }

        private static void MapPlaceholderFontFamily(BorderlessEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is null)
                return;

            UpdatePlaceholderLabel(handler.PlatformView, editor);
        }

        private static void MapFont(BorderlessEditorHandler handler, IEditor editor)
        {
            EditorHandler.MapFont(handler, editor);

            if (handler.PlatformView is null)
                return;

            UpdatePlaceholderLabel(handler.PlatformView, editor);
        }

        #endregion

        #region event handlers

        private void OnPlatformViewChanged(object? sender, EventArgs e)
        {
            if (PlatformView is null || VirtualView is null)
                return;

            UpdatePlaceholderLabel(PlatformView, VirtualView);
        }

        #endregion

        #region helper methods

        private static void RemoveNativeBorder(MauiTextView platformView)
        {
            platformView.BackgroundColor = UIColor.Clear;
            platformView.Layer.BorderWidth = 0;
            platformView.Layer.CornerRadius = 0;
            platformView.TextContainerInset = UIEdgeInsets.Zero;
            platformView.TextContainer.LineFragmentPadding = 0;
            platformView.ClipsToBounds = true;
        }

        private static void UpdatePlaceholderLabel(MauiTextView platformView, IEditor editor)
        {
            if (platformView is null || editor is null)
                return;

            var placeholder = (editor as IPlaceholder)?.Placeholder ?? string.Empty;
            var placeholderColor = (editor as IPlaceholder)?.PlaceholderColor?.ToPlatform() ?? UIColor.LightGray;

            var label = FindPlaceholderLabel(platformView);
            if (label is null)
                return;

            label.Text = placeholder;
            label.TextColor = placeholderColor;
            label.Font = ResolvePlaceholderFont(editor, platformView);

            var hasText = !string.IsNullOrEmpty(editor.Text);
            label.Hidden = hasText || string.IsNullOrEmpty(placeholder);
        }

        private static UILabel? FindPlaceholderLabel(MauiTextView platformView)
        {
            return platformView.Subviews
                .OfType<UILabel>()
                .OrderBy(v => v.Frame.Y)
                .FirstOrDefault();
        }

        private static UIFont ResolvePlaceholderFont(IEditor editor, MauiTextView platformView)
        {
            var fontSize = platformView.Font?.PointSize ?? UIFont.SystemFontSize;

            if (editor is BorderlessEditor borderlessEditor &&
                !string.IsNullOrWhiteSpace(borderlessEditor.PlaceholderFontFamily))
            {
                var placeholderFont = UIFont.FromName(borderlessEditor.PlaceholderFontFamily, fontSize);
                if (placeholderFont is not null)
                    return placeholderFont;
            }

            return platformView.Font ?? UIFont.SystemFontOfSize(fontSize);
        }

        #endregion
    }
}