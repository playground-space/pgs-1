using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class SomeGrain : Grain
    {
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }

        public SomeGrain(IGrainContext grainContext, IGrainRuntime grainRuntime = null) : base(grainContext, grainRuntime)
        {
            
        }
    }
}
