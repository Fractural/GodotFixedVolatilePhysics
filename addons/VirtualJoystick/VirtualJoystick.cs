using Fractural;
using Godot;
using System;

namespace VirtualJoystickAddon
{
    public class VirtualJoystick : GDScriptWrapper
    {
        public Vector2 Output => (Vector2)Source.Call("get_output");

        public VirtualJoystick() { }
        public VirtualJoystick(Godot.Object source) : base(source) { }
    }
}