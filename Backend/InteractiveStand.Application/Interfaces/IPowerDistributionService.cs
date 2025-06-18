using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveStand.Application.Interfaces
{
    public interface IPowerDistributionService
    {
        Task DistributePowerAsync(int regionId);
    }
}
