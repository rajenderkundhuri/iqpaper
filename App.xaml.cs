using Microsoft.Extensions.DependencyInjection;

namespace SIPrep;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var navPage = new NavigationPage(new MainPage())
		{
			BarBackgroundColor = Color.FromArgb("#1E3A8A"),
			BarTextColor = Colors.White
		};
		return new Window(navPage);
	}
}