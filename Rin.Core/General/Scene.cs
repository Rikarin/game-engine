using Arch.Core;
using Arch.System;
using Rin.Core.Systems;
using LocalToWorldSystem = Rin.Core.Systems.LocalToWorldSystem;
using ParentSystem = Rin.Core.Systems.ParentSystem;

namespace Rin.Core.General;

public class Scene {
    public string Name { get; }
    // public bool IsDirty { get; }
    // public bool IsLoaded { get; }

    /// <summary>
    ///     Relative path of the scene. "Assets/Scenes/Scene01.rin"
    /// </summary>
    public string Path { get; }

    // ECS
    public World World { get; }
    public Group<float> Systems { get; }

    internal Scene(string name, string path) : this(name, path, World.Create()) { }

    internal Scene(string name, string path, World world) {
        Name = name;
        Path = path;
        World = world;

        // TODO: Load this
        Systems = new(
            new ParentSystem(World),
            new LocalToWorldSystem(World),
            new ScriptSystem(World)
        );
    }

    // public void Save() {
    //     SceneManager.SaveScene(this);
    // }
}
