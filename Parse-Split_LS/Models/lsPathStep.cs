using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse_Split_LS.Models
{
    public class LsPathStep
    {
       

        public char _StepType; // '!' it is a comment, 'J'  a joint motion, 'L' a linear motion, ' ' something else.
        public string _Body;
        public int _PointNr;

        public LsPathStep(char StepType, string Body, int PointNr)
        {
            _StepType = StepType;
            _Body = Body;
            _PointNr = PointNr;
        }
        

    }
}
