using Bachelor_Project.Electrode_Types;
using Bachelor_Project.Simulation.Agent_Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bachelor_Project.Simulation
{
    internal class Droplet: TileEntity
    {

        public string Substance_Name { get; set; }
        public string Color { get; set; }
        public float Temperature { get; set; }

        public int Size { get; set; }
        public float Volume { get; set; }
        public ArrayList Occupy { get; set; } = [];

        public Droplet(Input input, float volume, string substance_name, string name = "") : base(input.PositionX, input.PositionY, 1, 1, name)
        {
            Temperature = 20;
            Volume = volume;
            Substance_Name = substance_name;
            Color = GetColor(Substance_Name);
            Size = getSize(volume);
            Droplet_Actions.InputDroplet(this, input, Size);
        }

        private string GetColor(String substance_name)
        {
            return "0000FF"; // Needs to be changed to a color based on the substance name.
        }

        private int getSize(float Volume)
        {
            return ((int)Volume/12)+1;
        }

    }
}
