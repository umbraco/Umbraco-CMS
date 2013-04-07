using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core
{
    /// <summary>
    /// Topological Sort algorithm for sorting items based on dependencies.
    /// Use the static method TopologicalSorter.GetSortedItems for a convenient 
    /// way of sorting a list of items with dependencies between them.
    /// </summary>
    public class TopologicalSorter
    {
        private readonly int[] _vertices; // list of vertices
        private readonly int[,] _matrix; // adjacency matrix
        private int _numVerts; // current number of vertices
        private readonly int[] _sortedArray;

        public TopologicalSorter(int size)
        {
            _vertices = new int[size];
            _matrix = new int[size, size];
            _numVerts = 0;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    _matrix[i, j] = 0;
            _sortedArray = new int[size]; // sorted vert labels
        }

        #region Public Methods

        public int AddVertex(int vertex)
        {
            _vertices[_numVerts++] = vertex;
            return _numVerts - 1;
        }

        public void AddEdge(int start, int end)
        {
            _matrix[start, end] = 1;
        }

        public int[] Sort() // toplogical sort
        {
            while (_numVerts > 0) // while vertices remain,
            {
                // get a vertex with no successors, or -1
                int currentVertex = NoSuccessors();
                if (currentVertex == -1) // must be a cycle                
                    throw new Exception("Graph has cycles");

                // insert vertex label in sorted array (start at end)
                _sortedArray[_numVerts - 1] = _vertices[currentVertex];

                DeleteVertex(currentVertex); // delete vertex
            }

            // vertices all gone; return sortedArray
            return _sortedArray;
        }

        #endregion

        #region Private Helper Methods

        // returns vert with no successors (or -1 if no such verts)
        private int NoSuccessors()
        {
            for (int row = 0; row < _numVerts; row++)
            {
                bool isEdge = false; // edge from row to column in adjMat
                for (int col = 0; col < _numVerts; col++)
                {
                    if (_matrix[row, col] > 0) // if edge to another,
                    {
                        isEdge = true;
                        break; // this vertex has a successor try another
                    }
                }
                if (!isEdge) // if no edges, has no successors
                    return row;
            }
            return -1; // no
        }

        private void DeleteVertex(int delVert)
        {
            // if not last vertex, delete from vertexList
            if (delVert != _numVerts - 1)
            {
                for (int j = delVert; j < _numVerts - 1; j++)
                    _vertices[j] = _vertices[j + 1];

                for (int row = delVert; row < _numVerts - 1; row++)
                    MoveRowUp(row, _numVerts);

                for (int col = delVert; col < _numVerts - 1; col++)
                    MoveColLeft(col, _numVerts - 1);
            }
            _numVerts--; // one less vertex
        }

        private void MoveRowUp(int row, int length)
        {
            for (int col = 0; col < length; col++)
                _matrix[row, col] = _matrix[row + 1, col];
        }

        private void MoveColLeft(int col, int length)
        {
            for (int row = 0; row < length; row++)
                _matrix[row, col] = _matrix[row, col + 1];
        }

        #endregion

        #region Static methods

        public static IEnumerable<T> GetSortedItems<T>(List<DependencyField<T>> fields) where T : class 
        {
            int[] sortOrder = GetTopologicalSortOrder(fields);
            var list = new List<T>();
            for (int i = 0; i < sortOrder.Length; i++)
            {
                var field = fields[sortOrder[i]];
                list.Add(field.Item.Value);
            }
            list.Reverse();
            return list;
        }

        internal static int[] GetTopologicalSortOrder<T>(List<DependencyField<T>> fields) where T : class 
        {
            var g = new TopologicalSorter(fields.Count());
            var indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                indexes[fields[i].Alias.ToLowerInvariant()] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Length; j++)
                    {
                        if (indexes.ContainsKey(fields[i].DependsOn[j].ToLowerInvariant()) == false)
                            throw new IndexOutOfRangeException(
                                string.Format(
                                    "The alias '{0}' has an invalid dependency. The dependency '{1}' does not exist in the list of aliases",
                                    fields[i], fields[i].DependsOn[j]));

                        g.AddEdge(i, indexes[fields[i].DependsOn[j].ToLowerInvariant()]);
                    }
                }
            }

            int[] result = g.Sort();
            return result;
        }

        #endregion

        public class DependencyField<T> where T : class 
        {
            public DependencyField()
            {
            }

            public DependencyField(string @alias, string[] dependsOn, Lazy<T> item)
            {
                Alias = alias;
                DependsOn = dependsOn;
                Item = item;
            }

            public string Alias { get; set; }
            public string[] DependsOn { get; set; }
            public Lazy<T> Item { get; set; }
        }
    }
}