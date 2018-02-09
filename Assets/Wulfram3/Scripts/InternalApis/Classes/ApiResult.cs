using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiResult<T>
{
    public string message { get; set; }

    public T Result { get; set; }
}
