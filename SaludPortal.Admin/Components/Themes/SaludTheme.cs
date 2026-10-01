using MudBlazor;

namespace SaludPortal.Admin.Components.Themes;

// Paleta de colores corporativos Neuquén:
//   Azul    rgb(43,  62,  76 ) → #2B3E4C
//   Verde   rgb(135, 184, 103) → #87B867
//   Beige 1 rgb(252, 242, 220) → #FCF2DC
//   Beige 2 rgb(244, 223, 185) → #F4DFB9
public static class SaludTheme
{
    public static MudTheme Light { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary        = "#2B3E4C",   // azul-nqn
            PrimaryDarken  = "#1e2d38",
            PrimaryLighten = "#3e5a6d",
            Secondary      = "#87B867",   // verde-nqn
            SecondaryDarken  = "#5f8a44",
            SecondaryLighten = "#a8ce87",
            Tertiary       = "#F4DFB9",   // beige-nqn-2
            Info           = "#2B3E4C",
            Success        = "#87B867",
            Warning        = "#D97706",
            Error          = "#DC2626",
            Background     = "#FAFAF7",   // blanco calido (base UI)
            Surface        = "#FFFFFF",
            AppbarBackground = "#2B3E4C",
            AppbarText     = "#F4DFB9",
            DrawerBackground = "#FFFFFF",
            DrawerText     = "#2B3E4C"
        }
    };

    public static MudTheme Dark { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary        = "#87B867",   // verde-nqn como acento en oscuro
            PrimaryDarken  = "#5f8a44",
            PrimaryLighten = "#a8ce87",
            Secondary      = "#F4DFB9",   // beige-nqn-2 como secundario
            Tertiary       = "#2B3E4C",
            Info           = "#87B867",
            Success        = "#87B867",
            Warning        = "#F59E0B",
            Error          = "#F87171",
            Background     = "#111820",
            Surface        = "#1c2632",   // azul oscuro, derivado de azul-nqn
            AppbarBackground = "#1a2633",
            AppbarText     = "#F4DFB9",
            DrawerBackground = "#16202b",
            DrawerText     = "#F4DFB9"
        }
    };

    public static MudTheme Current(bool darkMode) => darkMode ? Dark : Light;
}
