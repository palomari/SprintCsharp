using System;

namespace ProjetoCasaApostas.Models;

public class Aposta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Jogo { get; set; }
    public decimal Valor { get; set; }
    public string Resultado { get; set; }
    public DateTime DataAposta { get; set; }
}
