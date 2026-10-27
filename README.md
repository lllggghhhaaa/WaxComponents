# WaxComponents

A UI component library for Blazor, built with Tailwind CSS and light/dark mode support.

- NuGet: [https://www.nuget.org/packages/WaxComponents](https://www.nuget.org/packages/WaxComponents)
- Source: [https://github.com/lllggghhhaaa/WaxComponents](https://github.com/lllggghhhaaa/WaxComponents)
- License: Apache-2.0
- Target: .NET 11.0

Components: Accordion, Alert, Avatar, Badge, Button, Card, Checkbox, Chip, Channel Button, Color Picker, Dialog, Empty State, Fab, Icon Button, Navigation (Sidebar, Server Button), Popover, Progress, Radio, Reaction Chip, Section Heading, Select, Setting Row, Slider, Spoiler, Switch, Tabs, Text Field.

## Install

```bash
dotnet add package WaxComponents
```

## Setup

Register the services in `Program.cs`:

```csharp
builder.Services.AddWaxComponents();
```

Add the stylesheet and theme support to `App.razor`:

```razor
<link rel="stylesheet" href="_content/WaxComponents/app.css" />
<FontMap />
<ThemePreloader />
```

## Usage

```razor
<WaxButton Variant="WaxButtonVariant.Filled" OnClick="OnClick">
    <ChildContent>Click me</ChildContent>
</WaxButton>

<WaxBadge Color="WaxColor.Primary">New</WaxBadge>

<WaxSwitch Label="Dark mode" />
```

```csharp
private void OnClick() { }
```

## Theming

Inject `IThemeService` to change the theme at runtime:

```razor
@inject IThemeService Theme

<button @onclick="() => Theme.ToggleDarkLightAsync()">Toggle theme</button>
```

Modes: `ThemeMode.System`, `ThemeMode.Light`, `ThemeMode.Dark`.