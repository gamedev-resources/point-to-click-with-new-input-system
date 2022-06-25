using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public static class GeneralExtensions
{
    public static Vector3 WithNewY(this Vector3 vector, float newY) => new Vector3(vector.x, newY, vector.z);
}
