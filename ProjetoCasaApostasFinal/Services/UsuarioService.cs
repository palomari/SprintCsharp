using Oracle.ManagedDataAccess.Client;
using ProjetoCasaApostas.Models;
using System.Collections.Generic;

namespace ProjetoCasaApostas.Services;

public class UsuarioService
{
    private readonly string _connStr;

    public UsuarioService(string connStr)
    {
        _connStr = connStr;
    }

    public bool CriarUsuario(Usuario usuario)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "INSERT INTO TB_USUARIO (NOME, EMAIL, SALDO) VALUES (:NOME, :EMAIL, :SALDO)";
        using var cmd = new OracleCommand(sql, conn) { BindByName = true };
        cmd.Parameters.Add("NOME", OracleDbType.Varchar2).Value = usuario.Nome;
        cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = usuario.Email;
        cmd.Parameters.Add("SALDO", OracleDbType.Decimal).Value = usuario.Saldo;

        return cmd.ExecuteNonQuery() > 0;
    }

    public List<Usuario> ListarUsuarios()
    {
        var usuarios = new List<Usuario>();
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "SELECT ID, NOME, EMAIL, SALDO FROM TB_USUARIO ORDER BY ID";
        using var cmd = new OracleCommand(sql, conn);
        using var rd = cmd.ExecuteReader();

        while (rd.Read())
        {
            usuarios.Add(new Usuario
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Email = rd.GetString(2),
                Saldo = rd.GetDecimal(3)
            });
        }
        return usuarios;
    }

    public Usuario? TryGetByEmail(string email)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "SELECT ID, NOME, EMAIL, SALDO FROM TB_USUARIO WHERE EMAIL = :EMAIL";
        using var cmd = new OracleCommand(sql, conn) { BindByName = true };
        cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = email;

        using var rd = cmd.ExecuteReader();
        if (!rd.Read()) return null;

        return new Usuario
        {
            Id = rd.GetInt32(0),
            Nome = rd.GetString(1),
            Email = rd.GetString(2),
            Saldo = rd.GetDecimal(3)
        };
    }

    public bool AtualizarUsuario(Usuario usuario)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "UPDATE TB_USUARIO SET NOME = :NOME, SALDO = :SALDO WHERE ID = :ID";
        using var cmd = new OracleCommand(sql, conn) { BindByName = true };
        cmd.Parameters.Add("NOME", OracleDbType.Varchar2).Value = usuario.Nome;
        cmd.Parameters.Add("SALDO", OracleDbType.Decimal).Value = usuario.Saldo;
        cmd.Parameters.Add("ID", OracleDbType.Int32).Value = usuario.Id;

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AtualizarSaldo(int usuarioId, decimal novoSaldo)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "UPDATE TB_USUARIO SET SALDO = :SALDO WHERE ID = :ID";
        using var cmd = new OracleCommand(sql, conn) { BindByName = true };
        cmd.Parameters.Add("SALDO", OracleDbType.Decimal).Value = novoSaldo;
        cmd.Parameters.Add("ID", OracleDbType.Int32).Value = usuarioId;

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool RemoverUsuario(string email)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string getIdSql = "SELECT ID FROM TB_USUARIO WHERE EMAIL = :EMAIL";
        using var getCmd = new OracleCommand(getIdSql, conn) { BindByName = true };
        getCmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = email;
        var userIdObj = getCmd.ExecuteScalar();
        if (userIdObj == null) return false;
        int userId = Convert.ToInt32(userIdObj);

        string delApostaSql = "DELETE FROM TB_APOSTA WHERE USUARIO_ID = :USERID";
        using (var delApostaCmd = new OracleCommand(delApostaSql, conn) { BindByName = true })
        {
            delApostaCmd.Parameters.Add("USERID", OracleDbType.Int32).Value = userId;
            delApostaCmd.ExecuteNonQuery();
        }

        string delUserSql = "DELETE FROM TB_USUARIO WHERE ID = :USERID";
        using (var delUserCmd = new OracleCommand(delUserSql, conn) { BindByName = true })
        {
            delUserCmd.Parameters.Add("USERID", OracleDbType.Int32).Value = userId;
            return delUserCmd.ExecuteNonQuery() > 0;
        }
    }

}
