using System;
using System.Reflection;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS0618 // Type or member is obsolete
namespace MSCLoader
{
    public abstract class Mod
	{
        internal bool disabled = false;
        public virtual bool isDisabled { get => disabled; internal set { disabled = value;  modListElement.SetModEnabled(!value); } }

        public abstract string ID { get; }
        public virtual string Name => ID;
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual string Description { get; } = "";
        public virtual byte[] Icon { get; set; } = null;
        public virtual string IconName { get; set; } = "";

        public ModListElement modListElement;
        public ModSettings modSettings;

        public virtual void ModSettings() { }
        public virtual void ModSettingsLoaded() { }

        public virtual void OnMenuLoad() { }
        public virtual void MenuUpdate() { }
        public virtual void MenuOnGUI() { }
        public virtual void MenuFixedUpdate() { }

        public virtual void OnNewGame() { }
        public virtual void PreLoad() { }
        public virtual void OnLoad() { }
        public virtual void PostLoad() { SecondPassOnLoad(); }
        public virtual void OnSave() { }

        public virtual void OnGUI() { }
		public virtual void Update() { }
        public virtual void FixedUpdate() { }

        // LEGACY
        [Obsolete("Deprecated, not needed.")]
        public virtual bool UseAssetsFolder => false;
        [Obsolete("Deprecated, not needed.")]
        public virtual bool LoadInMenu => false;
        [Obsolete("Deprecated, not needed.")]
        public virtual bool SecondPass => false;
        [Obsolete("Deprecated, use PostLoad() instead.")]
        public virtual void SecondPassOnLoad() { }

        bool update = false;
        internal virtual bool hasUpdate { get => update;  set => update = value; }

        string compiledVer = null;
        internal virtual string compiledVersion { get => compiledVer;  set => compiledVer = value; }

        string filePath = null;
        internal virtual string fileName { get => filePath;  set => filePath = value; }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore IDE1006 // Naming Styles
