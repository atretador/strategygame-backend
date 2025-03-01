using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Dto
{
    public class WorldDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool isLive { get; set; }
        public bool isOpen { get; set; }
        public SimpleWorldSettingsDto SimpleWorldSettings { get; set; } = new SimpleWorldSettingsDto();
    }
}