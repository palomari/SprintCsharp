using System.Globalization;
using System.Text;
using ProjetoCasaApostas.Models;
using ProjetoCasaApostas.Services;
using ProjetoCasaApostas.Utils;
using Oracle.ManagedDataAccess.Client;

namespace ProjetoCasaApostas;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // Carrega user e senha de secrets.obf (ofuscados em Base64)
        var (user, pwd) = LoadCredentialsObfuscated();

        string ConnStr =
            "User Id=" + user + ";Password=" + pwd + ";" +
            "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))" +
            "(CONNECT_DATA=(SID=orcl)));";

        TestConnection(ConnStr);
        EnsureSchema(ConnStr);

        var usuarioService = new UsuarioService(ConnStr);
        var apostaService = new ApostaService(ConnStr);

        MainMenu(usuarioService, apostaService);
    }

    // ====================== MAIN MENU ======================
    static void MainMenu(UsuarioService usuarioService, ApostaService apostaService)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== CASA DE APOSTAS ====");
            Console.WriteLine("1) Usuários");
            Console.WriteLine("2) Apostas");
            Console.WriteLine("3) Relatórios");
            Console.WriteLine("0) Sair");
            string op = ReadString("Escolha: ");

            switch (op)
            {
                case "1": MenuUsuarios(usuarioService); break;
                case "2": MenuApostas(usuarioService, apostaService); break;
                case "3": MenuRelatorios(usuarioService); break;
                case "0": return;
                default: Console.WriteLine("Opção inválida."); Pause(); break;
            }
        }
    }

    // ====================== MENU USUÁRIOS ======================
    static void MenuUsuarios(UsuarioService usuarioService)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== USUÁRIOS ====");
            Console.WriteLine("1) Cadastrar");
            Console.WriteLine("2) Listar");
            Console.WriteLine("3) Atualizar");
            Console.WriteLine("4) Remover");
            Console.WriteLine("9) Voltar");
            string op = ReadString("Escolha: ");

            switch (op)
            {
                case "1":
                    string nome = ReadString("Nome: ");
                    string email = ReadString("Email: ");
                    decimal saldo = ReadDecimal("Saldo inicial: ");
                    bool ok = usuarioService.CriarUsuario(new Usuario { Nome = nome, Email = email, Saldo = saldo });
                    Console.WriteLine(ok ? "Usuário criado!" : "Falha ao criar usuário.");
                    Pause();
                    break;

                case "2":
                    var usuarios = usuarioService.ListarUsuarios();
                    Console.WriteLine("ID | NOME | EMAIL | SALDO");
                    Console.WriteLine("----------------------------------------");
                    foreach (var u in usuarios)
                        Console.WriteLine($"{u.Id} | {u.Nome} | {u.Email} | {u.Saldo:F2}");
                    Pause();
                    break;

                case "3":
                    string emailUpdate = ReadString("Email do usuário que deseja atualizar: ");
                    var usuario = usuarioService.TryGetByEmail(emailUpdate);
                    if (usuario == null) { Console.WriteLine("Usuário não encontrado."); Pause(); break; }

                    string novoNome = ReadString($"Novo nome (enter mantém '{usuario.Nome}'): ", allowEmpty: true, @default: usuario.Nome);
                    decimal novoSaldo = ReadDecimal($"Novo saldo (enter mantém {usuario.Saldo:F2}): ", allowEmpty: true, defaultValue: usuario.Saldo);

                    usuario.Nome = novoNome;
                    usuario.Saldo = novoSaldo;
                    bool atualizado = usuarioService.AtualizarUsuario(usuario);
                    Console.WriteLine(atualizado ? "Atualizado!" : "Nada atualizado.");
                    Pause();
                    break;

                case "4":
                    string emailDel = ReadString("Email do usuário para remover: ");
                    bool removido = usuarioService.RemoverUsuario(emailDel);
                    Console.WriteLine(removido ? "Usuário removido!" : "Usuário não encontrado.");
                    Pause();
                    break;

                case "9": return;
                default: Console.WriteLine("Opção inválida."); Pause(); break;
            }
        }
    }

    // ====================== MENU APOSTAS ======================
    static void MenuApostas(UsuarioService usuarioService, ApostaService apostaService)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== APOSTAS ====");
            Console.WriteLine("1) Futebol");
            Console.WriteLine("2) Corrida");
            Console.WriteLine("9) Voltar");
            string op = ReadString("Escolha: ");

            switch (op)
            {
                case "1": Apostar(usuarioService, apostaService, "Futebol", new[] { "Palmeiras", "São Paulo" }); break;
                case "2": Apostar(usuarioService, apostaService, "Corrida", new[] { "Corredor A", "Corredor B" }); break;
                case "9": return;
                default: Console.WriteLine("Opção inválida."); Pause(); break;
            }
        }
    }

    static void Apostar(UsuarioService usuarioService, ApostaService apostaService, string jogo, string[] opcoes)
    {
        string email = ReadString("Digite seu email: ");
        var usuario = usuarioService.TryGetByEmail(email);
        if (usuario == null) { Console.WriteLine("Usuário não encontrado."); Pause(); return; }

        Console.WriteLine($"Saldo atual: {usuario.Saldo:F2}");
        decimal valor = ReadDecimal("Valor da aposta: ");
        if (valor <= 0 || valor > usuario.Saldo) { Console.WriteLine("Valor inválido."); Pause(); return; }

        Console.WriteLine($"Escolha ({jogo}):");
        for (int i = 0; i < opcoes.Length; i++)
            Console.WriteLine($"{i + 1}) {opcoes[i]}");

        string escolha = ReadString("Sua escolha: ");
        string vencedor = opcoes[new Random().Next(opcoes.Length)];
        bool ganhou = (escolha == "1" && vencedor == opcoes[0]) || (escolha == "2" && vencedor == opcoes[1]);
        string resultado = ganhou ? "Ganhou" : "Perdeu";

        decimal novoSaldo = ganhou ? usuario.Saldo + valor : usuario.Saldo - valor;
        usuarioService.AtualizarSaldo(usuario.Id, novoSaldo);

        apostaService.RegistrarAposta(new Aposta
        {
            UsuarioId = usuario.Id,
            Jogo = jogo,
            Valor = valor,
            Resultado = resultado,
            DataAposta = DateTime.Now
        });

        Console.WriteLine($"Aposta {resultado}! Saldo atualizado: {novoSaldo:F2}");
        Pause();
    }

    // ====================== MENU RELATÓRIOS ======================
    static void MenuRelatorios(UsuarioService usuarioService)
    {
        Console.Clear();
        Console.WriteLine("==== RELATÓRIOS ====");
        var usuarios = usuarioService.ListarUsuarios();

        Console.WriteLine("ID | NOME | EMAIL | SALDO");
        Console.WriteLine("----------------------------------------");
        foreach (var u in usuarios)
            Console.WriteLine($"{u.Id} | {u.Nome} | {u.Email} | {u.Saldo:F2}");

        string opc = ReadString("Gerar relatorio_usuarios.json? (S/N): ").Trim().ToUpper();
        if (opc == "S")
            JsonExporter.Exportar(usuarios, "relatorio.json");

        Pause();
    }

    // ====================== DB BOOTSTRAP ======================
    static void TestConnection(string ConnStr)
    {
        try
        {
            using var conn = new OracleConnection(ConnStr);
            conn.Open();
            Console.WriteLine($"Conexão Oracle OK. Versão: {conn.ServerVersion}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Falha na conexão Oracle: " + ex.Message);
        }
    }

    static void EnsureSchema(string ConnStr)
    {
        using var conn = new OracleConnection(ConnStr);
        conn.Open();

        try
        {
            using var cmd = new OracleCommand(@"
                CREATE TABLE TB_USUARIO (
                    ID     NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    NOME   VARCHAR2(150) NOT NULL,
                    EMAIL  VARCHAR2(150) UNIQUE NOT NULL,
                    SALDO  NUMBER(12,2) DEFAULT 0
                )", conn);
            cmd.ExecuteNonQuery();
        }
        catch { }

        try
        {
            using var cmd = new OracleCommand(@"
                CREATE TABLE TB_APOSTA (
                    ID          NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    USUARIO_ID  NUMBER NOT NULL,
                    JOGO        VARCHAR2(50),
                    VALOR       NUMBER(12,2),
                    RESULTADO   VARCHAR2(10),
                    DATA_APOSTA DATE DEFAULT SYSDATE
                )", conn);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    // ====================== HELPERS ======================
    static string ReadString(string prompt, bool allowEmpty = false, string? @default = null)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            if (allowEmpty) return @default ?? "";
            Console.WriteLine("Valor inválido.");
        }
    }

    static decimal ReadDecimal(string prompt, bool allowEmpty = false, decimal defaultValue = 0)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (decimal.TryParse(input?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                return value;
            if (allowEmpty) return defaultValue;
            Console.WriteLine("Valor inválido.");
        }
    }

    static void Pause(string msg = "Pressione ENTER para continuar...")
    {
        Console.WriteLine();
        Console.WriteLine(msg);
        Console.ReadLine();
    }

    // ====================== SECRET LOADER ======================
    static (string user, string pwd) LoadCredentialsObfuscated()
    {
        string baseDir = AppContext.BaseDirectory;
        string path = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "secrets.obf"));

        if (File.Exists(path))
        {
            try
            {
                string obf = File.ReadAllText(path).Trim();
                byte[] bytes = Convert.FromBase64String(obf);
                string decoded = Encoding.UTF8.GetString(bytes);
                var parts = decoded.Split(':', 2);
                if (parts.Length == 2)
                    return (parts[0], parts[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Falha ao ler secrets.obf: " + ex.Message);
            }
        }

        Console.Write("DB User: ");
        string user = Console.ReadLine()!.Trim();
        Console.Write("DB Password: ");
        string pwd = Console.ReadLine()!.Trim();
        return (user, pwd);
    }
}
