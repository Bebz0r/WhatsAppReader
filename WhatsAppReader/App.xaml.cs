using WhatsAppReader.Model;

namespace WhatsAppReader;

public partial class App : Application
{
    // Preferences
    public static Prefs thePrefs;

    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
