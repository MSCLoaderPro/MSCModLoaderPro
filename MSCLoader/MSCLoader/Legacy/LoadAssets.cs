using System;
using System.Collections;
using System.IO;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    public static class LoadAssets
    {
        public static void MakeGameObjectPickable(GameObject go)
        {
            go.layer = LayerMask.NameToLayer("Parts");
            go.tag = "PART";
        }

        public static Texture2D LoadTexture(Mod mod, string fileName, bool normalMap = false)
        {
            string fn = Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName);

            if (!File.Exists(fn))
            {
                throw new FileNotFoundException(string.Format("<b>LoadTexture() Error:</b> File not found: {0}{1}", fn, Environment.NewLine), fn);
            }
            string ext = Path.GetExtension(fn).ToLower();
            if (ext == ".png" || ext == ".jpg")
            {
                Texture2D t2d = new Texture2D(1, 1);
                t2d.LoadImage(File.ReadAllBytes(fn));
                if (normalMap) SetNormalMap(ref t2d);

                return t2d;
            }
            else if (ext == ".dds")
            {
                Texture2D returnTex = LoadDDSManual(fn);
                if (normalMap) SetNormalMap(ref returnTex);

                return returnTex;
            }
            else if (ext == ".tga")
            {
                Texture2D returnTex = LoadTGA(fn);

                if (normalMap) SetNormalMap(ref returnTex);

                return returnTex;
            }
            else throw new NotSupportedException(string.Format("<b>LoadTexture() Error:</b> Texture not supported: {0}{1}", fileName, Environment.NewLine));
        }

        public static AssetBundle LoadBundle(Mod mod, string bundleName)
        {
            string bundle = Path.Combine(ModLoader.GetModAssetsFolder(mod), bundleName);
            if(File.Exists(bundle))
            {
                try { ModConsole.Log(string.Format("Loading Asset: {0}...", bundleName)); } catch { } 
                return AssetBundle.CreateFromMemoryImmediate(File.ReadAllBytes(bundle));
            }
            else throw new FileNotFoundException(string.Format("<b>LoadBundle() Error:</b> File not found: <b>{0}</b>{1}", bundleName, Environment.NewLine), bundleName);
        }

        static Texture2D LoadDDSManual(string ddsPath)
        {
            try
            {
                byte[] ddsBytes = File.ReadAllBytes(ddsPath);

                byte ddsSizeCheck = ddsBytes[4];
                if (ddsSizeCheck != 124) throw new Exception("Invalid DDS DXTn texture. Unable to read");

                int height = ddsBytes[13] * 256 + ddsBytes[12];
                int width = ddsBytes[17] * 256 + ddsBytes[16];

                byte DXTType = ddsBytes[87];

                TextureFormat textureFormat = DXTType == 49 ? TextureFormat.DXT1 : TextureFormat.DXT5;

                int DDS_HEADER_SIZE = 128;
                byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
                Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

                Texture2D texture = new Texture2D(width, height, textureFormat, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
                texture.name = new FileInfo(ddsPath).Name;

                return (texture);
            }
            catch (Exception ex)
            {
                ModConsole.LogError(string.Format("<b>LoadTexture() Error:</b>{0}Error: Could not load DDS texture", Environment.NewLine,ex.Message));
                System.Console.WriteLine(ex);
                return new Texture2D(8, 8);
            }
        }

        static void SetNormalMap(ref Texture2D tex)
        {
            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color temp = pixels[i];
                temp.r = pixels[i].g;
                temp.a = pixels[i].r;
                pixels[i] = temp;
            }
            tex.SetPixels(pixels);
        }

        static Texture2D LoadTGA(string fileName)
        {
            using (FileStream imageFile = File.OpenRead(fileName))
            {
                using (BinaryReader r = new BinaryReader(imageFile))
                {
                    r.BaseStream.Seek(12, SeekOrigin.Begin);

                    short width = r.ReadInt16();
                    short height = r.ReadInt16();
                    int bitDepth = r.ReadByte();
                    r.BaseStream.Seek(1, SeekOrigin.Current);

                    Texture2D tex = new Texture2D(width, height);
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
                    else throw new Exception(string.Format("<b>LoadTexture() Error:</b> TGA texture is not 32 or 24 bit depth.{0}", Environment.NewLine));

                    tex.SetPixels32(pulledColors);
                    tex.Apply();

                    return tex;
                }
            }
        }
    }
}