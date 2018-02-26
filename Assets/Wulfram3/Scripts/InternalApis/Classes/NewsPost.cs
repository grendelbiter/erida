using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsPost 
{
    public string _id { get; set; }

    public string Title { get; set; }

    public DateTime PostDate { get; set; }

    public string Author { get; set; }

    public string Content { get; set; }

    public string Url { get { return "http://www.wulfrida.com/news/" + _id; } }
}
