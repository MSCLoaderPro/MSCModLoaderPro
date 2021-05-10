using System;
using System.ComponentModel;

#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    /// <summary> Main Mod Class, parent class for all mods. </summary>
    public abstract class Mod
	{
        internal bool enabled = true;
        internal ModUpdateData ModUpdateData;

        /// <summary>Determines whether or not the mod is enabled.</summary>
        public virtual bool Enabled { get => enabled; set { modListElement.SetModEnabled(value); } }
        /// <summary>The mod's ID, used for identification. Has to be unique!</summary>
        public abstract string ID { get; }
        /// <summary>The mod's name, shown in lists etc.</summary>
        public virtual string Name => ID;
        /// <summary>Who made the mod? You, presumably!</summary>
        public abstract string Author { get; }
        /// <summary>Contains the mod version.</summary>
        public abstract string Version { get; }
        /// <summary>A short description of your mod. Displayed in the settings window for the mod, hidden if empty.</summary>
        public virtual string Description { get; set; } = "";
        /// <summary>Icon displayed in the mod list, preferably square and not larger than 256x256.</summary>
        public virtual byte[] Icon { get; set; } = null;
        /// <summary>A link from which ModLoader will check for updates. Must be GitHub or NexusMods, eg. https://github.com/Athlon007/MOP </summary>
        public virtual string UpdateLink { get; internal set; } = "";

        /// <summary> The mod list element for the mod. </summary>
        public ModListElement modListElement;
        /// <summary> The settings container for the mod. Used when adding settings. </summary>
        public ModSettings modSettings;

        /// <summary> Method for adding settings to the mod. Order of execution: 1 </summary>
        public virtual void ModSettings() { }
        /// <summary> Method called when all mods have had their ModSettings() called. Order of execution: 2 </summary>
        public virtual void ModSettingsLoaded() { }

        /// <summary> Load Method for anything involving the menu scene. Order of execution: 3 </summary>
        public virtual void MenuOnLoad() { OnMenuLoad(); }
        /// <summary> OnGUI Method for the menu scene. Order of execution: Every frame in menu </summary>
        public virtual void MenuOnGUI() { }
        /// <summary> Update Method for the menu scene. Order of execution: Every fixed time step </summary>
        public virtual void MenuUpdate() { }
        /// <summary> FixedUpdate Method for the menu scene. Order of execution: Every GUI frame </summary>
        public virtual void MenuFixedUpdate() { }

        /// <summary> Method executed when the player starts a new game, use cases include removing old save files. Order of execution: 4 </summary>
        public virtual void OnNewGame() { }
        /// <summary> Method executed one frame after the game scene loads. Order of execution: 5 </summary>
        public virtual void PreLoad() { }
        /// <summary> Method executed just when the game has completely finished loading. Order of execution: 6 </summary>
        public virtual void OnLoad() { }
        /// <summary> Method executed after every mod has executed OnLoad(). Order of execution: 7 </summary>
        public virtual void PostLoad() { SecondPassOnLoad(); }
        /// <summary> Method executed when the player saves the game. Order of execution: 8 </summary>
        public virtual void OnSave() { }

        /// <summary> OnGUI method for the game scene. Order of execution: Every GUI frame </summary>
        public virtual void OnGUI() { }
        /// <summary> Update method for the game scene. Order of execution: Every frame </summary>
		public virtual void Update() { }
        /// <summary> FixedUpdate method for the game scene. Order of execution: Every fixed time step </summary>
        public virtual void FixedUpdate() { }

        /// <summary>Method called whenever the mod is enabled.</summary>
        public virtual void OnModEnabled() { }

        /// <summary>Method called whenever the mod is enabled.</summary>
        public virtual void OnModDisabled() { }
        
        /// <summary>Method called whenever mod settings are open.</summary>
        public virtual void ModSettingsOpen() { }

        /// <summary>Method called whenever mod settings are closed.</summary>
        public virtual void ModSettingsClose() { }

        #region Obsolete Methods
        [Obsolete("Deprecated, not needed."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool UseAssetsFolder => false;
        [Obsolete("Deprecated, not needed."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool LoadInMenu => false;
        [Obsolete("Deprecated, not needed."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool SecondPass => false;
        [Obsolete("Deprecated, use PostLoad() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void SecondPassOnLoad() { }
        [Obsolete("Deprecated, use MenuOnLoad() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnMenuLoad() { }

        internal bool disabled = false;
        [Obsolete("Deprecated, use Enabled instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool isDisabled { get => !enabled; internal set { enabled = !value; modListElement.SetModEnabled(!value); } }

        bool update = false;
        internal virtual bool hasUpdate { get => update;  set => update = value; }

        string compiledVer = null;
        internal virtual string compiledVersion { get => compiledVer;  set => compiledVer = value; }

        string filePath = null;
        internal virtual string fileName { get => filePath;  set => filePath = value; }
        #endregion
    }
}
