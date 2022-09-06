using FixMath.NET;
using Godot;

namespace Volatile.GodotEngine
{
    // Tool tag is required to make VolatileWorld discoverable in the editor by VolatileBody,
    // which checks if it's a child of a VolatileWorld when generating configuration warnings.
    [Tool]
    public class VolatileWorld : VoltNode2D
    {
        [Export]
        public int HistoryLength = 0;
        [Export]
        public bool ProcessSelf { get; set; } = true;
        [Export]
        public bool DebugDraw { get; set; } = false;
        public VoltWorld World { get; private set; }

        public override void _EnterTree()
        {
            base._EnterTree();

            if (Engine.EditorHint)
            {
                SetPhysicsProcess(false);
                return;
            }
            World = new VoltWorld(HistoryLength);
            SetPhysicsProcess(ProcessSelf);
            World.DeltaTime = Fix64.One / (Fix64)Engine.IterationsPerSecond;
        }

        public override void _Ready()
        {
            base._Ready();

            if (Engine.EditorHint)
            {
                SetPhysicsProcess(false);
                return;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            World.Update();
            Update();
        }

        public override void _Draw()
        {
            base._Draw();
            if (Engine.EditorHint || !DebugDraw) return;
            var color = Palette.Accent;
            foreach (var body in World.Bodies)
            {
                DrawRect(new Rect2(body.AABB.BottomLeft.ToGDVector2(), body.AABB.Size.ToGDVector2()), color, false);
                for (int i = 0; i < body.shapeCount; i++)
                {
                    var AABB = body.shapes[i].AABB;
                    DrawRect(new Rect2(AABB.BottomLeft.ToGDVector2(), AABB.Size.ToGDVector2()), color, false);
                }
            }
        }
    }
}
