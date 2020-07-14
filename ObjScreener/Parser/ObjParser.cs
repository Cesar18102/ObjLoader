using System;
using System.Linq;
using System.Collections.Generic;

using ObjScreener.Data;

using OpenTK;

namespace ObjScreener.Parser
{
    public class ObjParser
    {
        private uint? ParseIndex(string ind)
        {
            uint temp = 0;
            uint? result = null;

            if (UInt32.TryParse(ind, out temp))
                result = temp - 1;

            return result;
        }

        public Geometry Parse(string obj)
        {
            List<string> lines = obj.Replace(".", ",")
                                    .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(line => line[0] != '#').ToList();

            List<float> vertices = new List<float>();
            List<float> normals = new List<float>();
            List<float> texCoords = new List<float>();

            uint id = 0;
            List<uint> indexes = new List<uint>();
            Dictionary<ModelPoint, uint> modelPoints = new Dictionary<ModelPoint, uint>();

            foreach (string line in lines)
            {
                string[] lineData = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                switch(lineData[0])
                {
                    case "v":
                        for (int i = 1; i < 4; ++i)
                            vertices.Add(Convert.ToSingle(lineData[i]));
                        break;
                    case "vn":
                        for (int i = 1; i < 4; ++i)
                            normals.Add(Convert.ToSingle(lineData[i]));
                        break;
                    case "vt":
                        for (int i = 1; i < 3; ++i)
                            texCoords.Add(Convert.ToSingle(lineData[i]));
                        break;
                    case "f":
                        uint?[] inds = lineData.Skip(1).Aggregate(
                            new List<uint?>(),
                            (acc, data) => acc.Concat(data.Split('/').Select(dt => ParseIndex(dt))).ToList(),
                            acc => acc.ToArray()
                        );

                        if (lineData.Length == 4)//triangle
                        {
                            for (int i = 0; i < 3; ++i)
                            {
                                ModelPoint point = new ModelPoint(
                                    inds[i * 3].Value,
                                    inds[i * 3 + 1].Value,
                                    inds[i * 3 + 2].Value
                                );

                                if (!modelPoints.ContainsKey(point))
                                    modelPoints.Add(point, id++);
                                
                                indexes.Add(modelPoints[point]);
                            }
                        }
                        else if(lineData.Length == 5)//square is splitted onto two triangles
                        {
                            ModelPoint[] firstTriangle = new ModelPoint[]
                            {
                                new ModelPoint(inds[0].Value, inds[1].Value, inds[2].Value),
                                new ModelPoint(inds[3].Value, inds[4].Value, inds[5].Value),
                                new ModelPoint(inds[9].Value, inds[10].Value, inds[11].Value)
                            };

                            for (int i = 0; i < firstTriangle.Length; ++i)
                            {
                                if (!modelPoints.ContainsKey(firstTriangle[i]))
                                    modelPoints.Add(firstTriangle[i], id++);

                                indexes.Add(modelPoints[firstTriangle[i]]);
                            }

                            ModelPoint[] secondTriangle = new ModelPoint[]
                            {
                                new ModelPoint(inds[9].Value, inds[10].Value, inds[11].Value),
                                new ModelPoint(inds[3].Value, inds[4].Value, inds[5].Value),
                                new ModelPoint(inds[6].Value, inds[7].Value, inds[8].Value)
                            };

                            for (int i = 0; i < secondTriangle.Length; ++i)
                            {
                                if (!modelPoints.ContainsKey(secondTriangle[i]))
                                    modelPoints.Add(secondTriangle[i], id++);

                                indexes.Add(modelPoints[secondTriangle[i]]);
                            }
                        }
                        break;
                }
            }

            List<ReusablePoint> points = new List<ReusablePoint>();

            foreach(KeyValuePair<ModelPoint, uint> point in modelPoints)
            {
                Vector3 vertex = new Vector3(
                    vertices[(int)point.Key.VertexIndex * 3],
                    vertices[(int)point.Key.VertexIndex * 3 + 1],
                    vertices[(int)point.Key.VertexIndex * 3 + 2]
                );

                Vector3 normal = new Vector3(
                    normals[(int)point.Key.NormalIndex * 3],
                    normals[(int)point.Key.NormalIndex * 3 + 1],
                    normals[(int)point.Key.NormalIndex * 3 + 2]
                );

                Vector2 texCoord = new Vector2(
                    texCoords[(int)point.Key.TexIndex * 2],
                    1.0f - texCoords[(int)point.Key.TexIndex * 2 + 1]
                );

                points.Add(new ReusablePoint(vertex, normal, texCoord));
            }

            return new Geometry(points, indexes);
        }
    }
}
