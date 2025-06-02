using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GraphLogic; 

namespace GraphInterface 
{
    public partial class MainWindow : Window
    {
        private Graph<string> grafo;
        private Dictionary<Vertex<string>, Point> vertexRelativePositions = new Dictionary<Vertex<string>, Point>();
        private Random random = new Random();

        // Paleta de Cores Profissional
        private SolidColorBrush vertexFillBrush = new SolidColorBrush(Color.FromRgb(70, 130, 180)); // SteelBlue
        private SolidColorBrush vertexStrokeBrush = new SolidColorBrush(Color.FromRgb(46, 84, 120)); // Darker SteelBlue
        private SolidColorBrush edgeStrokeBrush = new SolidColorBrush(Color.FromRgb(119, 136, 153)); // LightSlateGray
        private SolidColorBrush weightForegroundBrush = Brushes.DarkSlateGray;
        private SolidColorBrush weightBackgroundBrush = new SolidColorBrush(Color.FromArgb(200, 245, 245, 245)); // Semi-transparent WhiteSmoke
        private SolidColorBrush canvasBackground = Brushes.WhiteSmoke; // Fundo do Canvas

        public MainWindow()
        {
            InitializeComponent();
            grafo = new Graph<string>();
            graphCanvas.Background = canvasBackground; // Define o fundo do canvas
            graphCanvas.SizeChanged += (s, e) => DrawGraph();
            Log("Interface iniciada. Carregue um grafo ou adicione vértices/arestas.");
        }

        // --- Manipuladores de Eventos --- 

        private void BtnAddVertice_Click(object sender, RoutedEventArgs e)
        {
            string nomeVertice = txtNovoVertice.Text.Trim();
            if (!string.IsNullOrWhiteSpace(nomeVertice))
            {
                try
                {
                    if (!grafo.Vertices.Any(v => v.Element == nomeVertice))
                    {
                        if (!vertexRelativePositions.Keys.Any(v => v.Element == nomeVertice))
                        {
                            vertexRelativePositions.Add(new Vertex<string>(nomeVertice), new Point(random.NextDouble() * 100, random.NextDouble() * 100));
                        }
                    }
                    grafo.AddVertex(nomeVertice);
                    Log($"Vértice 	'{nomeVertice}	' adicionado.");
                    txtNovoVertice.Clear();
                    AtualizarInterface();
                }
                catch (Exception ex)
                {
                    LogError($"Erro ao adicionar vértice: {ex.Message}");
                }
            }
            else
            {
                LogAviso("Nome do vértice não pode ser vazio.");
            }
        }

        private void BtnRemoverVertice_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRemoverVertice.SelectedItem is string nomeVertice && !string.IsNullOrWhiteSpace(nomeVertice))
            {
                try
                {
                    var verticeParaRemover = grafo.Vertices.FirstOrDefault(v => v.Element == nomeVertice);
                    if (verticeParaRemover != null)
                    {
                        var posKey = vertexRelativePositions.Keys.FirstOrDefault(k => k.Element == verticeParaRemover.Element);
                        if (posKey != null)
                        {
                            vertexRelativePositions.Remove(posKey);
                        }
                        grafo.RemoveVertex(nomeVertice);
                        Log($"Vértice 	'{nomeVertice}	' removido (se existia).");
                        AtualizarInterface();
                    }
                    else
                    {
                        LogAviso($"Vértice 	'{nomeVertice}	' não encontrado para remoção.");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Erro ao remover vértice: {ex.Message}");
                }
            }
            else
            {
                LogAviso("Selecione um vértice para remover.");
            }
        }

        private void BtnAddAresta_Click(object sender, RoutedEventArgs e)
        {
            string origem = cmbArestaOrigem.SelectedItem as string;
            string destino = cmbArestaDestino.SelectedItem as string;
            string pesoStr = txtArestaPeso.Text.Trim();

            if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(destino))
            {
                LogAviso("Selecione os vértices de origem e destino.");
                return;
            }

            if (int.TryParse(pesoStr, out int peso))
            {
                try
                {
                    grafo.AddEdge(origem, destino, peso);
                    Log($"Aresta 	'{origem}	' -> 	'{destino}	' (Peso: {peso}) adicionada.");
                    txtArestaPeso.Clear();
                    AtualizarInterface();
                }
                catch (Exception ex)
                {
                    LogError($"Erro ao adicionar aresta: {ex.Message}");
                }
            }
            else
            {
                LogAviso("Peso da aresta inválido. Insira um número inteiro.");
            }
        }

        private void BtnExecutarDijkstra_Click(object sender, RoutedEventArgs e)
        {
            string origemStr = cmbDijkstraOrigem.SelectedItem as string;
            string destinoStr = cmbDijkstraDestino.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(origemStr) || string.IsNullOrWhiteSpace(destinoStr))
            {
                LogAviso("Selecione os vértices de origem e destino para Dijkstra.");
                return;
            }

            var vOrigem = grafo.Vertices.FirstOrDefault(v => v.Element == origemStr);
            var vDestino = grafo.Vertices.FirstOrDefault(v => v.Element == destinoStr);

            if (vOrigem != null && vDestino != null)
            {
                try
                {
                    using (StringWriter sw = new StringWriter())
                    {
                        Console.SetOut(sw);
                        grafo.Dijkstra(vOrigem, vDestino);
                        Log("--- Resultado Dijkstra ---");
                        string result = sw.ToString().Trim();
                        Log(result);
                        Log("-------------------------");
                        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                        standardOutput.AutoFlush = true;
                        Console.SetOut(standardOutput);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Erro ao executar Dijkstra: {ex.Message}");
                }
            }
            else
            {
                LogAviso("Vértices de origem ou destino não encontrados no grafo.");
            }
        }

        private void BtnCarregarGrafo_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "grafo.txt";
            try
            {
                if (File.Exists(filePath))
                {
                    grafo = new Graph<string>();
                    vertexRelativePositions.Clear();
                    grafo.LoadFromFile(filePath);
                    foreach (var vertex in grafo.Vertices)
                    {
                        if (!vertexRelativePositions.Keys.Any(v => v.Element == vertex.Element))
                        {
                            vertexRelativePositions.Add(vertex, new Point(random.NextDouble() * 100, random.NextDouble() * 100));
                        }
                    }
                    Log($"Grafo carregado de 	'{filePath}	'.");
                    AtualizarInterface();
                }
                else
                {
                    LogAviso($"Arquivo 	'{filePath}	' não encontrado no diretório de execução.");
                }
            }
            catch (Exception ex)
            {
                LogError($"Erro ao carregar grafo do arquivo: {ex.Message}");
            }
        }

        // --- Métodos Auxiliares --- 

        private void AtualizarInterface()
        {
            AtualizarComboBoxesEListas();
            DrawGraph();
        }

        private void AtualizarComboBoxesEListas()
        {
            var currentVertexElements = grafo.Vertices.Select(v => v.Element).ToHashSet();
            var positionsToRemove = vertexRelativePositions.Keys.Where(k => !currentVertexElements.Contains(k.Element)).ToList();
            foreach (var keyToRemove in positionsToRemove)
            {
                vertexRelativePositions.Remove(keyToRemove);
            }
            foreach (var vertex in grafo.Vertices)
            {
                if (!vertexRelativePositions.Keys.Any(v => v.Element == vertex.Element))
                {
                    vertexRelativePositions.Add(vertex, new Point(random.NextDouble() * 100, random.NextDouble() * 100));
                }
            }

            var nomesVertices = grafo.Vertices.Select(v => v.Element).OrderBy(name => name).ToList();

            string selRemover = cmbRemoverVertice.SelectedItem as string;
            string selOrigemAresta = cmbArestaOrigem.SelectedItem as string;
            string selDestinoAresta = cmbArestaDestino.SelectedItem as string;
            string selOrigemDijkstra = cmbDijkstraOrigem.SelectedItem as string;
            string selDestinoDijkstra = cmbDijkstraDestino.SelectedItem as string;

            cmbRemoverVertice.ItemsSource = nomesVertices;
            cmbArestaOrigem.ItemsSource = nomesVertices;
            cmbArestaDestino.ItemsSource = nomesVertices;
            cmbDijkstraOrigem.ItemsSource = nomesVertices;
            cmbDijkstraDestino.ItemsSource = nomesVertices;

            cmbRemoverVertice.SelectedItem = selRemover;
            cmbArestaOrigem.SelectedItem = selOrigemAresta;
            cmbArestaDestino.SelectedItem = selDestinoAresta;
            cmbDijkstraOrigem.SelectedItem = selOrigemDijkstra;
            cmbDijkstraDestino.SelectedItem = selDestinoDijkstra;

            lstVertices.ItemsSource = nomesVertices;
            lstArestas.ItemsSource = grafo.Edges.Select(e => e.ToString()).OrderBy(s => s).ToList();
        }

        private void DrawGraph()
        {
            graphCanvas.Children.Clear();
            if (!grafo.Vertices.Any() || !vertexRelativePositions.Any()) return;

            double canvasWidth = graphCanvas.ActualWidth;
            double canvasHeight = graphCanvas.ActualHeight;
            if (canvasWidth <= 10 || canvasHeight <= 10) return;

            double padding = 40;
            double vertexRadius = 18;
            double drawWidth = canvasWidth - 2 * padding;
            double drawHeight = canvasHeight - 2 * padding;

            if (drawWidth <= 0 || drawHeight <= 0) return;

            // 1. Encontrar limites das posições relativas
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (var pos in vertexRelativePositions.Values)
            {
                minX = Math.Min(minX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxX = Math.Max(maxX, pos.X);
                maxY = Math.Max(maxY, pos.Y);
            }
            double graphRelWidth = maxX - minX;
            double graphRelHeight = maxY - minY;
            if (graphRelWidth < 1) graphRelWidth = 1;
            if (graphRelHeight < 1) graphRelHeight = 1;

            // 2. Calcular escala
            double scaleX = drawWidth / graphRelWidth;
            double scaleY = drawHeight / graphRelHeight;
            double scale = Math.Min(scaleX, scaleY);

            // 3. Calcular translação
            double graphCenterX = minX + graphRelWidth / 2.0;
            double graphCenterY = minY + graphRelHeight / 2.0;
            double canvasCenterX = padding + drawWidth / 2.0;
            double canvasCenterY = padding + drawHeight / 2.0;
            double offsetX = canvasCenterX - graphCenterX * scale;
            double offsetY = canvasCenterY - graphCenterY * scale;

            // 4. Calcular posições finais
            Dictionary<Vertex<string>, Point> finalPositions = new Dictionary<Vertex<string>, Point>();
            foreach (var kvp in vertexRelativePositions)
            {
                Point finalPos = new Point(kvp.Value.X * scale + offsetX, kvp.Value.Y * scale + offsetY);
                var graphVertex = grafo.Vertices.FirstOrDefault(v => v.Element == kvp.Key.Element);
                if (graphVertex != null)
                {
                    finalPositions[graphVertex] = finalPos;
                }
            }

            // 5. Desenhar Arestas (com nova cor)
            foreach (var edge in grafo.Edges)
            {
                if (finalPositions.TryGetValue(edge.Origin, out Point startPoint) &&
                    finalPositions.TryGetValue(edge.Destination, out Point endPoint))
                {
                    Line line = new Line
                    {
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                        X2 = endPoint.X,
                        Y2 = endPoint.Y,
                        Stroke = edgeStrokeBrush, // Cor da Aresta
                        StrokeThickness = 1.5
                    };
                    graphCanvas.Children.Add(line);

                    TextBlock weightText = new TextBlock
                    {
                        Text = edge.Weight.ToString(),
                        Foreground = weightForegroundBrush, // Cor do Texto do Peso
                        FontSize = 10,
                        Background = weightBackgroundBrush, // Fundo do Texto do Peso
                        Padding = new Thickness(2, 1, 2, 1), // Padding para o fundo
                        RenderTransform = new TranslateTransform((startPoint.X + endPoint.X) / 2 + 5, (startPoint.Y + endPoint.Y) / 2 - 8)
                    };
                    Panel.SetZIndex(weightText, 3); // Garante que o peso fique sobre tudo
                    graphCanvas.Children.Add(weightText);
                }
            }

            // 6. Desenhar Vértices (com novas cores)
            foreach (var kvp in finalPositions)
            {
                Vertex<string> vertex = kvp.Key;
                Point position = kvp.Value;

                Ellipse ellipse = new Ellipse
                {
                    Width = vertexRadius * 2,
                    Height = vertexRadius * 2,
                    Fill = vertexFillBrush, // Cor de Preenchimento do Vértice
                    Stroke = vertexStrokeBrush, // Cor da Borda do Vértice
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(ellipse, position.X - vertexRadius);
                Canvas.SetTop(ellipse, position.Y - vertexRadius);
                Panel.SetZIndex(ellipse, 1);
                graphCanvas.Children.Add(ellipse);

                TextBlock textBlock = new TextBlock
                {
                    Text = vertex.Element,
                    Foreground = Brushes.White, // Texto branco para contrastar com o fundo azul
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold, // Deixa a fonte um pouco mais forte
                    FontSize = 11,
                    Width = vertexRadius * 2,
                    Height = vertexRadius * 2,
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(0, (vertexRadius * 2 - 11) / 2 - 1, 0, 0)
                };
                Canvas.SetLeft(textBlock, position.X - vertexRadius);
                Canvas.SetTop(textBlock, position.Y - vertexRadius);
                Panel.SetZIndex(textBlock, 2);
                graphCanvas.Children.Add(textBlock);
            }
        }


        // --- Logging --- 

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLogs.AppendText($"{message}\n");
                txtLogs.ScrollToEnd();
            });
        }

        private void LogAviso(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLogs.AppendText($"AVISO: {message}\n");
                txtLogs.ScrollToEnd();
            });
        }

        private void LogError(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLogs.AppendText($"ERRO: {message}\n");
                txtLogs.ScrollToEnd();
            });
        }

    } 
}