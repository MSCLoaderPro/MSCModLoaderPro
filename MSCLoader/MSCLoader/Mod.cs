using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
namespace MSCLoader
{
    public abstract class Mod
	{
        bool disabled = false;
        public virtual bool isDisabled { get => disabled; internal set => disabled = value; }

        bool update = false;
        internal virtual bool hasUpdate { get => update;  set => update = value; }

        string compiledVer = null;
        internal virtual string compiledVersion { get => compiledVer;  set => compiledVer = value; }

        string filePath = null;
        internal virtual string fileName { get => filePath;  set => filePath = value; }

        public abstract string ID { get; }
        public virtual string Name => ID;
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual string Description { get; } = "";
        public virtual byte[] Icon { get; set; } = null;

        public ModListElement modListElement;
        public ModSettings modSettings;

        public virtual void ModSettings() { }
        public virtual void ModSettingsLoaded() { }
        public virtual void OnMenuLoad() { }

        public virtual void OnNewGame() { }
        public virtual void PreLoad() { }
        public virtual void OnLoad() { }
        public virtual void PostLoad() { SecondPassOnLoad(); }
        public virtual void OnSave() { }

        public virtual void OnGUI() { }
		public virtual void Update() { }
        public virtual void FixedUpdate() { }

        // LEGACY
        public virtual bool UseAssetsFolder => false;
        public virtual bool LoadInMenu => false;
        public virtual bool SecondPass => false;
        public virtual void SecondPassOnLoad() { }
    }
}
#pragma warning restore IDE1006 // Naming Styles
