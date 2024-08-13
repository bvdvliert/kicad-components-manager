using Avalonia.Controls;
using KiCadComponentsManager.ViewModels;

namespace KiCadComponentsManager.Views
{
    public partial class AddComponentWindow : Window
    {
        public AddComponentWindow(AddComponentViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }

        private void Binding(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        private void Binding_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}
