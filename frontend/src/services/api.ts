import axios, { AxiosInstance, AxiosResponse, AxiosError } from 'axios';
import { toast } from 'react-toastify';
import {
  ApiResponse,
  PaginatedResponse,
  AuthResponse,
  Usuario,
  Funcionario,
  SolicitacaoReembolso,
  Relatorio,
  LoginDto,
  CriarUsuarioDto,
  CriarFuncionarioDto,
  CriarSolicitacaoDto,
  AtualizarSolicitacaoDto,
  AprovarRejeitarSolicitacaoDto,
  GerarRelatorioDto,
  FiltroSolicitacoes,
  FiltroFuncionarios,
  FiltroRelatorios,
  EstatisticasDashboard,
  UploadResponse
} from '@/types';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: process.env.REACT_APP_API_URL || 'http://localhost:80',
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor para adicionar token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor para tratar erros
    this.api.interceptors.response.use(
      (response: AxiosResponse) => {
        return response;
      },
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('token');
          localStorage.removeItem('usuario');
          window.location.href = '/login';
          toast.error('Sessão expirada. Faça login novamente.');
        } else if (error.response?.status === 403) {
          toast.error('Você não tem permissão para realizar esta ação.');
        } else if (error.response?.status && error.response.status >= 500) {
          toast.error('Erro interno do servidor. Tente novamente mais tarde.');
        }
        return Promise.reject(error);
      }
    );
  }

  // Métodos de autenticação
  async login(dados: LoginDto): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/api/auth/login', dados);
    return response.data;
  }

  async register(dados: CriarUsuarioDto): Promise<ApiResponse<Usuario>> {
    const response = await this.api.post<ApiResponse<Usuario>>('/api/auth/register', dados);
    return response.data;
  }

  async refreshToken(): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/api/auth/refresh');
    return response.data;
  }

  // Métodos de usuários
  async obterUsuarios(filtros?: any): Promise<PaginatedResponse<Usuario>> {
    const response = await this.api.get<PaginatedResponse<Usuario>>('/api/usuarios', {
      params: filtros
    });
    return response.data;
  }

  async obterUsuario(id: string): Promise<ApiResponse<Usuario>> {
    const response = await this.api.get<ApiResponse<Usuario>>(`/api/usuarios/${id}`);
    return response.data;
  }

  async criarUsuario(dados: CriarUsuarioDto): Promise<ApiResponse<Usuario>> {
    const response = await this.api.post<ApiResponse<Usuario>>('/api/usuarios', dados);
    return response.data;
  }

  async atualizarUsuario(id: string, dados: Partial<CriarUsuarioDto>): Promise<ApiResponse<Usuario>> {
    const response = await this.api.put<ApiResponse<Usuario>>(`/api/usuarios/${id}`, dados);
    return response.data;
  }

  async excluirUsuario(id: string): Promise<ApiResponse<void>> {
    const response = await this.api.delete<ApiResponse<void>>(`/api/usuarios/${id}`);
    return response.data;
  }

  // Métodos de funcionários
  async obterFuncionarios(filtros?: FiltroFuncionarios): Promise<PaginatedResponse<Funcionario>> {
    const response = await this.api.get<PaginatedResponse<Funcionario>>('/api/funcionarios', {
      params: filtros
    });
    return response.data;
  }

  async obterFuncionario(id: string): Promise<ApiResponse<Funcionario>> {
    const response = await this.api.get<ApiResponse<Funcionario>>(`/api/funcionarios/${id}`);
    return response.data;
  }

  async criarFuncionario(dados: CriarFuncionarioDto): Promise<ApiResponse<Funcionario>> {
    const response = await this.api.post<ApiResponse<Funcionario>>('/api/funcionarios', dados);
    return response.data;
  }

  async atualizarFuncionario(id: string, dados: Partial<CriarFuncionarioDto>): Promise<ApiResponse<Funcionario>> {
    const response = await this.api.put<ApiResponse<Funcionario>>(`/api/funcionarios/${id}`, dados);
    return response.data;
  }

  async excluirFuncionario(id: string): Promise<ApiResponse<void>> {
    const response = await this.api.delete<ApiResponse<void>>(`/api/funcionarios/${id}`);
    return response.data;
  }

  // Métodos de solicitações
  async obterSolicitacoes(filtros?: FiltroSolicitacoes): Promise<PaginatedResponse<SolicitacaoReembolso>> {
    const response = await this.api.get<PaginatedResponse<SolicitacaoReembolso>>('/api/solicitacoes', {
      params: filtros
    });
    return response.data;
  }

  async obterSolicitacao(id: string): Promise<ApiResponse<SolicitacaoReembolso>> {
    const response = await this.api.get<ApiResponse<SolicitacaoReembolso>>(`/api/solicitacoes/${id}`);
    return response.data;
  }

  async criarSolicitacao(dados: CriarSolicitacaoDto): Promise<ApiResponse<SolicitacaoReembolso>> {
    const response = await this.api.post<ApiResponse<SolicitacaoReembolso>>('/api/solicitacoes', dados);
    return response.data;
  }

  async atualizarSolicitacao(id: string, dados: AtualizarSolicitacaoDto): Promise<ApiResponse<SolicitacaoReembolso>> {
    const response = await this.api.put<ApiResponse<SolicitacaoReembolso>>(`/api/solicitacoes/${id}`, dados);
    return response.data;
  }

  async aprovarSolicitacao(id: string, dados?: AprovarRejeitarSolicitacaoDto): Promise<ApiResponse<SolicitacaoReembolso>> {
    const response = await this.api.put<ApiResponse<SolicitacaoReembolso>>(`/api/solicitacoes/${id}/aprovar`, dados);
    return response.data;
  }

  async rejeitarSolicitacao(id: string, dados?: AprovarRejeitarSolicitacaoDto): Promise<ApiResponse<SolicitacaoReembolso>> {
    const response = await this.api.put<ApiResponse<SolicitacaoReembolso>>(`/api/solicitacoes/${id}/rejeitar`, dados);
    return response.data;
  }

  async excluirSolicitacao(id: string): Promise<ApiResponse<void>> {
    const response = await this.api.delete<ApiResponse<void>>(`/api/solicitacoes/${id}`);
    return response.data;
  }

  // Métodos de upload de arquivos
  async uploadComprovante(solicitacaoId: string, arquivo: File): Promise<ApiResponse<UploadResponse>> {
    const formData = new FormData();
    formData.append('arquivo', arquivo);
    
    const response = await this.api.post<ApiResponse<UploadResponse>>(
      `/api/solicitacoes/${solicitacaoId}/comprovantes`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  }

  async excluirComprovante(solicitacaoId: string, comprovanteId: string): Promise<ApiResponse<void>> {
    const response = await this.api.delete<ApiResponse<void>>(
      `/api/solicitacoes/${solicitacaoId}/comprovantes/${comprovanteId}`
    );
    return response.data;
  }

  async downloadComprovante(solicitacaoId: string, comprovanteId: string): Promise<Blob> {
    const response = await this.api.get(
      `/api/solicitacoes/${solicitacaoId}/comprovantes/${comprovanteId}/download`,
      {
        responseType: 'blob',
      }
    );
    return response.data;
  }

  // Métodos de relatórios
  async obterRelatorios(filtros?: FiltroRelatorios): Promise<PaginatedResponse<Relatorio>> {
    const response = await this.api.get<PaginatedResponse<Relatorio>>('/api/relatorios', {
      params: filtros
    });
    return response.data;
  }

  async obterRelatorio(id: string): Promise<ApiResponse<Relatorio>> {
    const response = await this.api.get<ApiResponse<Relatorio>>(`/api/relatorios/${id}`);
    return response.data;
  }

  async gerarRelatorio(dados: GerarRelatorioDto): Promise<ApiResponse<Relatorio>> {
    const response = await this.api.post<ApiResponse<Relatorio>>('/api/relatorios/gerar', dados);
    return response.data;
  }

  async downloadRelatorio(id: string): Promise<Blob> {
    const response = await this.api.get(`/api/relatorios/${id}/download`, {
      responseType: 'blob',
    });
    return response.data;
  }

  async excluirRelatorio(id: string): Promise<ApiResponse<void>> {
    const response = await this.api.delete<ApiResponse<void>>(`/api/relatorios/${id}`);
    return response.data;
  }

  // Métodos de estatísticas
  // Métodos de dashboard/estatísticas
  async obterEstatisticasDashboard(): Promise<ApiResponse<EstatisticasDashboard>> {
    const response = await this.api.get<ApiResponse<EstatisticasDashboard>>('/api/relatorios/estatisticas');
    return response.data;
  }

  // Métodos de health check
  async verificarSaude(): Promise<any> {
    const response = await this.api.get('/health');
    return response.data;
  }

  // Método para buscar departamentos
  async obterDepartamentos(): Promise<ApiResponse<string[]>> {
    const response = await this.api.get<ApiResponse<string[]>>('/api/funcionarios/departamentos');
    return response.data;
  }

  // Método para buscar cargos
  async obterCargos(): Promise<ApiResponse<string[]>> {
    const response = await this.api.get<ApiResponse<string[]>>('/api/funcionarios/cargos');
    return response.data;
  }

  // Método para buscar gestores
  async obterGestores(): Promise<ApiResponse<Funcionario[]>> {
    const response = await this.api.get<ApiResponse<Funcionario[]>>('/api/funcionarios/gestores');
    return response.data;
  }
}

export const apiService = new ApiService();
export default apiService;