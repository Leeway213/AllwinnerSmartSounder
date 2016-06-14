using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSounder.Model
{
    public interface IMainDataService
    {
        Task<MainData> GetMainData();
    }
}
