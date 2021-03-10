using MSCLoader.Helper;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated, use ModAssets instead.")]
    public static class LoadAssets
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use the extension method gameObject.MakePickable() instead.")]
        public static void MakeGameObjectPickable(GameObject go) => go.MakePickable();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use ModAssets.LoadTexture() instead.")]
        public static Texture2D LoadTexture(Mod mod, string fileName, bool normalMap = false) =>
            ModAssets.LoadTexture(Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName), normalMap);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use ModAssets.LoadBundle() instead.")]
        public static AssetBundle LoadBundle(Mod mod, string bundleName) =>
            ModAssets.LoadBundle(mod, bundleName);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("LoadOBJ is deprecated, please use AssetBundles instead or ModAssets.LoadMeshOBJ().")]
        public static GameObject LoadOBJ(Mod mod, string fileName, bool collider = true, bool rigidbody = false)
        {
            Mesh mesh = ModAssets.LoadMeshOBJ(Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName));
            if (mesh != null)
            {
                GameObject obj = new GameObject();
                obj.AddComponent<MeshFilter>().mesh = mesh;
                obj.AddComponent<MeshRenderer>();
                if (rigidbody)
                    obj.AddComponent<Rigidbody>();
                if (collider)
                {
                    if (rigidbody)
                        obj.AddComponent<MeshCollider>().convex = true;
                    else
                        obj.AddComponent<MeshCollider>();
                }
                return obj;
            }
            else
                return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("LoadOBJMesh is deprecated, please use AssetBundles instead.")]
        public static Mesh LoadOBJMesh(Mod mod, string fileName) =>
            ModAssets.LoadMeshOBJ(Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName));
    }
}