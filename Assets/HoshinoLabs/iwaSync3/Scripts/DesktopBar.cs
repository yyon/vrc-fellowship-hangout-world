using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoshinoLabs.IwaSync3
{
    [HelpURL("https://docs.google.com/document/d/1AOMawwq9suEgfa0iLCUX4MRhOiSLBNCLvPCnqW9yQ3g/edit#heading=h.5m3zcl191m82")]
    public class DesktopBar : IwaSync3Base
    {
#pragma warning disable CS0414
        [SerializeField]
        IwaSync3 iwaSync3;

        [SerializeField]
        bool desktopOnly = true;
#pragma warning restore CS0414
    }
}
