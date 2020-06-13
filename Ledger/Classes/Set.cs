using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ledger.Classes
{
    public class Set
    {
        public string Object { get; set; }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Scryfall_uri { get; set; }
        public string Search_uri { get; set; }
        public string Released_at { get; set; }
        public string Set_type { get; set; }
        public int Card_count { get; set; }
        public string Parent_set_code { get; set; }
        public bool Digital { get; set; }
        public bool Foil_only { get; set; }
        public string Icon_svg_uri { get; set; }
    }

    public class SetList
    {
        public string Object { get; set; }
        public bool Has_more { get; set; }
        public List<Set> Data { get; set; }
    }
}
