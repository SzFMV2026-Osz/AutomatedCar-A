using System.Collections.ObjectModel;
using AutomatedCar.Models;
using System.Linq;

using ReactiveUI;

namespace AutomatedCar.ViewModels
{
    using Avalonia.Controls;
    using Models;
    using System;
    using Visualization;

    public class CourseDisplayViewModel : ViewModelBase
    {
        public ObservableCollection<WorldObjectViewModel> WorldObjects { get; } = new ObservableCollection<WorldObjectViewModel>();
      
        private Avalonia.Vector offset;

        public CourseDisplayViewModel(World world)
        {
            this.WorldObjects = new ObservableCollection<WorldObjectViewModel>(world.WorldObjects.Select(wo => new WorldObjectViewModel(wo)));
            this.Width = world.Width;
            this.Height = world.Height;
            var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000 / GameBase.TicksPerSecond) };
            timer.Tick += (sender, args) => this.RefreshSensorTriangles();
            timer.Start();
        }


        private Avalonia.Points cameraTriangle = new Avalonia.Points();

        private Avalonia.Points radarTriangle = new Avalonia.Points();

        public Avalonia.Points CameraTriangle
        {
            get => this.cameraTriangle;
            set => this.RaiseAndSetIfChanged(ref this.cameraTriangle, value);
        }

        public Avalonia.Points RadarTriangle
        {
            get => this.radarTriangle;
            set => this.RaiseAndSetIfChanged(ref this.radarTriangle, value);
        }

        private void RefreshSensorTriangles()
        {
            var camera = World.Instance.ControlledCar.camera.triangle;
            var radar = World.Instance.ControlledCar.radar.triangle;
            this.CameraTriangle = new Avalonia.Points { camera.MainPoint, camera.Pt1, camera.Pt2 };
            this.RadarTriangle = new Avalonia.Points { radar.MainPoint, radar.Pt1, radar.Pt2 };
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public Avalonia.Vector Offset
        {
            get => this.offset;
            set => this.RaiseAndSetIfChanged(ref this.offset, value);
        }

        private DebugStatus debugStatus = new DebugStatus();

        public DebugStatus DebugStatus
        {
            get => this.debugStatus;
            set => this.RaiseAndSetIfChanged(ref this.debugStatus, value);
        }

        public void ShiftUp()
        {
            World.Instance.ControlledCar.MovementController.ShiftUp();
        }

        public void ShiftDown()
        {
            World.Instance.ControlledCar.MovementController.ShiftDown();
        }

        public void PageUp()
        {
            World.Instance.ControlledCar.Rotation += 5;
        }

        public void PageDown()
        {
            World.Instance.ControlledCar.Rotation -= 5;
        }

        public void ToggleDebug()
        {
            this.debugStatus.Enabled = !this.debugStatus.Enabled;
        }

        public void ToggleCamera()
        {
            this.DebugStatus.Camera = !this.DebugStatus.Camera;
        }

        public void ToggleRadar()
        {
            this.DebugStatus.Radar = !this.DebugStatus.Radar;
        }

        public void ToggleUltrasonic()
        {
            //World.Instance.DebugStatus.Ultrasonic = !World.Instance.DebugStatus.Ultrasonic;
        }

        public void ToggleRotation()
        {
            //World.Instance.DebugStatus.Rotate = !World.Instance.DebugStatus.Rotate;
        }

        public void FocusCar(ScrollViewer scrollViewer)
        {
            var offsetX = World.Instance.ControlledCar.X - (scrollViewer.Viewport.Width / 2);
            var offsetY = World.Instance.ControlledCar.Y - (scrollViewer.Viewport.Height / 2);
            this.Offset = new Avalonia.Vector(offsetX, offsetY);
        }
    }
}