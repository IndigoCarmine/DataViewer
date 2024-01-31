using System.IO;
using System.Text;

namespace DataViewer
{
    static class LoaderFactory
    {
        public static FileLoader Create(string filepath, Type type)
        {
            throw new NotImplementedException();
        }
    }
    
    abstract class FileLoader{
        protected string filepath;
        protected double[]? _x;
        protected double[]? _y;
        public double[] x 
        { 
            get 
            {
                if (_x != null) return _x;
                
                var data = LoadValue();
                if (data != null)return data.Item1;
                return new double[0];
            }
            protected set { _x = value; }
        }
        public double[] y
        {
            get
            {
                if (_y != null) return _y;

                var data = LoadValue();
                if (data != null) return data.Item2;
                return new double[0];
            }
            protected set { _y = value; }
        }

        public FileLoader(string filepath)
        {
            this.filepath = filepath;
        }

        abstract public Tuple<double[], double[]>? LoadValue();

    }

    class NMRFileLoader : FileLoader
    {
        public NMRFileLoader(string filepath) : base(filepath)
        {
        }

        public override Tuple<double[], double[]>? LoadValue()
        {

            List<int> temp_x = new List<int>();
            List<int> temp_y = new List<int>();
            using(var fs = new FileStream(filepath,FileMode.Open))
            {

                byte[] buffer = new byte[8];
                //read 4byte and cast as float32
                while(true)
                {

                    int e = fs.Read(buffer, 0, 8);
                    if(e == 0) break;
                    var rev = buffer.Reverse().ToArray();
                    temp_x.Add(BitConverter.ToInt16(rev, 0));
                    temp_y.Add(BitConverter.ToInt16(rev, 4));

                }
            }



            x = temp_x.Select(e=>(double)e).ToArray();
            y = temp_y.Select(e=>(double)e).ToArray();


            return Tuple.Create(x, y);
        }


        private int toInt16(byte[] data)
        {
            return BitConverter.ToInt32(data.Reverse().ToArray(), 0);
        }

    }



    class FtIrFileLoader : FileLoader
    {
        public FtIrFileLoader(string filepath) : base(filepath)
        {
        }

        public override Tuple<double[], double[]>? LoadValue()
        {
            bool is_data = false;



            List<double> temp_x = new List<double>();
            List<double> temp_y = new List<double>();


            using (var fs = new StreamReader(filepath, Encoding.UTF8))
            {
                while (true)
                {
                    string? line = fs.ReadLine();
                    if (line == null) break;
                    if (line == "XYDATA") { is_data = true; continue; }

                    if (!is_data) continue;
                    if (line == "") break;
                    var line_data = line.Split('\t');
                    temp_x.Add(double.Parse(line_data[0]));
                    temp_y.Add(double.Parse(line_data[1]));

                }
            }

            x = temp_x.ToArray();
            y = temp_y.ToArray();

            return Tuple.Create(x, y);
        }
    }

    class CSVFileLoader : FileLoader
    {

        bool has_header;

        public CSVFileLoader(string filepath,bool has_header) : base(filepath)
        {
            this.has_header = has_header;
        }

        public override Tuple<double[], double[]>? LoadValue()
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();


            using (var fs = new StreamReader(filepath, Encoding.UTF8))
            {
                if(has_header)fs.ReadLine();
                while (true)
                {
                    string? line = fs.ReadLine();
                    if (line == null) break;
                    var line_data = line.Split(',');
                    x.Add(double.Parse(line_data[0]));
                    y.Add(double.Parse(line_data[1]));

                }
            }

            return Tuple.Create(x.ToArray(), y.ToArray());
        }
    }

    class TSVFileLoader : FileLoader
    {
        bool has_header;
        public TSVFileLoader(string filepath,bool has_header) : base(filepath)
        {
            this.has_header = has_header;
        }

        public override Tuple<double[], double[]>? LoadValue()
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();


            using (var fs = new StreamReader(filepath, Encoding.UTF8))
            {
                if(has_header) fs.ReadLine();
                while (true)
                {
                    string? line = fs.ReadLine();
                    if (line == null) break;
                    var line_data = line.Split('\t');
                    x.Add(double.Parse(line_data[0]));
                    y.Add(double.Parse(line_data[1]));

                }
            }

            return Tuple.Create(x.ToArray(), y.ToArray());
        }
    }
}
