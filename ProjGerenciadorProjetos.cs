using System;
using System.Collections.Generic;
using System.Linq;

namespace GerenciadorProjetos
{
    public class Tarefa
    {
        public int Id { get; private set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public int Prioridade { get; set; } // 1=alta, 2=média, 3=baixa
        public string Status { get; private set; } // "Aberta", "Fechada", "Cancelada"
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataConclusao { get; private set; }

        public Tarefa(int id, string titulo, string descricao, int prioridade)
        {
            Id = id;
            Titulo = titulo;
            Descricao = descricao;
            Prioridade = prioridade;
            Status = "Aberta";
            DataCriacao = DateTime.Now;
            DataConclusao = null;
        }

        public void Concluir()
        {
            if (Status != "Aberta")
                throw new InvalidOperationException("Só é possível concluir tarefas abertas.");
            Status = "Fechada";
            DataConclusao = DateTime.Now;
        }

        public void Cancelar()
        {
            if (Status != "Aberta")
                throw new InvalidOperationException("Só é possível cancelar tarefas abertas.");
            Status = "Cancelada";
            DataConclusao = DateTime.Now;
        }

        public void Reabrir()
        {
            if (Status == "Aberta")
                throw new InvalidOperationException("A tarefa já está aberta.");
            Status = "Aberta";
            DataConclusao = null;
        }

        public static string NomePrioridade(int p) => p switch
        {
            1 => "Alta",
            2 => "Média",
            3 => "Baixa",
            _ => "?"
        };

        public override string ToString()
        {
            string conclusao = DataConclusao.HasValue ? DataConclusao.Value.ToString("dd/MM/yyyy HH:mm") : "-";
            return $"[{Id}] {Titulo} | Prioridade: {NomePrioridade(Prioridade)} | Status: {Status} | " +
                   $"Criada: {DataCriacao:dd/MM/yyyy HH:mm} | Conclusão: {conclusao}\n      {Descricao}";
        }
    }

    public class Projeto
    {
        public int Id { get; private set; }
        public string Nome { get; set; }
        private List<Tarefa> tarefas = new List<Tarefa>();
        public IReadOnlyList<Tarefa> Tarefas => tarefas;

        public Projeto(int id, string nome)
        {
            Id = id;
            Nome = nome;
        }

        public void AdicionarTarefa(Tarefa t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            tarefas.Add(t);
        }

        public bool RemoverTarefa(Tarefa t) => tarefas.Remove(t);

        // Busca pelo Id da tarefa informada (retorna null se não encontrar)
        public Tarefa BuscarTarefa(Tarefa t) => tarefas.FirstOrDefault(x => x.Id == t.Id);

        public List<Tarefa> TarefasPorStatus(string s) =>
            tarefas.Where(x => x.Status.Equals(s, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Tarefa> TarefasPorPrioridade(int p) =>
            tarefas.Where(x => x.Prioridade == p).ToList();

        public int TotalAbertas() => tarefas.Count(x => x.Status == "Aberta");
        public int TotalFechadas() => tarefas.Count(x => x.Status == "Fechada");
        public int TotalCanceladas() => tarefas.Count(x => x.Status == "Cancelada");

        public override string ToString() => $"[{Id}] {Nome} ({tarefas.Count} tarefa(s))";
    }

    public class Projetos
    {
        private List<Projeto> itens = new List<Projeto>();

        public bool Adicionar(Projeto p)
        {
            if (p == null || itens.Any(x => x.Id == p.Id)) return false;
            itens.Add(p);
            return true;
        }

        public bool Remover(Projeto p) => itens.Remove(p);

        // Busca pelo Id do projeto informado
        public Projeto Buscar(Projeto p) => itens.FirstOrDefault(x => x.Id == p.Id);

        public List<Projeto> Listar() => new List<Projeto>(itens);
    }

    class Program
    {
        static Projetos projetos = new Projetos();
        static int proximoIdProjeto = 1;
        static int proximoIdTarefa = 1;

        static void Main(string[] args)
        {
            int opcao;
            do
            {
                MostrarMenu();
                opcao = LerInt("Opção: ");
                Console.WriteLine();
                try
                {
                    switch (opcao)
                    {
                        case 0: // aceito 0 também para sair
                        case 1: opcao = 1; Console.WriteLine("Encerrando..."); break;
                        case 2: AdicionarProjeto(); break;
                        case 3: PesquisarProjeto(); break;
                        case 4: RemoverProjeto(); break;
                        case 5: AdicionarTarefa(); break;
                        case 6: AlterarSituacao("concluir"); break;
                        case 7: AlterarSituacao("cancelar"); break;
                        case 8: AlterarSituacao("reabrir"); break;
                        case 9: ListarTarefasProjeto(); break;
                        case 10: FiltrarEmProjeto(); break;
                        case 11: FiltrarEmTodos(); break;
                        case 12: ResumoGeral(); break;
                        default: Console.WriteLine("Opção inválida."); break;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Operação não permitida: {ex.Message}");
                }
                if (opcao != 1)
                {
                    Console.WriteLine("\nPressione ENTER para continuar...");
                    Console.ReadLine();
                }
            } while (opcao != 1);
        }

        // ---------- Menu ----------
        static void MostrarMenu()
        {
            Console.Clear();
            Console.WriteLine("===== GERENCIADOR DE PROJETOS =====");
            Console.WriteLine(" 1  - Sair");
            Console.WriteLine(" 2  - Adicionar projeto");
            Console.WriteLine(" 3  - Pesquisar projeto (tarefas por status e totais)");
            Console.WriteLine(" 4  - Remover projeto (apenas se sem tarefas)");
            Console.WriteLine(" 5  - Adicionar tarefa em projeto");
            Console.WriteLine(" 6  - Concluir tarefa");
            Console.WriteLine(" 7  - Cancelar tarefa");
            Console.WriteLine(" 8  - Reabrir tarefa");
            Console.WriteLine(" 9  - Listar tarefas de um projeto");
            Console.WriteLine(" 10 - Filtrar tarefas por status/prioridade em um projeto");
            Console.WriteLine(" 11 - Filtrar tarefas por status/prioridade em todos os projetos");
            Console.WriteLine(" 12 - Resumo geral");
            Console.WriteLine("===================================");
        }

        // ---------- Entrada de dados ----------
        static int LerInt(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                if (int.TryParse(Console.ReadLine(), out int v)) return v;
                Console.WriteLine("Valor inválido. Digite um número inteiro.");
            }
        }

        static string LerTexto(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                string s = (Console.ReadLine() ?? "").Trim();
                if (s.Length > 0) return s;
                Console.WriteLine("O texto não pode ficar vazio.");
            }
        }

        static int LerPrioridade()
        {
            while (true)
            {
                int p = LerInt("Prioridade (1=alta, 2=média, 3=baixa): ");
                if (p >= 1 && p <= 3) return p;
                Console.WriteLine("Prioridade deve ser 1, 2 ou 3.");
            }
        }

        static string LerStatus()
        {
            while (true)
            {
                Console.WriteLine("Status: 1=Aberta, 2=Fechada, 3=Cancelada");
                int s = LerInt("Escolha: ");
                switch (s)
                {
                    case 1: return "Aberta";
                    case 2: return "Fechada";
                    case 3: return "Cancelada";
                }
                Console.WriteLine("Opção de status inválida.");
            }
        }

        static Projeto LerProjeto()
        {
            if (projetos.Listar().Count == 0)
            {
                Console.WriteLine("Nenhum projeto cadastrado.");
                return null;
            }
            int id = LerInt("Id do projeto: ");
            Projeto p = projetos.Buscar(new Projeto(id, ""));
            if (p == null) Console.WriteLine("Projeto não encontrado.");
            return p;
        }

        static Tarefa LerTarefa(Projeto p)
        {
            int id = LerInt("Id da tarefa: ");
            Tarefa t = p.BuscarTarefa(new Tarefa(id, "", "", 1));
            if (t == null) Console.WriteLine("Tarefa não encontrada neste projeto.");
            return t;
        }

        static void ImprimirTarefas(List<Tarefa> lista)
        {
            if (lista.Count == 0) { Console.WriteLine("  (nenhuma tarefa)"); return; }
            foreach (var t in lista) Console.WriteLine("  " + t);
        }

        // ---------- Opções ----------
        static void AdicionarProjeto()
        {
            string nome = LerTexto("Nome do projeto: ");
            var p = new Projeto(proximoIdProjeto++, nome);
            if (projetos.Adicionar(p))
                Console.WriteLine($"Projeto cadastrado com id {p.Id}.");
            else
                Console.WriteLine("Não foi possível cadastrar o projeto.");
        }

        static void PesquisarProjeto()
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            Console.WriteLine($"\nProjeto: {p}");
            foreach (string status in new[] { "Aberta", "Fechada", "Cancelada" })
            {
                Console.WriteLine($"\n-- {status}s --");
                ImprimirTarefas(p.TarefasPorStatus(status));
            }
            Console.WriteLine($"\nTotais -> Abertas: {p.TotalAbertas()} | Fechadas: {p.TotalFechadas()} | Canceladas: {p.TotalCanceladas()}");
        }

        static void RemoverProjeto()
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            if (p.Tarefas.Count > 0)
            {
                Console.WriteLine("Não é possível remover: o projeto possui tarefas.");
                return;
            }
            Console.WriteLine(projetos.Remover(p) ? "Projeto removido." : "Falha ao remover o projeto.");
        }

        static void AdicionarTarefa()
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            string titulo = LerTexto("Título: ");
            string descricao = LerTexto("Descrição: ");
            int prioridade = LerPrioridade();
            var t = new Tarefa(proximoIdTarefa++, titulo, descricao, prioridade);
            p.AdicionarTarefa(t);
            Console.WriteLine($"Tarefa cadastrada com id {t.Id}.");
        }

        static void AlterarSituacao(string acao)
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            Tarefa t = LerTarefa(p);
            if (t == null) return;
            switch (acao)
            {
                case "concluir": t.Concluir(); Console.WriteLine("Tarefa concluída."); break;
                case "cancelar": t.Cancelar(); Console.WriteLine("Tarefa cancelada."); break;
                case "reabrir": t.Reabrir(); Console.WriteLine("Tarefa reaberta."); break;
            }
        }

        static void ListarTarefasProjeto()
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            Console.WriteLine($"\nTarefas de {p.Nome}:");
            ImprimirTarefas(p.Tarefas.ToList());
        }

        static List<Tarefa> AplicarFiltro(Projeto p)
        {
            Console.WriteLine("Filtrar por: 1=Status, 2=Prioridade");
            int tipo = LerInt("Escolha: ");
            if (tipo == 1) return p.TarefasPorStatus(LerStatus());
            if (tipo == 2) return p.TarefasPorPrioridade(LerPrioridade());
            Console.WriteLine("Tipo de filtro inválido.");
            return null;
        }

        static void FiltrarEmProjeto()
        {
            Projeto p = LerProjeto();
            if (p == null) return;
            var resultado = AplicarFiltro(p);
            if (resultado == null) return;
            Console.WriteLine($"\nResultado em {p.Nome}:");
            ImprimirTarefas(resultado);
        }

        static void FiltrarEmTodos()
        {
            var lista = projetos.Listar();
            if (lista.Count == 0) { Console.WriteLine("Nenhum projeto cadastrado."); return; }

            Console.WriteLine("Filtrar por: 1=Status, 2=Prioridade");
            int tipo = LerInt("Escolha: ");
            string status = null;
            int prioridade = 0;
            if (tipo == 1) status = LerStatus();
            else if (tipo == 2) prioridade = LerPrioridade();
            else { Console.WriteLine("Tipo de filtro inválido."); return; }

            foreach (var p in lista)
            {
                Console.WriteLine($"\nProjeto: {p}");
                var r = tipo == 1 ? p.TarefasPorStatus(status) : p.TarefasPorPrioridade(prioridade);
                ImprimirTarefas(r);
            }
        }

        static void ResumoGeral()
        {
            var lista = projetos.Listar();
            int abertas = lista.Sum(p => p.TotalAbertas());
            int fechadas = lista.Sum(p => p.TotalFechadas());
            int canceladas = lista.Sum(p => p.TotalCanceladas());

            // % concluídas = fechadas / (abertas + fechadas), ignorando as canceladas
            int baseCalculo = abertas + fechadas;
            double pct = baseCalculo == 0 ? 0 : (double)fechadas / baseCalculo * 100;

            Console.WriteLine("===== RESUMO GERAL =====");
            Console.WriteLine($"Projetos:    {lista.Count}");
            Console.WriteLine($"Abertas:     {abertas}");
            Console.WriteLine($"Fechadas:    {fechadas}");
            Console.WriteLine($"Canceladas:  {canceladas}");
            Console.WriteLine($"% concluídas (fechadas / (abertas + fechadas)): {pct:F1}%");
        }
    }
}
