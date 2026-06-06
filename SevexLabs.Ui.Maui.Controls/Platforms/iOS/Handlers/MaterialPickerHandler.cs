using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace SevexLabs.Ui.Maui.Controls.Handlers;

public class MaterialPickerHandler : PickerHandler
{
    protected override void ConnectHandler(MauiPicker platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is MaterialPicker)
            platformView.EditingDidBegin += HandleEditingDidBegin;

        var cancelButton = new UIBarButtonItem("Annulla", UIBarButtonItemStyle.Plain, (s, e) =>
        {
            (VirtualView as MaterialPicker)?.RaiseSelectionCancelled();
            platformView.ResignFirstResponder(); // chiude il picker
        });

        var space = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

        var doneButton = new UIBarButtonItem("Fine", UIBarButtonItemStyle.Done, (s, e) =>
        {
            (VirtualView as MaterialPicker)?.RaiseSelectionConfirmed();
            platformView.ResignFirstResponder(); // chiude il picker
        });

        var toolbar = new UIToolbar();
        toolbar.SizeToFit();
        toolbar.SetItems(new[] { cancelButton, space, doneButton }, false);

        platformView.InputAccessoryView = toolbar;
    }

    protected override void DisconnectHandler(MauiPicker platformView)
    {
        if (VirtualView is MaterialPicker)
            platformView.EditingDidBegin -= HandleEditingDidBegin;

        base.DisconnectHandler(platformView);
    }

    private void HandleEditingDidBegin(object? sender, EventArgs e)
    {
        if (VirtualView is MaterialPicker cp)
        {
            cp.InitialSelectedIndex = cp.SelectedIndex;

            // Se è -1, forziamo il primo item
            if (cp.SelectedIndex == -1 && cp.Items.Count > 0)
            {
                cp.SelectedIndex = 0;
            }
        }
    }
}