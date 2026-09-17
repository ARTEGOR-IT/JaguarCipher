namespace JaguarCipher.Resources.Pages;

public partial class HelpPage : ContentPage
{
    public event EventHandler? Closed;

    public HelpPage()
    {
        InitializeComponent();
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
        Closed?.Invoke(this, EventArgs.Empty);
    }
}