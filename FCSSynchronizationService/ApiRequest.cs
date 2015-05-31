using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCSSynchronizationService
{
   class ApiRequest
   {
      public string master_id { get; set; }
      public string api_key { get; set; }
      public List<Dictionary<string, object>> batch { get; set; }
   }
}
