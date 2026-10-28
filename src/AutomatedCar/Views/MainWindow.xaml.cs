namespace AutomatedCar.Views
{
    using AutomatedCar.ViewModels;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Markup.Xaml;

    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
            AutomatedCar.Models.World.Instance.ControlledCar.collisionDetector.Collision += (sender, worldObject) =>
                new Window
                {
                    Title = "Collision",
                    Width = 300,
                    Height = 100,
                    Content = new TextBlock { Text = "Collision with: " + worldObject.Filename, Margin = new Avalonia.Thickness(20) },
                }.Show(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // false while the key is held down and the OS repeats the event
            bool isNewKeyPress = Keyboard.Keys.Add(e.Key);
            base.OnKeyDown(e);

            MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

            // throttle, brake and steering (arrow keys) are read by the MovementController every tick
            if (isNewKeyPress && e.Key == Key.Q)
            {
                viewModel.CourseDisplay.ShiftUp();
            }

            if (isNewKeyPress && e.Key == Key.A)
            {
                viewModel.CourseDisplay.ShiftDown();
            }

            if (Keyboard.IsKeyDown(Key.PageUp))
            {
                viewModel.CourseDisplay.PageUp();
            }

            if (Keyboard.IsKeyDown(Key.PageDown))
            {
                viewModel.CourseDisplay.PageDown();
            }

            if (Keyboard.IsKeyDown(Key.D1))
            {
                viewModel.CourseDisplay.ToggleDebug();
            }

            if (e.KeySymbol == "ő" || e.KeySymbol == "Ő")
            {
                viewModel.CourseDisplay.ToggleCamera();
            }

            if (e.KeySymbol == "ú" || e.KeySymbol == "Ú")
            {
                viewModel.CourseDisplay.ToggleRadar();
            }

            if (Keyboard.IsKeyDown(Key.D4))
            {
                viewModel.CourseDisplay.ToggleUltrasonic();
            }

            if (Keyboard.IsKeyDown(Key.D5))
            {
                viewModel.CourseDisplay.ToggleRotation();
            }

            if (Keyboard.IsKeyDown(Key.F1))
            {
                new HelpWindow().Show();
                Keyboard.Keys.Remove(Key.F1);
            }

            var scrollViewer = this.Get<CourseDisplayView>("courseDisplay").Get<ScrollViewer>("scrollViewer");
            viewModel.CourseDisplay.FocusCar(scrollViewer);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Keyboard.Keys.Remove(e.Key);
            base.OnKeyUp(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}