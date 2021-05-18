using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse_Split_LS.Models
{
    public class lsPathPoint
    {
        public int UF, UT;
        public string CONFIG;
        public double X, Y, Z, W, P, R;
        public double J1, J2, J3, J4, J5, J6;

        public lsPathPoint(int uf, int ut, string config, double x, double y, double z, double w, double p, double r, double j1,  double j2, double j3, double j4, double j5, double j6)
        {
            UF = uf;
            UT = ut;
            CONFIG = config;
            X = x;
            Y = y;
            Z = z;
            W = w;
            P = p;
            R = r;
            J1 = j1;
            J2 = j2;
            J3 = j3;
            J4 = j4;
            J5 = j5;
            J6 = j6;
        }
        public lsPathPoint(lsPathPoint lpp)
        {
            UF = lpp.UF;
            UT = lpp.UT;
            CONFIG = lpp.CONFIG;
            X = lpp.X;
            Y = lpp.Y;
            Z = lpp.Z;
            W = lpp.W;
            P = lpp.P;
            R = lpp.R;
            J1 = lpp.J1;
            J2 = lpp.J2;
            J3 = lpp.J3;
            J4 = lpp.J4;
            J5 = lpp.J5;
            J6 = lpp.J6;
        }
        public lsPathPoint()
        {
            UF = 1;
            UT = 1;
            CONFIG = "";
            X = 0d;
            Y = 0d;
            Z = 0d;
            W = 0d;
            P = 0d;
            R = 0d;
            J1 = 0d;
            J2 = 0d;
            J3 = 0d;
            J4 = 0d;
            J5 = 0d;
            J6 = 0d;
        }
    }
}
