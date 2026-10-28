namespace AutomatedCar
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using AutomatedCar.Models;
    using AutomatedCar.ViewModels;
    using AutomatedCar.Views;
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using AutomatedCar.NPC;
    using AutomatedCar.NPC.Routing;
    using System;
    using System.Collections;
    using AutomatedCar.Npc.Routing;

    public class App : Application
    {

        // start positions of the controlled cars per map: x, y, rotation, image
        private static readonly Dictionary<string, (int X, int Y, int Rotation, string Filename)[]> ControlledCarStarts = new Dictionary<string, (int X, int Y, int Rotation, string Filename)[]>
        {
            ["test_world"] = new[] { (480, 1425, 0, "car_1_white.png"), (4250, 1420, -90, "car_1_red.png") },
            ["oval"] = new[] { (555, 6400, 0, "car_1_white.png"), (12000, 14240, 270, "car_1_red.png") },
        };

        // kept as a field so the NPC timer lives as long as the application
        private readonly NpcManager npcManager = new NpcManager();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var world = this.CreateWorld();
                desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(world) };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public World CreateWorld()
        {
            var world = World.Instance;
            string mapName = MapSelection.Resolve(Environment.GetCommandLineArgs().Skip(1).ToList());

            // this.AddDummyCircleTo(world);

            world.PopulateFromJSON($"AutomatedCar.Assets.{mapName}.json");

            this.AddControlledCarsTo(world, mapName);

            this.AddNpcsTo(world, mapName);

            return world;
        }

        private PolylineGeometry GetControlledCarBoundaryBox()
        {
            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly()
    .GetManifestResourceStream($"AutomatedCar.Assets.worldobject_polygons.json"));
            string json_text = reader.ReadToEnd();
            dynamic stuff = JObject.Parse(json_text);
            var points = new List<Point>();
            foreach (var i in stuff["objects"][0]["polys"][0]["points"])
            {
                points.Add(new Point(i[0].ToObject<int>(), i[1].ToObject<int>()));
            }

            return new PolylineGeometry(points, false);
        }

        private void AddDummyCircleTo(World world)
        {
            var circle = new Circle(200, 200, "circle.png", 20);

            circle.Width = 40;
            circle.Height = 40;
            circle.ZIndex = 20;
            circle.Rotation = 45;

            world.AddObject(circle);
        }

        private AutomatedCar CreateControlledCar(int x, int y, int rotation, string filename)
        {
            var controlledCar = new Models.AutomatedCar(x, y, filename);

            controlledCar.Geometry = this.GetControlledCarBoundaryBox();
            controlledCar.RawGeometries.Add(controlledCar.Geometry);
            controlledCar.Geometries.Add(controlledCar.Geometry);
            controlledCar.RotationPoint = new System.Drawing.Point(54, 120);
            controlledCar.Rotation = rotation;

            controlledCar.Start();

            return controlledCar;
        }

        private void AddControlledCarsTo(World world, string mapName)
        {
            foreach (var start in ControlledCarStarts[mapName])
            {
                world.AddControlledCar(this.CreateControlledCar(start.X, start.Y, start.Rotation, start.Filename));
            }
        }

        private void AddNpcsTo(World world, string mapName)
        {
            foreach (var npc in NpcFactory.CreateAll(mapName, new JsonNpcRouteRepository()))
            {
                if (npc is WorldObject worldObject)
                {
                    world.AddObject(worldObject);
                };
                this.npcManager.Register(npc);
            }
            this.npcManager.Start();
        }
    }
}