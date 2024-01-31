using ScottPlot;
using ScottPlot.Plottables;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileLoader? loader;
        public MainWindow()
        {
            InitializeComponent();
            ScottPlot.Control.InputBindings customInputBindings = new()
            {
                DragPanButton = ScottPlot.Control.MouseButton.Middle,
                DragZoomRectangleButton = ScottPlot.Control.MouseButton.Right,
                DragZoomButton = ScottPlot.Control.MouseButton.Right,
                ZoomInWheelDirection = ScottPlot.Control.MouseWheelDirection.Up,
                ZoomOutWheelDirection = ScottPlot.Control.MouseWheelDirection.Down,
                ClickAutoAxisButton = ScottPlot.Control.MouseButton.Right,
                ClickContextMenuButton = ScottPlot.Control.MouseButton.Left,
            };

            ScottPlot.Control.Interaction interaction = new(WpfPlot1)
            {
                Inputs = customInputBindings,
            };
            WpfPlot1.Plot.ScaleFactor = 3;
            WpfPlot1.Plot.FigureBackground = ScottPlot.Colors.Transparent;
            WpfPlot1.Plot.DataBackground = ScottPlot.Colors.Transparent;
            WpfPlot1.Plot.Style.ColorAxes(ScottPlot.Color.Gray(255));


            WpfPlot1.Interaction = interaction;
            WpfPlot1.Refresh();
        }

        private void Grid_Drag(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            //MessageBox.Show("dropped");

            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (dropFiles == null)
                return;

            foreach(var file in dropFiles)
            {
                if (File.Exists(file))
                {
                   loader = new TSVFileLoader(file, false);

                    var data = loader.LoadValue();
                    if (data == null) return;
                    //MessageBox.Show(string.Join(',',Array.ConvertAll(data.Item1,e=>e.ToString())));
                    RangeSlider.Maximum = data.Item1.Count();
                    RangeSlider.Minimum = 0;
                    
                    var scatter = WpfPlot1.Plot.Add.Scatter(data.Item1, data.Item2,color:ScottPlot.Colors.White);
                    scatter.MarkerStyle = MarkerStyle.None;
 
                    //WpfPlot1.Plot.Axes.SetLimitsX(1, -1);
                    WpfPlot1.Plot.Axes.AutoScale();
                    WpfPlot1.Refresh();
                }
                else
                {
                    MessageBox.Show("deuebfei");
                }
            }
        }

        private void Range_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loader == null) return;
            WpfPlot1.Plot.Clear();
            var seg_x = new ArraySegment<double>(loader.x);
            var seg_y = new ArraySegment<double>(loader.y);
            var scatter = WpfPlot1.Plot.Add.Scatter(seg_x.Slice((int)e.NewValue).ToArray(), seg_y.Slice((int)e.NewValue).ToArray(),color:ScottPlot.Colors.White);
            scatter.MarkerStyle = MarkerStyle.None;
            //WpfPlot1.Plot.Axes.SetLimitsX(1, -1);
            WpfPlot1.Refresh();

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            WpfPlot1.Plot.Style.AxisFrame(1, 0, 1, 0);
            WpfPlot1.Refresh();
        }

        private void WpfPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            Pixel mousePixel = new(p.X,p.Y);
            Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);
            DataPoint? nearest = (WpfPlot1.Plot.PlottableList.FirstOrDefault() as Scatter)?.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);


            if (nearest != null && nearest.HasValue)
            {
                NearestPoint.Text = nearest.Value.X.ToString() + ":" + nearest.Value.Y.ToString();
            }
        }
    }
}