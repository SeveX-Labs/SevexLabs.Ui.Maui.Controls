# SevexLabs UI MAUI Controls

Reusable UI controls for .NET MAUI.

## Install

The recommended way to use this library is through the NuGet package:

```bash
dotnet add package SevexLabs.Ui.Maui.Controls --version 1.0.2
```

Using the NuGet package is the preferred mode for application projects, CI builds,
release builds and normal consumers of the library.

## Register

Register the handlers in `MauiProgram.cs`:

```csharp
using SevexLabs.Ui.Maui.Controls.Extensions;

builder.UseSevexLabsUiControls();
```

## XAML Namespace

```xml
xmlns:controls="clr-namespace:SevexLabs.Ui.Maui.Controls;assembly=SevexLabs.Ui.Maui.Controls"
```


## Gallery

### Borders and containers

FastBorder examples covering borders, rounded cards, shadows and clipped content.

![FastBorder examples](https://raw.githubusercontent.com/SeveX-Labs/SevexLabs.Ui.Maui.Controls/main/docs/images/controls-gallery-fastborder.png)

### Inputs and forms

Material input controls for text, multiline notes, read-only values, numeric values and pickers.

![Material input controls](https://raw.githubusercontent.com/SeveX-Labs/SevexLabs.Ui.Maui.Controls/main/docs/images/controls-gallery-inputs.png)

### Buttons and selection

Buttons with icon/loading states, chips and segmented selection controls.

![Buttons and selection controls](https://raw.githubusercontent.com/SeveX-Labs/SevexLabs.Ui.Maui.Controls/main/docs/images/controls-gallery-buttons-selection.png)

### Text and navigation helpers

HTML-formatted text and slide-based step content.

![HTML label and slide steps](https://raw.githubusercontent.com/SeveX-Labs/SevexLabs.Ui.Maui.Controls/main/docs/images/controls-gallery-html-steps.png)

### Loading and progress

Loading placeholders, countdown controls and media progress.

![Loading and progress controls](https://raw.githubusercontent.com/SeveX-Labs/SevexLabs.Ui.Maui.Controls/main/docs/images/controls-gallery-loading-progress.png)

## FastBorder API

`FastBorder` is a lightweight alternative to MAUI `Border`, but it does not use
the same border property names. Use `BorderColor`, `BorderThickness`, and
`CornerRadius`.

Do not use MAUI `Border` properties such as `Stroke`, `StrokeThickness`, or
`StrokeShape` with `FastBorder`.

```xml
<controls:FastBorder
    BorderColor="#4F46E5"
    BorderThickness="1"
    CornerRadius="12"
    Padding="16">
    <Label Text="Native-rendered FastBorder" />
</controls:FastBorder>
```

## Included Controls

- `FastBorder`
- `BorderlessEntry`
- `BorderlessEditor`
- `MaterialButton`
- `MaterialDisplayField`
- `MaterialEditor`
- `MaterialEntry`
- `MaterialFlexChip`
- `MaterialFlexChips`
- `MaterialNumericEntry`
- `MaterialPicker`
- `MaterialScrollView`
- `MaterialSegment`
- `MaterialSegmentedControl`
- `HtmlFormsLabel`
- `MediaProgressBar`
- `ShimmerLayout`
- `ShimmerView`
- `SlideStepsView`
- `Countdown controls`

`MaterialEntry` includes packaged default password eye icons. Set `EyeOpenSource` and `EyeClosedSource` only when you want to replace those defaults with app-provided images.

## Local development with ProjectReference

This library is intended to be consumed primarily as a NuGet package.

For local development, debugging, or testing changes before publishing a new package, you can also clone the repository and reference the project directly with a `ProjectReference` from a consuming .NET MAUI app.

This mode is optional and should be treated as a development-only workflow. Normal consumers should use the NuGet package.

### Enable ProjectReference mode locally

To enable local `ProjectReference` mode, create a file named:

```text
Directory.Build.local.props
```

in the same directory as:

```text
Directory.Build.props
```

Do not commit this file. It is meant to contain local machine/developer settings only.

Recommended local configuration:

```xml
<Project>
	<PropertyGroup>
		<UseAsProjectReference>true</UseAsProjectReference>
		<OverrideAndroidSpecificVersion>36.0</OverrideAndroidSpecificVersion>
		<!-- Optional, only if the consuming app requires a specific MacCatalyst platform version. -->
		<!-- <OverrideMacCatalystSpecificVersion>26.0</OverrideMacCatalystSpecificVersion> -->
	</PropertyGroup>
</Project>
```

### What this does

By default, the project uses package-oriented, generic .NET MAUI platform TFMs, for example:

```text
net10.0-ios
net10.0-android
```

When `UseAsProjectReference` is enabled, the project can adjust its target frameworks to match the platform-specific target required by a consuming app.

For example, with:

```xml
<OverrideAndroidSpecificVersion>36.0</OverrideAndroidSpecificVersion>
```

the Android target becomes:

```text
net10.0-android36.0
```

This is useful when a consuming app targets a specific Android platform version and the library is referenced directly as a project instead of as a NuGet package.

If `UseAsProjectReference=true` is set and `OverrideAndroidSpecificVersion` is not provided, the project is configured to fall back to Android `36.0` for ProjectReference mode. Setting the value explicitly is still recommended because it makes the consuming setup easier to read.

### Optional iOS override

If a consuming app requires a specific iOS platform version, use:

```xml
<OverrideIosSpecificVersion>26.0</OverrideIosSpecificVersion>
```

In that case, the iOS target becomes:

```text
net10.0-ios26.0
```

If `OverrideIosSpecificVersion` is not set, the iOS target remains generic:

```text
net10.0-ios
```

### Optional MacCatalyst override

Projects that include MacCatalyst also support:

```xml
<OverrideMacCatalystSpecificVersion>26.0</OverrideMacCatalystSpecificVersion>
```

When set together with `UseAsProjectReference=true`, this changes the MacCatalyst target to:

```text
net10.0-maccatalyst26.0
```

### Important notes

- `Directory.Build.local.props` is for local development only.
- Do not commit `Directory.Build.local.props`.
- Normal NuGet builds and CI builds should run without this local file.
- When the local file is not present, `UseAsProjectReference` defaults to `false`.
- When `UseAsProjectReference` is `false`, the project uses its normal package-oriented target frameworks.
- If the project supports MacCatalyst, `OverrideMacCatalystSpecificVersion` can be used in the same way as the Android and iOS overrides.
- If you switch between package mode and project-reference mode, clean `bin` and `obj` folders before rebuilding.
- Restore and build should be performed in the same mode. If restore runs with local overrides enabled, build should use the same overrides.
- If Rider keeps building against an old Android/iOS target after switching modes, reload all projects. If the problem persists, use **File > Invalidate Caches...** and reopen the solution.

## Packing and testing the NuGet package locally

When creating or testing the NuGet package, make sure the local ProjectReference overrides are disabled. Otherwise the package can be produced with development-specific target frameworks.

Before packing, temporarily rename the local props file if it exists:

```bash
mv Directory.Build.local.props Directory.Build.local.props.disabled
```

Then clean generated folders from the repository root:

```bash
find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
```

Pack the library by calling `dotnet pack` directly on the library `.csproj`, not on the solution root:

```bash
dotnet pack SevexLabs.Ui.Maui.Controls/SevexLabs.Ui.Maui.Controls.csproj \
  -c Release \
  -o ./local-nuget
```

Packing the concrete library project avoids unintentionally building sample apps, tests, or other projects in the solution.

After packing, you can re-enable your local development settings:

```bash
mv Directory.Build.local.props.disabled Directory.Build.local.props
```

To inspect the generated package contents:

```bash
unzip -l ./local-nuget/SevexLabs.Ui.Maui.Controls.1.0.2.nupkg | grep "lib/"
```

With .NET MAUI/.NET 10, it is normal for the generated `.nupkg` to contain platform-normalized asset folders such as:

```text
lib/net10.0-android36.0/
lib/net10.0-ios26.0/
```

even when the project file declares generic TFMs such as `net10.0-android` or `net10.0-ios`. Those platform versions are resolved by the installed .NET SDK/workloads during build/pack.

To test the package without publishing it, add `./local-nuget` as a local NuGet source in a consuming app and use the normal `PackageReference` workflow. This is the best way to verify the package as a real consumer would use it.
