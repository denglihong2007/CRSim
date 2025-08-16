using CRSim.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRSim.Core.Abstractions
{
    public interface ISettingsService
    {
        void LoadSettings();
        void SaveSettings();
        Settings GetSettings();
        public bool HasShownWelcome { get; set; }
    }
}
