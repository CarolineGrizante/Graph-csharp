using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
// using static permite acesssr membros estáticos da classe JSType
// Por exemplo, ao usar esse using static, você pode acessar diretamente membros estáticos sem precisar escrever JSType
using System.Runtime.Intrinsics.X86;
using GraphLogic;
// Permite usar funcionalidades de baixo nível para processadores x86, como instruções SIMD para otimizar o desempenho
// Em um processador que suporta SIMD, é possível realizar operações em vetores de dados em vez de apenas em um único dado por vez

// Observação: No código usamos a palavra predecessor: No algoritmo significa o vértice que veio antes dele no caminho mais curto a partir da origem
namespace GraphLogic
{
    // Definimos uma classe genérica Graph<T> que representa um grafo
    // T é o tipo dos elementos dos vértices
    public class Graph<T>
    {
        // Lista de vértices que compõem o grafo
        public List<Vertex<T>> Vertices { get; } = new List<Vertex<T>>();
        // Lista de arestas que conectam os vértices
        public List<Edge<T>> Edges { get; } = new List<Edge<T>>();

        // Adiciona um novo vértice ao grafo
        public void AddVertex(T element)
        {
            if (element == null)
            {
                Console.WriteLine("Elemento não pode ser nulo.");
                return;
            }
            // Verifica se o vértice já existe no grafo antes de adicioná-lo
            // Vertices.Any(...) verifica se algum dos vértices já existentes na lista satisfaz a condição 
            // v => EqualityComparer<T>.Default.Equals(v.Element, element) é a condição que está sendo verificada 
            // Para cada vértice v, compara v.Element (o valor armazenado) com o novo element que está sendo inserido
            // Se nenhum vértice igual já existe, a condição se torna true

            if (!Vertices.Any(v => EqualityComparer<T>.Default.Equals(v.Element, element)))
                Vertices.Add(new Vertex<T>(element));
        }
        // Adiciona uma aresta entre dois vértices, com um peso (tamanho) específico
        // T originElement e T destinationElement são os valores dos vértices entre os quais queremos criar uma conexão
        // int weight é o peso da aresta, por exemplo, a distância da origem ao destino

        public void AddEdge(T originElement, T destinationElement, int weight)
        {
            if (originElement == null || destinationElement == null)
            {
                Console.WriteLine("Os elementos de origem ou destino não podem ser nulos.");
                return;
            }
            // Encontra os vértices de origem e destino no grafo
            // Encontra os vértices de origem e destino no grafo
            // Procura, dentro da lista de vértices (Vertices), os objetos Vertex<T> cujo Element seja igual ao valor passado (originElement ou destinationElement)
            // Usa FirstOrDefault(...), que retorna o primeiro vértice encontrado com o valor correspondente
            // Ou null se nenhum for encontrado

            var origin = Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, originElement));
            var destination = Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, destinationElement));

            if (origin != null && destination != null)
            {
                // Cria a aresta e a adiciona à lista de arestas do grafo
                // Cria um novo objeto do tipo Edge<T>, que representa a aresta entre os dois vértices, com o peso fornecido
                var edge = new Edge<T>(origin, destination, weight);
                Edges.Add(edge);
                origin.Adjacents.Add(edge); // Adiciona a aresta à lista de adjacências do vértice de origem
            }
            else
            {
                Console.WriteLine("Vértices não encontrados.");
            }
        }

        // Lê todas as linhas do arquivo e percorre uma por uma
        // Cada line será uma string do tipo "origem,destino,peso"
        public void LoadFromFile(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                // Lê as linhas do arquivo, que devem estar no formato "origem,destino,peso"
                var parts = line.Split(',');
                if (parts.Length != 3 || !int.TryParse(parts[2], out int weight))
                    continue;
                // Divide a linha em partes separadas pela vírgula.
                // parts[0] = origem
                // parts[1] = destino
                // parts[2] = peso

                // Adiciona os vértices e a aresta ao grafo a partir dos dados do arquivo
                AddVertex((T)Convert.ChangeType(parts[0], typeof(T)));
                AddVertex((T)Convert.ChangeType(parts[1], typeof(T)));
                // Converte as partes 0 e 1 para o tipo T (genérico), por exemplo: string, int, etc
                // Adiciona os dois vértices ao grafo
                AddEdge((T)Convert.ChangeType(parts[0], typeof(T)), (T)Convert.ChangeType(parts[1], typeof(T)), weight);
            }
        }

        // Implementação do algoritmo de Dijkstra para encontrar o menor caminho entre dois vértices
        public void Dijkstra(Vertex<T> origem, Vertex<T> destino)
        {
            if (origem == null || destino == null)
            {
                Console.WriteLine("Vértices de origem ou destino são nulos.");
                return;
            }

            // Dicionários para armazenar a distância dos vértices e os vértices predecessores no caminho
            // dist: armazena a menor distância conhecida da origem até cada vértice
            var dist = new Dictionary<Vertex<T>, int>();

            // prev: armazena o vértice anterior no caminho mais curto até aquele ponto
            var prev = new Dictionary<Vertex<T>, Vertex<T>?>();

            // Conjunto de vértices que já foram visitados
            var visitados = new HashSet<Vertex<T>>();

            // Inicializa a distância de todos os vértices para infinito, exceto a origem que tem distância 0
            foreach (var v in Vertices)
            {
                dist[v] = int.MaxValue;
                prev[v] = null;
            }
            dist[origem] = 0;

            // Executa o algoritmo de Dijkstra
            while (visitados.Count != Vertices.Count)
            {
                // Encontra o vértice não visitado com a menor distância
                // Seleciona o vértice u com a menor distância que ainda não foi visitado
                // Esse será o próximo vértice a ser analisado 
                var u = dist.Where(x => !visitados.Contains(x.Key))
                            .OrderBy(x => x.Value)
                            .FirstOrDefault().Key;

                // Se não há mais vértices acessíveis (desconectados), o algoritmo termina
                if (u == null || dist[u] == int.MaxValue)
                    break;

                // Marca o vértice como visitado
                visitados.Add(u);

                // Atualiza a distância dos vizinhos do vértice u
                foreach (var e in u.Adjacents)
                {
                    // v: vértice de destino da aresta
                    // alt: nova distância possível até v passando por u
                    var v = e.Destination;
                    var alt = dist[u] + e.Weight;

                    // Se o novo caminho encontrado for mais curto, atualiza a distância e o predecessor
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }

            // Verifica se o destino é alcançável
            if (dist[destino] == int.MaxValue)
            {
                Console.WriteLine("Caminho não encontrado.");
                return;
            }
            // Reconstrução do caminho mais curto
            // Aqui é onde o dicionário de predecessores (prev) entra em ação:
            // Começamos no destino
            // A cada iteração, pegamos o predecessor(prev[at]) e empilhamos esse vértice
            // Continuamos até at ser null(chegamos na origem)
            // Usamos uma pilha(Stack) porque o caminho foi montado de trás pra frente, ou seja, ao empilhar, conseguimos imprimir na ordem correta: do início até o fim

            var caminho = new Stack<Vertex<T>>();
            for (var at = destino; at != null; at = prev[at])
                caminho.Push(at);

            // Exibe o caminho e o peso total
            // Exibe na tela o caminho encontrado, incluindo o vértice de origem, o destino e o peso total do caminho
            Console.WriteLine($"Caminho de {origem.Element} para {destino.Element} (Peso: {dist[destino]}):");
            // Concatena e imprime os elementos do caminho com "->" entre eles, para mostrar a sequência de vértices
            Console.WriteLine(string.Join(" -> ", caminho.Select(v => v.Element)));
        }

        // Método para remover um vértice do grafo
        public void RemoveVertex(T element)
        {
            if (element == null)
            {
                Console.WriteLine("Elemento não pode ser nulo.");
                return;
            }

            // Encontra o vértice no grafo
            // Procura o vértice na lista de vértices com base no elemento informado
            var vertex = Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, element));

            // Se o vértice foi encontrado
            if (vertex != null)
            {
                // Percorre todas as arestas que saem desse vértice 
                // Usamos ToList() para evitar problemas ao remover durante a iteração
                foreach (var edge in vertex.Adjacents.ToList())
                {
                    // Remove todas as arestas que saem do vértice a ser removido
                    RemoveEdge(vertex.Element, edge.Destination.Element);
                }

                foreach (var v in Vertices)
                {
                    v.Adjacents.RemoveAll(e => EqualityComparer<T>.Default.Equals(e.Destination.Element, element));
                }
                // Remove o vértice da lista de vértices
                Vertices.Remove(vertex);
                // Informa que o vértice foi removido com sucesso
                Console.WriteLine($"Vértice {element} removido com sucesso.");
            }
            else
            {
                // Se o vértice não foi encontrado no grafo, informa isso ao usuário
                Console.WriteLine($"Vértice {element} não encontrado.");
            }
        }

        // Método para remover uma aresta do grafo
        public void RemoveEdge(T originElement, T destinationElement)
        {
            // Verifica se os elementos de origem ou destino são nulos
            if (originElement == null || destinationElement == null)
            {
                Console.WriteLine("Os elementos de origem ou destino não podem ser nulos.");
                return;
            }

            // Encontra os vértices de origem e destino
            // Procura o vértice de origem na lista de vértices do grafo
            var origin = Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, originElement));
            // Procura o vértice de destino na lista de vértices do grafo
            var destination = Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, destinationElement));

            // Se ambos os vértices foram encontrados
            if (origin != null && destination != null)
            {
                // Encontra a aresta que conecta os dois vértices

                // origin.Adjacents: aacessa a lista de adjacências do vértice de origem (origin)
                // FirstOrDefault(...): Este é um método LINQ que tenta encontrar o primeiro elemento da lista de adjacências que atenda à condição especificada 
                var edgeToRemove = origin.Adjacents.FirstOrDefault(e =>
                    EqualityComparer<T>.Default.Equals(e.Destination.Element, destinationElement));
                // e => EqualityComparer<T>.Default.Equals(e.Destination.Element, destinationElement):
                // e: Representa uma aresta(do tipo Edge<T>)
                // e.Destination.Element: acessa o vértice de destino da aresta pegando o valor do elemento (do tipo T)
                // destinationElement: é o valor que estamos buscando no vértice de destino
                // EqualityComparer<T>.Default.Equals(..., ...): o método Equals verifica se o valor de e.Destination.Element é igual ao valor de destinationElement

                // Se a aresta foi encontrada
                if (edgeToRemove != null)
                {
                    // Remove a aresta da lista de adjacências do vértice de origem e da lista de arestas do grafo
                    origin.Adjacents.Remove(edgeToRemove);
                    // Remove a aresta da lista geral de arestas do grafo
                    Edges.Remove(edgeToRemove);
                    // Exibe uma mensagem de sucesso
                    Console.WriteLine($"Aresta de {originElement} para {destinationElement} removida com sucesso!");
                }
                else
                {
                    Console.WriteLine("Aresta não encontrada.");
                }
            }
            else
            {
                Console.WriteLine("Vértices inválidos.");
            }
        }

        // Esse método é responsável por listar todos os vértices do grafo 
        public void ListVertices()
        {
            Console.WriteLine("Vértices:");
            // Faz um loop (foreach) que percorre cada elemento v da lista de vértices Vertices
            foreach (var v in Vertices)
            {
                Console.WriteLine($"- {v.Element}");
                // Para cada vértice v, exibe o valor do seu elemento (v.Element) 
                // O $ permite interpolação de string, ou seja, inserir o valor diretamente no texto
            }
        }

        // Lista todas as arestas do grafo
        public void ListEdges()
        {
            Console.WriteLine("Arestas:");
            // Percorre cada aresta e na lista Edges
            foreach (var e in Edges)
            {
                Console.WriteLine($"{e.Origin.Element} -> {e.Destination.Element} (Peso: {e.Weight})");
                // Mostra no console: O elemento de origem da aresta(e.Origin.Element)
                // O símbolo ->, indicando a direção da aresta
                // O elemento de destino(e.Destination.Element)
                // O peso da aresta(e.Weight)
            }
        }

        // Retorna um objeto do tipo Vertex<T> ou null (indicado pelo ?), caso o vértice não seja encontrado
        public Vertex<T>? GetVertexByElement(T element)
        {
            return Vertices.FirstOrDefault(v => EqualityComparer<T>.Default.Equals(v.Element, element));
            // Busca na lista Vertices o primeiro vértice cujo elemento (v.Element) seja igual ao element fornecido
            // A comparação é feita usando EqualityComparer<T>.Default.Equals(...), que garante uma comparação genérica segura(funciona com qualquer tipo T)
            // Se encontrar, retorna o vértice
            // Se não encontrar, retorna null(por isso usamos FirstOrDefault)
        }
    }
}

