using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberErgoGo
{
    /// <summary>
    /// Stores all settings. It's a singleton to store the settings global.
    /// </summary>
    class SettingsManager
    {
        private static SettingsManager instance;

        /// <summary>
        /// Private constructor to make sure there is just one instance.
        /// </summary>
        private SettingsManager() 
        { 
        
        }

        /// <summary>
        /// The access to the instance via this method.
        /// </summary>
        public static SettingsManager getInstance()
        {
            if (instance == null)
            {
                instance = new SettingsManager();
            }
            return instance;
        }


    }
}
