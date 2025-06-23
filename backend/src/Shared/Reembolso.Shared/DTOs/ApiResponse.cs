namespace Reembolso.Shared.DTOs;

public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public bool IsSuccess => Sucesso; // Alias para compatibilidade
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }
    public List<string> Erros { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Propriedade ErrorMessage para compatibilidade
    public string ErrorMessage => Sucesso ? string.Empty : Mensagem;

    public static ApiResponse<T> SucessoComDados(T dados, string mensagem = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados
        };
    }

    public static ApiResponse<T> SucessoSemDados(string mensagem = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            Sucesso = true,
            Mensagem = mensagem
        };
    }

    public static ApiResponse<T> Erro(string mensagem, List<string>? erros = null)
    {
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros ?? new List<string>()
        };
    }

    public static ApiResponse<T> Erro(List<string> erros)
    {
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = "Erro na validação dos dados",
            Erros = erros
        };
    }

    // Métodos para compatibilidade com Success/Error
    public static ApiResponse<T> Success(T dados, string mensagem = "Operação realizada com sucesso")
    {
        return SucessoComDados(dados, mensagem);
    }

    public static ApiResponse<T> Error(string mensagem, List<string>? erros = null)
    {
        return Erro(mensagem, erros);
    }
}