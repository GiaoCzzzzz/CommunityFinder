using Microsoft.Maui.Controls;

namespace CommunityFinder.Views;

public partial class ThemePage : ContentPage
{
    public ThemePage()
    {
        InitializeComponent();
        // Set initial mode text
        CurrentModeLabel.Text = "Current Mode: Normal";
    }

    private void NormalModeButton_Clicked(object sender, EventArgs e)
    {
        // Set global font size to normal (12)
        Application.Current.Resources["GlobalFontSize"] = 12.0;
        CurrentModeLabel.Text = "Current Mode: Normal";
    }

    private void ElderModeButton_Clicked(object sender, EventArgs e)
    {
        // Set global font size to elder mode (16)
        Application.Current.Resources["GlobalFontSize"] = 20.0;
        CurrentModeLabel.Text = "Current Mode: Elder";
    }
}
