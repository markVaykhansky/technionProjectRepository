using Newtonsoft.Json;
using Physics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Testing.CustomParser;
using Testint.CustomParser;

namespace PhysicsSimulator
{

    public partial class Form1 : Form
    {
        Moveable _p;
        BoundingRect _inner, _outter;
        Timer _t;
        List<Slice> _slices;
        BoundingRect _body;
        public Form1()
        {
            InitializeComponent();
            //_p = new Moveable(0, 0, 0, 1, 2, 0.3f, 0,3f, 0);
            //_p.Init();
            _t = new Timer();
            ////_t.Tick += _t_Tick;
            //_t.Interval = 20;
            _body = new BoundingRect(0, 0, 0, 50, 50, 50);
            _inner = _body.Inflate(0.1f, 0.1f, 0.1f);
            _outter = _body.Inflate(0.2f,0.2f,0.2f);
            _slices = Slicer.SliceRect(_inner, _outter, 4); //3

            //CustomPointsListParser p = new CustomPointsListParser();
            //GameInputFile gif = new GameInputFile();
            //gif.InnerBoundaryInflRatio = new Dictionary<string, float>();
            //gif.InnerBoundaryInflRatio.Add("RatioX", 0.2f);
            //gif.InnerBoundaryInflRatio.Add("RatioY", 0.2f);
            //gif.OuterBoundaryInflRatio = new Dictionary<string, float>();
            //gif.OuterBoundaryInflRatio.Add("RatioX", 0.2f);
            //gif.OuterBoundaryInflRatio.Add("RatioY", 0.2f);
            //gif.Players = new Dictionary<string, Dictionary<string, List<UserDefinedPoint>>>();
            //gif.Players.Add("efi", new Dictionary<string, List<UserDefinedPoint>>());
            //gif.Players["efi"].Add("InnerBoundaryCheck", new List<UserDefinedPoint>());
            //gif.Players["efi"]["InnerBoundaryCheck"].Add(new UserDefinedPoint() { X = 0.1f, AX = 0.8f, Z=0.4f });
            //gif.Players["efi"]["InnerBoundaryCheck"].Add(new UserDefinedPoint() {SliceId=2, X = 0.87f, AX = 0.2f, Y = 0.4f });
            //gif.Players.Add("igor", new Dictionary<string, List<UserDefinedPoint>>());
            //gif.Players["igor"].Add("InnerBoundaryCheck", new List<UserDefinedPoint>());
            //gif.Players["igor"]["InnerBoundaryCheck"].Add(new UserDefinedPoint() { X = 0.1f, AX = 0.8f, Z = 0.4f });
            //gif.Players["igor"]["InnerBoundaryCheck"].Add(new UserDefinedPoint() { SliceId = 2, X = 0.87f, AX = 0.2f, Y = 0.4f });
            //var res = JsonConvert.SerializeObject(gif);
            //File.WriteAllText(@"c:\Tests\1.json", res);
        }

        private void _t_Tick(object sender, EventArgs e)
        {
            var res = _p.GetNextPosition();
            chart1.Series["x"].Points.AddXY(res.X, res.Y);        
                //chart1.Series["x"].Points.AddY(res.X);
            //chart1.Series["y"].Points.AddY(res.Y);
            //chart1.Series["z"].Points.AddY(res.Z);
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() =>
                {
                    textBox1.AppendText(res.ToString() + Environment.NewLine);
                }));
            }
            else
            {
                textBox1.AppendText(res.ToString() + Environment.NewLine);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.Text = "Details:\n";
            textBox2.AppendText("inner: " + _inner.ToString()+'\n');
            textBox2.AppendText("outter: " + _outter.ToString()+'\n');
            foreach(var s in _slices)
            {
                textBox2.AppendText(s.ToString() + '\n');
            }
        }
        int index = 0;
        void addSlice(Slice s)
        {
            chart1.Series[index].Points.AddXY(s.X, s.Y);
            chart1.Series[index].Points.AddXY(s.X + s.Width, s.Y);
            chart1.Series[index].Points.AddXY(s.X + s.Width, s.Y - s.Height);
            chart1.Series[index++].Points.AddXY(s.X, s.Y - s.Height);
        }
        int moveableCounter = 0;
        void addRandomPoints(Slice s)
        {
            var p = s.ConvertPoint(0.5f, 0.9f, 0.1f);
           
            chart1.Series["p"].Points.AddXY(p.X,p.Y);
        }

        Moveable a;
        private void button1_Click(object sender, EventArgs e)
        {
            //_t.Start();


            addSlice(_slices[1]);
            addSlice(_slices[0]);
            addSlice(_slices[2]);
            chart1.Series["b"].Points.AddXY(_body.X, _body.Y);
            chart1.Series["b"].Points.AddXY(_body.X + _body.Width, _body.Y);
            chart1.Series["b"].Points.AddXY(_body.X + _body.Width, _body.Y - _body.Height);
            chart1.Series["b"].Points.AddXY(_body.X, _body.Y - _body.Height);


            //addRandomPoints(_slices[0]);
            //addRandomPoints(_slices[1]);
            //addRandomPoints(_slices[2]);
            var ppp = _slices[0].ConvertPoint(0.0f, 0.0f, 0.0f); //get point in slice coordinates
            a = new Moveable((float)ppp.X, (float)ppp.Y, (float)ppp.Z, 100.0f, 0.0f, .0f, 0.0f, -100.0f, 0.0f); //create moveable object
            a.Init(); //start time
            _t.Tick += moveableTick;
            _t.Start();
        }

        private void moveableTick(object sender, EventArgs e)
        {
            var nextP = a.GetNextPosition();
            chart1.Series["p"].Points.AddXY(nextP.X, nextP.Y);
            textBox2.AppendText(nextP.ToString() + Environment.NewLine);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _t.Stop();
        }
    }
}
