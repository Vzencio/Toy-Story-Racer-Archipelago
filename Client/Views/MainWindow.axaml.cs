using Avalonia.Controls;

using TSRAP.GUI.ViewModels;


namespace TSRAP.GUI.Views;


public partial class MainWindow :
    Window
{
    public MainWindow()
    {
        InitializeComponent();


        Closing +=
            async (sender, args) =>
            {
                if (
                    DataContext
                    is MainViewModel viewModel
                )
                {
                    await viewModel
                        .ShutdownAsync();
                }
            };
    }
}