using System;

namespace Umbraco.Tests.CodeFirst
{
    public class TopologicalSorter
    {
        #region - Private Members -

        private readonly int[] _vertices; // list of vertices
        private readonly int[,] _matrix; // adjacency matrix
        private int _numVerts; // current number of vertices
        private readonly int[] _sortedArray;

        #endregion

        #region - CTors -

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

        #endregion

        #region - Public Methods -

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
                int currentVertex = noSuccessors();
                if (currentVertex == -1) // must be a cycle                
                    throw new Exception("Graph has cycles");

                // insert vertex label in sorted array (start at end)
                _sortedArray[_numVerts - 1] = _vertices[currentVertex];

                deleteVertex(currentVertex); // delete vertex
            }

            // vertices all gone; return sortedArray
            return _sortedArray;
        }

        #endregion

        #region - Private Helper Methods -

        // returns vert with no successors (or -1 if no such verts)
        private int noSuccessors()
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

        private void deleteVertex(int delVert)
        {
            // if not last vertex, delete from vertexList
            if (delVert != _numVerts - 1)
            {
                for (int j = delVert; j < _numVerts - 1; j++)
                    _vertices[j] = _vertices[j + 1];

                for (int row = delVert; row < _numVerts - 1; row++)
                    moveRowUp(row, _numVerts);

                for (int col = delVert; col < _numVerts - 1; col++)
                    moveColLeft(col, _numVerts - 1);
            }
            _numVerts--; // one less vertex
        }

        private void moveRowUp(int row, int length)
        {
            for (int col = 0; col < length; col++)
                _matrix[row, col] = _matrix[row + 1, col];
        }

        private void moveColLeft(int col, int length)
        {
            for (int row = 0; row < length; row++)
                _matrix[row, col] = _matrix[row, col + 1];
        }

        #endregion
    }
}