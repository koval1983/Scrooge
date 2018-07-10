using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrooge
{
    class Couple
    {
        int index;

        List<Network> children;

        Network n1, n2;

        public Couple(Network n1, Network n2, int index, List<Network> children)
        {
            this.n1 = n1;
            this.n2 = n2;
            this.index = index;
            this.children = children;
        }

        public void Cross()
        {
            children.Add(n1.Cross(n2));
        }
    }
}
