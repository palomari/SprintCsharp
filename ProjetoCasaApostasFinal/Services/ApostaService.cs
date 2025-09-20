using Oracle.ManagedDataAccess.Client;
using ProjetoCasaApostas.Models;
using System.Collections.Generic;

namespace ProjetoCasaApostas.Services;

public class ApostaService
{
    private readonly string _connStr;

    public ApostaService(string connStr)
    {
        _connStr = connStr;
    }

    public void RegistrarAposta(Aposta aposta)
    {
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = @"
    INSERT INTO TB_APOSTA (USUARIO_ID, JOGO, VALOR, RESULTADO, DATA_APOSTA)
    VALUES (:USUARIO_ID, :JOGO, :VALOR, :RESULTADO, SYSDATE)";

        using var cmd = new OracleCommand(sql, conn) { BindByName = true };
        cmd.Parameters.Add("USUARIO_ID", OracleDbType.Int32).Value = aposta.UsuarioId;
        cmd.Parameters.Add("JOGO", OracleDbType.Varchar2).Value = aposta.Jogo;
        cmd.Parameters.Add("VALOR", OracleDbType.Decimal).Value = aposta.Valor;
        cmd.Parameters.Add("RESULTADO", OracleDbType.Varchar2).Value = aposta.Resultado;

        cmd.ExecuteNonQuery();

    }


    public List<Aposta> ListarApostas()
    {
        var apostas = new List<Aposta>();
        using var conn = new OracleConnection(_connStr);
        conn.Open();

        string sql = "SELECT ID, USUARIO_ID, JOGO, VALOR, RESULTADO, DATA_APOSTA FROM TB_APOSTA ORDER BY DATA_APOSTA DESC";
        using var cmd = new OracleCommand(sql, conn);
        using var rd = cmd.ExecuteReader();

        while (rd.Read())
        {
            apostas.Add(new Aposta
            {
                Id = rd.GetInt32(0),
                UsuarioId = rd.GetInt32(1),
                Jogo = rd.GetString(2),
                Valor = rd.GetDecimal(3),
                Resultado = rd.GetString(4),
                DataAposta = rd.GetDateTime(5)
            });
        }
        return apostas;
    }
}
