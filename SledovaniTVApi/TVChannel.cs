﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SledovaniTVAPI
{
    public class TVChannel : JSONObject
    {
       public string Name { get; set;  }
       public string Url { get; set; }
       public string Id { get; set; }
       public string Type { get; set; }
       public string LogoUrl { get; set; }
       public string Locked { get; set; }
       public string ParentLocked { get; set; }
       public string Group { get; set; }
    }
}