using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphLogic;

namespace GraphLogic
{
    // Declara uma classe genérica para a aresta
    // O <T> serve para que a aresta use os mesmos tipos de dados que os vértices
    public class Edge<T>
    {
        // Origin representa o vértice de origem da aresta
        public Vertex<T> Origin { get; }

        // Destination representa o vértice de destino da aresta
        public Vertex<T> Destination { get; }

        // Weight guarda o peso da aresta, por exemplo, se do vértice A até B tem 3 metros, 3 é o peso 
        public int Weight { get; }

        // Construção da classe Edge que recebe como parâmetros origin, destination e weight
        public Edge(Vertex<T> origin, Vertex<T> destination, int weight)
        {
            Origin = origin;
            Destination = destination;
            Weight = weight;
        }

        // Define como a aresta será exibida como string
        public override string ToString()
        {
            // Retorna uma string no formato: Origem->Destino(Peso: X)
            return $"{Origin.Element} -> {Destination.Element} (Peso: {Weight})";
        }
    }
}

