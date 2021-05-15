using System;
using System.IO;
using UnityEngine;

namespace MSCLoader
{
    /// <summary>Holder class for all things related to asset loading.</summary>
    public static class ModAssets
    {
        /// <summary>Loads an AssetBundle from a specified byte Array</summary>
        /// <param name="bundleBytes">byte array to load.</param>
        /// <returns>Loaded AssetBundle</returns>
        public static AssetBundle LoadBundle(byte[] bundleBytes) =>
            AssetBundle.CreateFromMemoryImmediate(bundleBytes);
        /// <summary>Loads an AssetBundle from a specified path.</summary>
        /// <param name="filePath">File path to bundle.</param>
        /// <returns>Loaded AssetBundle</returns>
        public static AssetBundle LoadBundle(string filePath)
        {
            if (File.Exists(filePath)) return AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(filePath)); 
            else throw new FileNotFoundException($"<b>LoadBundle() Error:</b> No AssetBundle file found at path: {filePath}");
        }
        /// <summary>Loads an AssetBundle from the mod's Asset folder by name</summary>
        /// <param name="mod">Mod to load the AssetBundle from.</param>
        /// <param name="bundleName">File name of the bundle.</param>
        /// <returns>Loaded AssetBundle</returns>
        public static AssetBundle LoadBundle(Mod mod, string bundleName)
        {
            ModConsole.Log($"Loading AssetBundle: {bundleName}..");
            return LoadBundle(Path.Combine(ModLoader.GetModAssetsFolder(mod), bundleName));
        }
        /// <summary>Loads a Texture2D at the specified path, supported types: .jpg, .png, .dds, .tga</summary>
        /// <param name="filePath">Path to file.</param>
        /// <param name="normalMap">(Optional) Should it be converted into a normal map?</param>
        /// <returns>Loaded Texture2D</returns>
        public static Texture2D LoadTexture(string filePath, bool normalMap = false)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"<b>LoadTexture() Error:</b> File not found: {filePath}", filePath);

            string fileExtension = Path.GetExtension(filePath).ToLower();

            switch (fileExtension)
            {
                case ".jpg": return LoadTextureJPG(filePath, normalMap);
                case ".png": return LoadTexturePNG(filePath, normalMap);
                case ".dds": return LoadTextureDDS(filePath, normalMap);
                case ".tga": return LoadTextureTGA(filePath, normalMap);
                default: throw new NotSupportedException($"<b>LoadTexture() Error:</b> File {fileExtension} not supported as a texture: {filePath}");
            }
        }
        /// <summary>Loads a Texture2D at the specified path, supported types: .jpg, .png, .dds, .tga</summary>
        /// <param name="mod">Mod which asset folder to look in.</param>
        /// <param name="textureName">Name of the texture.</param>
        /// <param name="normalMap">(Optional) Should it be converted into a normal map?</param>
        /// <returns>Loaded Texture2D</returns>
        public static Texture2D LoadTexture(Mod mod, string textureName, bool normalMap = false)
        {
            return LoadTexture(Path.Combine(ModLoader.GetModAssetsFolder(mod), textureName), normalMap);
        }
        /// <summary>Loads a PNG image as a Texture2D.</summary>
        /// <param name="filePath">Path to image file.</param>
        /// <param name="normalMap">Is it a normal map?</param>
        /// <returns>Loaded Texture2D.</returns>
        public static Texture2D LoadTexturePNG(string filePath, bool normalMap = false)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(filePath));
            return normalMap ? texture.ConvertToNormalMap() : texture;
        }
        /// <summary>Loads a JPG image as a Texture2D.</summary>
        /// <param name="filePath">Path to image file.</param>
        /// <param name="normalMap">Is it a normal map?</param>
        /// <returns>Loaded Texture2D.</returns>
        public static Texture2D LoadTextureJPG(string filePath, bool normalMap = false) =>
            LoadTexturePNG(filePath, normalMap);
        /// <summary>Loads a DDS image as a Texture2D.</summary>
        /// <param name="filePath">Path to image file.</param>
        /// <param name="normalMap">Is it a normal map?</param>
        /// <returns>Loaded Texture2D.</returns>
        public static Texture2D LoadTextureDDS(string filePath, bool normalMap = false)
        {
            // jeff-smith http://answers.unity.com/answers/707772/view.html
            // AARO4130 https://github.com/helemaalbigt/DesignSpace/blob/master/Assets/OBJImport/TextureLoader.cs
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                if (fileBytes[4] != 124) throw new Exception("Invalid DDS texture. Can't read.");

                byte DXTType = fileBytes[87];
                TextureFormat textureFormat = TextureFormat.DXT5;

                if (DXTType == 49) textureFormat = TextureFormat.DXT1;
                else if (DXTType == 53) textureFormat = TextureFormat.DXT5;
                else throw new Exception("Unsupported Texture Format. Can't load texture. Only DXT1(BC1) and DXT5(BC3) Supported.");

                int headerSize = 128;
                byte[] dxtBytes = new byte[fileBytes.Length - headerSize];
                Buffer.BlockCopy(fileBytes, headerSize, dxtBytes, 0, fileBytes.Length - headerSize);

                int height = fileBytes[13] * 256 + fileBytes[12];
                int width = fileBytes[17] * 256 + fileBytes[16];

                Texture2D texture = new Texture2D(width, height, textureFormat, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
                texture.name = Path.GetFileName(filePath);

                return normalMap ? texture.ConvertToNormalMap() : texture;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                ModConsole.LogError("LoadTexture(): Can't load dds:" + filePath + "\n" + ex);
                return new Texture2D(8, 8);
            }
        }
        /// <summary>Loads a TGA image as a Texture2D.</summary>
        /// <param name="filePath">Path to image file.</param>
        /// <param name="normalMap">Is it a normal map?</param>
        /// <returns>Loaded Texture2D.</returns>
        public static Texture2D LoadTextureTGA(string filePath, bool normalMap = false)
        {
            // ALLCAPS https://forum.unity.com/threads/tga-loader-for-unity3d.172291/#post-1587475
            using (var imageFile = File.OpenRead(filePath))
            {
                using (BinaryReader r = new BinaryReader(imageFile))
                {
                    // Skip some header info we don't care about.
                    // Even if we did care, we have to move the stream seek point to the beginning,
                    // as the previous method in the workflow left it at the end.
                    r.BaseStream.Seek(12, SeekOrigin.Begin);

                    short width = r.ReadInt16();
                    short height = r.ReadInt16();
                    int bitDepth = r.ReadByte();

                    // Skip a byte of header information we don't care about.
                    r.BaseStream.Seek(1, SeekOrigin.Current);

                    Texture2D texture = new Texture2D(width, height);
                    Color32[] pulledColors = new Color32[width * height];

                    if (bitDepth == 32)
                    {
                        for (int i = 0; i < width * height; i++)
                        {
                            byte red = r.ReadByte();
                            byte green = r.ReadByte();
                            byte blue = r.ReadByte();
                            byte alpha = r.ReadByte();

                            pulledColors[i] = new Color32(blue, green, red, alpha);
                        }
                    }
                    else if (bitDepth == 24)
                    {
                        for (int i = 0; i < width * height; i++)
                        {
                            byte red = r.ReadByte();
                            byte green = r.ReadByte();
                            byte blue = r.ReadByte();

                            pulledColors[i] = new Color32(blue, green, red, 1);
                        }
                    }
                    else
                    {
                        throw new Exception($"LoadTexture() Error: TGA texture had non 32/24 bit depth.\n{filePath}");
                    }

                    texture.SetPixels32(pulledColors);
                    texture.Apply();

                    return normalMap ? texture.ConvertToNormalMap() : texture;
                }
            }
        }
        /// <summary>Convert a texture to a normal map.</summary>
        /// <param name="texture">Texture2D to convert</param>
        /// <param name="mipMaps">Generate mipmaps?</param>
        /// <returns>Converted Texture2D</returns>
        public static Texture2D ConvertToNormalMap(this Texture2D texture, bool mipMaps = true)
        {
            // Bunny83 http://answers.unity.com/comments/1195008/view.html
            Texture2D normalMap = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, mipMaps);

            Color32[] colors = texture.GetPixels32();
            for (int i = 0; i < colors.Length; i++)
            {
                Color32 c = colors[i];
                c.a = c.r;
                c.r = c.b = c.g;
                colors[i] = c;
            }
            normalMap.SetPixels32(colors);
            normalMap.Apply();

            return normalMap;
        }
        /// <summary>Loads an OBJ model file as a Mesh.</summary>
        /// <param name="filePath">Path to model file.</param>
        /// <returns>Loaded Mesh.</returns>
        public static Mesh LoadMeshOBJ(string filePath)
        {
            return ObjImporter.LoadMesh(filePath);
        }
    }
}
