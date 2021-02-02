using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSCLoader
{
#pragma warning disable CS1591
    public class OBJLoader
    {
        private struct meshStruct
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uv;
            //public Vector2[] uv1;
            //public Vector2[] uv2;
            public int[] triangles;
            //public int[] faceVerts;
            //public int[] faceUVs;
            public Vector3[] faceData;
            //public string name;
            public string fileName;
        }

        // Use this for initialization
        public Mesh ImportFile(string filePath)
        {
            meshStruct newMesh = createMeshStruct(filePath);
            populateMeshStruct(ref newMesh);

            Vector3[] newVerts = new Vector3[newMesh.faceData.Length];
            Vector2[] newUVs = new Vector2[newMesh.faceData.Length];
            Vector3[] newNormals = new Vector3[newMesh.faceData.Length];
            int i = 0;
            /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
             * for the appropriate Unity mesh array.
             */
            foreach (Vector3 v in newMesh.faceData)
            {
                newVerts[i] = newMesh.vertices[(int)v.x - 1];
                if (v.y >= 1)
                    newUVs[i] = newMesh.uv[(int)v.y - 1];

                if (v.z >= 1)
                    newNormals[i] = newMesh.normals[(int)v.z - 1];
                i++;
            }

            Mesh mesh = new Mesh();

            mesh.vertices = newVerts;
            mesh.uv = newUVs;
            mesh.normals = newNormals;
            mesh.triangles = newMesh.triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }

        private static meshStruct createMeshStruct(string filename)
        {
            int triangles = 0;
            int vertices = 0;
            int vt = 0;
            int vn = 0;
            int face = 0;
            meshStruct mesh = new meshStruct();
            mesh.fileName = filename;
            StreamReader stream = File.OpenText(filename);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();
                char[] splitIdentifier = { ' ' };
                string[] brokenString;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                        && !currentText.StartsWith("vn "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();                           //Trim the current line
                        brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                        switch (brokenString[0])
                        {
                            case "v":
                                vertices++;
                                break;
                            case "vt":
                                vt++;
                                break;
                            case "vn":
                                vn++;
                                break;
                            case "f":
                                face = face + brokenString.Length - 1;
                                triangles = triangles + 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertice, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                }
            }
            mesh.triangles = new int[triangles];
            mesh.vertices = new Vector3[vertices];
            mesh.uv = new Vector2[vt];
            mesh.normals = new Vector3[vn];
            mesh.faceData = new Vector3[face];
            return mesh;
        }

        private static void populateMeshStruct(ref meshStruct mesh)
        {
            StreamReader stream = File.OpenText(mesh.fileName);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();

                char[] splitIdentifier = { ' ' };
                char[] splitIdentifier2 = { '/' };
                string[] brokenString;
                string[] brokenBrokenString;
                int f = 0;
                int f2 = 0;
                int v = 0;
                int vn = 0;
                int vt = 0;
                int vt1 = 0;
                int vt2 = 0;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                        !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                        !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                        !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();
                        brokenString = currentText.Split(splitIdentifier, 50);
                        switch (brokenString[0])
                        {
                            case "g":
                                break;
                            case "usemtl":
                                break;
                            case "usemap":
                                break;
                            case "mtllib":
                                break;
                            case "v":
                                mesh.vertices[v] = new Vector3(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]),
                                                         Convert.ToSingle(brokenString[3]));
                                v++;
                                break;
                            case "vt":
                                mesh.uv[vt] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                vt++;
                                break;
                            case "vt1":
                                mesh.uv[vt1] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                vt1++;
                                break;
                            case "vt2":
                                mesh.uv[vt2] = new Vector2(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]));
                                vt2++;
                                break;
                            case "vn":
                                mesh.normals[vn] = new Vector3(Convert.ToSingle(brokenString[1]), Convert.ToSingle(brokenString[2]),
                                                        Convert.ToSingle(brokenString[3]));
                                vn++;
                                break;
                            case "vc":
                                break;
                            case "f":

                                int j = 1;
                                List<int> intArray = new List<int>();
                                while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                {
                                    Vector3 temp = new Vector3();
                                    brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                                    temp.x = Convert.ToInt32(brokenBrokenString[0]);
                                    if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                    {
                                        if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                        {
                                            temp.y = Convert.ToInt32(brokenBrokenString[1]);
                                        }
                                        temp.z = Convert.ToInt32(brokenBrokenString[2]);
                                    }
                                    j++;

                                    mesh.faceData[f2] = temp;
                                    intArray.Add(f2);
                                    f2++;
                                }
                                j = 1;
                                while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                                {
                                    mesh.triangles[f] = intArray[0];
                                    f++;
                                    mesh.triangles[f] = intArray[j];
                                    f++;
                                    mesh.triangles[f] = intArray[j + 1];
                                    f++;

                                    j++;
                                }
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.
                        }
                    }
                }
            }
        }
    }
 
    /* FastObjImporter.cs
    * by Marc Kusters (Nighteyes)
    * 
    * Used for loading .obj files exported by Blender
    * Example usage: Mesh myMesh = FastObjImporter.Instance.ImportFile("path_to_obj_file.obj");
    */
 
    public sealed class FastObjImporter
    {

        #region singleton
        // Singleton code
        // Static can be called from anywhere without having to make an instance
        private static FastObjImporter _instance;

        // If called check if there is an instance, otherwise create it
        public static FastObjImporter Instance
        {
            get { return _instance ?? (_instance = new FastObjImporter()); }
        }
        #endregion

        private List<int> triangles;
        private List<Vector3> vertices;
        private List<Vector2> uv;
        private List<Vector3> normals;
        private List<Vector3Int> faceData;
        private List<int> intArray;

        private const int MIN_POW_10 = -16;
        private const int MAX_POW_10 = 16;
        private const int NUM_POWS_10 = MAX_POW_10 - MIN_POW_10 + 1;
        private static readonly float[] pow10 = GenerateLookupTable();

        // Use this for initialization
        public Mesh ImportFile(string filePath)
        {
            triangles = new List<int>();
            vertices = new List<Vector3>();
            uv = new List<Vector2>();
            normals = new List<Vector3>();
            faceData = new List<Vector3Int>();
            intArray = new List<int>();

            LoadMeshData(filePath);

            Vector3[] newVerts = new Vector3[faceData.Count];
            Vector2[] newUVs = new Vector2[faceData.Count];
            Vector3[] newNormals = new Vector3[faceData.Count];

            /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
             * for the appropriate Unity mesh array.
             */
            for (int i = 0; i < faceData.Count; i++)
            {
                newVerts[i] = vertices[faceData[i].x - 1];
                if (faceData[i].y >= 1)
                    newUVs[i] = uv[faceData[i].y - 1];

                if (faceData[i].z >= 1)
                    newNormals[i] = normals[faceData[i].z - 1];
            }

            Mesh mesh = new Mesh();

            mesh.vertices = newVerts;
            mesh.uv = newUVs;
            mesh.normals = newNormals;
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }

        private void LoadMeshData(string fileName)
        {

            StringBuilder sb = new StringBuilder();
            string text = File.ReadAllText(fileName);
            int start = 0;
            string objectName = null;
            int faceDataCount = 0;

            StringBuilder sbFloat = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    sb.Remove(0, sb.Length);

                    // Start +1 for whitespace '\n'
                    sb.Append(text, start + 1, i - start);
                    start = i;

                    if (sb[0] == 'o' && sb[1] == ' ')
                    {
                        sbFloat.Remove(0, sbFloat.Length);
                        int j = 2;
                        while (j < sb.Length)
                        {
                            objectName += sb[j];
                            j++;
                        }
                    }
                    else if (sb[0] == 'v' && sb[1] == ' ') // Vertices
                    {
                        int splitStart = 2;

                        vertices.Add(new Vector3(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'v' && sb[1] == 't' && sb[2] == ' ') // UV
                    {
                        int splitStart = 3;

                        uv.Add(new Vector2(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'v' && sb[1] == 'n' && sb[2] == ' ') // Normals
                    {
                        int splitStart = 3;

                        normals.Add(new Vector3(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'f' && sb[1] == ' ')
                    {
                        int splitStart = 2;

                        int j = 1;
                        intArray.Clear();
                        int info = 0;
                        // Add faceData, a face can contain multiple triangles, facedata is stored in following order vert, uv, normal. If uv or normal are / set it to a 0
                        while (splitStart < sb.Length && char.IsDigit(sb[splitStart]))
                        {
                            faceData.Add(new Vector3Int(GetInt(sb, ref splitStart, ref sbFloat),
                                GetInt(sb, ref splitStart, ref sbFloat), GetInt(sb, ref splitStart, ref sbFloat)));
                            j++;

                            intArray.Add(faceDataCount);
                            faceDataCount++;
                        }

                        info += j;
                        j = 1;
                        while (j + 2 < info) //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                        {
                            triangles.Add(intArray[0]);
                            triangles.Add(intArray[j]);
                            triangles.Add(intArray[j + 1]);

                            j++;
                        }
                    }
                }
            }
        }

        private float GetFloat(StringBuilder sb, ref int start, ref StringBuilder sbFloat)
        {
            sbFloat.Remove(0, sbFloat.Length);
            while (start < sb.Length &&
                   (char.IsDigit(sb[start]) || sb[start] == '-' || sb[start] == '.'))
            {
                sbFloat.Append(sb[start]);
                start++;
            }
            start++;

            return ParseFloat(sbFloat);
        }

        private int GetInt(StringBuilder sb, ref int start, ref StringBuilder sbInt)
        {
            sbInt.Remove(0, sbInt.Length);
            while (start < sb.Length &&
                   (char.IsDigit(sb[start])))
            {
                sbInt.Append(sb[start]);
                start++;
            }
            start++;

            return IntParseFast(sbInt);
        }


        private static float[] GenerateLookupTable()
        {
            var result = new float[(-MIN_POW_10 + MAX_POW_10) * 10];
            for (int i = 0; i < result.Length; i++)
                result[i] = (float)((i / NUM_POWS_10) *
                        Mathf.Pow(10, i % NUM_POWS_10 + MIN_POW_10));
            return result;
        }

        private float ParseFloat(StringBuilder value)
        {
            float result = 0;
            bool negate = false;
            int len = value.Length;
            int decimalIndex = value.Length;
            for (int i = len - 1; i >= 0; i--)
                if (value[i] == '.')
                { decimalIndex = i; break; }
            int offset = -MIN_POW_10 + decimalIndex;
            for (int i = 0; i < decimalIndex; i++)
                if (i != decimalIndex && value[i] != '-')
                    result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i - 1];
                else if (value[i] == '-')
                    negate = true;
            for (int i = decimalIndex + 1; i < len; i++)
                if (i != decimalIndex)
                    result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i];
            if (negate)
                result = -result;
            return result;
        }

        private int IntParseFast(StringBuilder value)
        {
            // An optimized int parse method.
            int result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                result = 10 * result + (value[i] - 48);
            }
            return result;
        }
    }

    public sealed class Vector3Int
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public Vector3Int() { }

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}   
#pragma warning restore CS1591