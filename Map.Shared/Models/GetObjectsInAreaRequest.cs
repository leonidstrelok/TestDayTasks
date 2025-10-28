﻿using MemoryPack;

namespace Map.Shared.Models;

[MemoryPackable]
public partial class GetObjectsInAreaRequest
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
}