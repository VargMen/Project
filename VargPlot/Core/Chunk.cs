using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class Chunk
{
    public Chunk(float time, List<float> values)
    {
        Time = time;
        Values = values;
    }
    public float Time;
    public List<float> Values;
}
