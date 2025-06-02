using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection.Emit;

// Definimos uma classe genérica Vertex<T> que representa um vértice de um grafo
namespace GraphLogic 
{ 
    // T é um tipo genérico que identifica ou carrega dados do vértice
    public class Vertex<T>
    {
        // Guarda o conteúdo do vértice
        public T Element { get; set; }

        // Adjacents é uma lista de arestas conectadas ao vértice
        // Cada vértice guarda suas arestas de saída
        // Essas arestas sabem para qual outro vértice vão por causa do Edge<T>
        public List<Edge<T>> Adjacents { get; } = new List<Edge<T>>();

        public Vertex(T element)
        {
            Element = element;
        }
    }
}
