using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSounder.Model
{
    public class MainDataService : IMainDataService
    {
        public Task<MainData> GetMainData()
        {
            MainData data = new MainData("无状态");
            return Task.FromResult(data);
        }
    }
}
