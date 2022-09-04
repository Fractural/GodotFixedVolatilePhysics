using Godot;
using System;

namespace Tests
{
    public class FPSCounter : Label
    {
        public override void _Process(float delta)
        {
            Text = Engine.GetFramesPerSecond() + " FPS";
        }
    }
}