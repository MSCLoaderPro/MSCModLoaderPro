using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using MSCLoader.Helper;
using System;
using static System.Convert;

#pragma warning disable CS1591, IDE0017, IDE1006
namespace MSCLoader
{
    public class ObjImporter
    {
        struct MeshData
        {
            public Vector3[] vertices, normals, faceData;
            public Vector2[] uv;
            public int[] triangles;
            public string fileName;
        }

        // Use this for initialization
        public static Mesh LoadMesh(string path)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            MeshData meshData = LoadMeshData(path);
            CompileMeshData(ref meshData, path);

            Vector3[] newVertices = new Vector3[meshData.faceData.Length];
            Vector2[] newUVs = new Vector2[meshData.faceData.Length];
            Vector3[] newNormals = new Vector3[meshData.faceData.Length];

            // The following loops through the facedata and assigns the appropriate vertex, uv, or normal for the appropriate Unity mesh array.
            for (int i = 0; i < meshData.faceData.Length; i++)
            {
                newVertices[i] = meshData.vertices[(int)meshData.faceData[i].x - 1];
                if (meshData.faceData[i].y >= 1)
                    newUVs[i] = meshData.uv[(int)meshData.faceData[i].y - 1];
            
                if (meshData.faceData[i].z >= 1)
                    newNormals[i] = meshData.normals[(int)meshData.faceData[i].z - 1];
            }

            Mesh mesh = new Mesh();

            mesh.name = meshData.fileName;
            mesh.vertices = newVertices;
            mesh.uv = newUVs;
            mesh.normals = newNormals;
            mesh.triangles = meshData.triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();

            timer.Stop();
            ModConsole.Log($"OBJIMPORTER: LOADED MESH {mesh.name} ({mesh.vertexCount} VERTICES) IN {timer.ElapsedMilliseconds}ms");

            return mesh;
        }

        static MeshData LoadMeshData(string path)
        {
            int triangles = 0, vertices = 0, vt = 0, vn = 0, face = 0;

            MeshData mesh = new MeshData();
            mesh.fileName = Path.GetFileName(path);

            using (StreamReader stream = File.OpenText(path))
            {
                string entireText = stream.ReadToEnd();

                using (StringReader reader = new StringReader(entireText))
                {
                    string currentText = reader.ReadLine();

                    char[] splitIdentifier = { ' ' };
                    string[] brokenString;

                    while (currentText != null)
                    {
                        if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") && !currentText.StartsWith("vn "))
                        {
                            currentText = reader.ReadLine();
                            if (currentText != null) currentText = currentText.Replace("  ", " ");
                        }
                        else
                        {
                            currentText = currentText.Trim();                           //Trim the current line
                            brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces

                            switch (brokenString[0])
                            {
                                case "v": vertices++; break;
                                case "vt": vt++; break;
                                case "vn": vn++; break;
                                case "f":
                                    face = face + brokenString.Length - 1;
                                    triangles = triangles + 3 * (brokenString.Length - 2);
                                    //brokenString.Length is 3 or greater since a face must have at least 3 vertices.For each additional vertice, there is an additional triangle in the mesh(hence this formula).
                                    break;
                            }

                            currentText = reader.ReadLine();
                            if (currentText != null) currentText = currentText.Replace("  ", " "); 
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

        static void CompileMeshData(ref MeshData mesh, string path)
        {
            using (StreamReader stream = File.OpenText(path))
            {
                string entireText = stream.ReadToEnd();

                using (StringReader reader = new StringReader(entireText))
                {
                    string currentText = reader.ReadLine();

                    char[] splitIdentifier = { ' ' }, splitIdentifier2 = { '/' };
                    string[] brokenString, brokenBrokenString;
                    int f = 0, f2 = 0, v = 0, vn = 0, vt = 0, vt1 = 0, vt2 = 0;

                    while (currentText != null)
                    {
                        //!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                        //!currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                        //!currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                        //!currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))

                        if (!currentText.StartsWithAny("f ", "v ", "vt ", "vn ", "g ", "usemtl ", "mtllib ", "vt1 ", "vt2 ", "vc ", "usemap "))
                        {
                            currentText = reader.ReadLine();
                            if (currentText != null) currentText = currentText.Replace("  ", " ");
                        }
                        else
                        {
                            currentText = currentText.Trim();
                            brokenString = currentText.Split(splitIdentifier, 50);
                            switch (brokenString[0])
                            {
                                case "g": case "usemtl": case "usemap": case "mtllib": case "vc": break;
                                case "v":
                                    mesh.vertices[v] = new Vector3(ToSingle(brokenString[1]), ToSingle(brokenString[2]), ToSingle(brokenString[3]));
                                    v++;
                                    break;
                                case "vt":
                                    mesh.uv[vt] = new Vector2(ToSingle(brokenString[1]), ToSingle(brokenString[2]));
                                    vt++;
                                    break;
                                case "vt1":
                                    mesh.uv[vt1] = new Vector2(ToSingle(brokenString[1]), ToSingle(brokenString[2]));
                                    vt1++;
                                    break;
                                case "vt2":
                                    mesh.uv[vt2] = new Vector2(ToSingle(brokenString[1]), ToSingle(brokenString[2]));
                                    vt2++;
                                    break;
                                case "vn":
                                    mesh.normals[vn] = new Vector3(ToSingle(brokenString[1]), ToSingle(brokenString[2]), ToSingle(brokenString[3]));
                                    vn++;
                                    break;
                                case "f":

                                    int j = 1;
                                    List<int> intArray = new List<int>();

                                    while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                    {
                                        Vector3 temp = new Vector3();
                                        brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                                        temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
                                        if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                        {
                                            if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                            {
                                                temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
                                            }
                                            temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
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
                            if (currentText != null) currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.

                        }
                    }
                }
            }
        }
    }
}
