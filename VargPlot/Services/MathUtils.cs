using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VargPlot;

public class MathUtils
{
    public static List<int> FindDivisibleIntegers(float minVal, float maxVal, int divisor)
    {
        var result = new List<int>();

        int start = (int)Math.Ceiling(minVal);
        int end = (int)Math.Floor(maxVal);

        // First number divisible by divisor and >= start
        int firstDivisible = ((start + divisor - 1) / divisor) * divisor;

        for (int val = firstDivisible; val <= end; val += divisor)
            result.Add(val);

        return result;
    }
}
