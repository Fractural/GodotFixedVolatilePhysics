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
        }
    }
}
