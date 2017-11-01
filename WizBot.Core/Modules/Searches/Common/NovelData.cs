using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Modules.Searches.Common
{
    public class NovelData
    {
        public string Description { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string[] Authors { get; set; }
        public string Status { get; set; }
        public string[] Genres { get; set; }
        public string Score { get; set; }
    }
}