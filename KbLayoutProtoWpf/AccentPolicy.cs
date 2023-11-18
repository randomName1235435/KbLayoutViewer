﻿using System.Runtime.InteropServices;

namespace KbLayoutProtoWpf;

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public int AccentFlags;
    public int GradientColor;
    public int AnimationId;
}