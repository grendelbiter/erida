﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wyUpdate.Common.Custom;

public static class GameInfo
{
    private static ClientFile clientFile = new ClientFile();
	

    public static string Version()
    {
        clientFile.OpenClientFile(@"client.wyc");
        return clientFile.InstalledVersion;
    }

    public static string CompanyName()
    {
        clientFile.OpenClientFile(@"client.wyc");
        return clientFile.CompanyName;
    }

    public static string ProductName()
    {
        clientFile.OpenClientFile(@"client.wyc");
        return clientFile.ProductName;
    }
}