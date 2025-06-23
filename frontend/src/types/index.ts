// Enums
export enum StatusSolicitacao {
  PENDENTE = 'PENDENTE',
  APROVADA = 'APROVADA',
  REJEITADA = 'REJEITADA',
  PAGA = 'PAGA'
}

export enum TipoUsuario {
  ADMIN = 'ADMIN',
  GESTOR = 'GESTOR',
  FUNCIONARIO = 'FUNCIONARIO',
  FINANCEIRO = 'FINANCEIRO'
}

export enum TipoDespesa {
  ALIMENTACAO = 'ALIMENTACAO',
  TRANSPORTE = 'TRANSPORTE',
  HOSPEDAGEM = 'HOSPEDAGEM',
  COMBUSTIVEL = 'COMBUSTIVEL',
  MATERIAL_ESCRITORIO = 'MATERIAL_ESCRITORIO',
  OUTROS = 'OUTROS'
}

export enum RelatorioTipo {
  SolicitacoesPeriodo = 'SOLICITACOES_PERIODO',
  AprovacoesPorGestor = 'APROVACOES_POR_GESTOR',
  ValoresPorDepartamento = 'VALORES_POR_DEPARTAMENTO',
  TendenciasGastos = 'TENDENCIAS_GASTOS',
  GastosPorFuncionario = 'GASTOS_POR_FUNCIONARIO',
  Auditoria = 'AUDITORIA'
}

// Interfaces principais
export interface Usuario {
  id: string;
  nome: string;
  email: string;
  tipo: TipoUsuario;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string;
}

export interface Funcionario {
  id: string;
  nome: string;
  email: string;
  cpf: string;
  cargo: string;
  departamento: string;
  gestorId?: string;
  gestor?: Funcionario;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string;
}

export interface SolicitacaoReembolso {
  id: string;
  funcionarioId: string;
  funcionario?: Funcionario;
  descricao: string;
  valor: number;
  tipoDespesa: TipoDespesa;
  dataGasto: string;
  status: StatusSolicitacao;
  observacoes?: string;
  aprovadoPorId?: string;
  aprovadoPor?: Usuario;
  dataAprovacao?: string;
  comprovantes: Comprovante[];
  criadoEm: string;
  atualizadoEm: string;
}

export interface Comprovante {
  id: string;
  solicitacaoId: string;
  nomeArquivo: string;
  caminhoArquivo: string;
  tamanhoArquivo: number;
  tipoArquivo: string;
  criadoEm: string;
}

export interface Relatorio {
  id: string;
  titulo: string;
  descricao: string;
  tipoRelatorio: string;
  parametros: any;
  caminhoArquivo?: string;
  status: string;
  criadoPorId: string;
  criadoPor?: Usuario;
  criadoEm: string;
  processadoEm?: string;
}

// Interface para filtros de relatórios
export interface RelatorioFiltros {
  dataInicio?: string;
  dataFim?: string;
  departamento?: string;
  funcionarioId?: string;
  status?: StatusSolicitacao;
  tipoDespesa?: TipoDespesa;
}

// DTOs para formulários
export interface LoginDto {
  email: string;
  senha: string;
}

export interface CriarUsuarioDto {
  nome: string;
  email: string;
  senha: string;
  tipo: TipoUsuario;
}

export interface CriarFuncionarioDto {
  nome: string;
  email: string;
  cpf: string;
  cargo: string;
  departamento: string;
  gestorId?: string;
}

export interface CriarSolicitacaoDto {
  funcionarioId: string;
  descricao: string;
  valor: number;
  tipoDespesa: TipoDespesa;
  dataGasto: string;
  observacoes?: string;
}

export interface AtualizarSolicitacaoDto {
  descricao?: string;
  valor?: number;
  tipoDespesa?: TipoDespesa;
  dataGasto?: string;
  observacoes?: string;
}

export interface AprovarRejeitarSolicitacaoDto {
  observacoes?: string;
}

export interface GerarRelatorioDto {
  titulo: string;
  descricao: string;
  tipoRelatorio: string;
  parametros: {
    dataInicio?: string;
    dataFim?: string;
    funcionarioId?: string;
    departamento?: string;
    status?: StatusSolicitacao;
    tipoDespesa?: TipoDespesa;
  };
}

// Interfaces para API responses
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  totalItems: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface AuthResponse {
  token: string;
  usuario: Usuario;
  expiresIn: number;
}

// Interfaces para filtros
export interface FiltroSolicitacoes {
  funcionarioId?: string;
  status?: StatusSolicitacao;
  tipoDespesa?: TipoDespesa;
  dataInicio?: string;
  dataFim?: string;
  departamento?: string;
  page?: number;
  pageSize?: number;
  orderBy?: string;
  orderDirection?: 'asc' | 'desc';
}

export interface FiltroFuncionarios {
  nome?: string;
  email?: string;
  departamento?: string;
  cargo?: string;
  ativo?: boolean;
  page?: number;
  pageSize?: number;
  orderBy?: string;
  orderDirection?: 'asc' | 'desc';
}

export interface FiltroRelatorios {
  titulo?: string;
  tipoRelatorio?: string;
  status?: string;
  dataInicio?: string;
  dataFim?: string;
  page?: number;
  pageSize?: number;
  orderBy?: string;
  orderDirection?: 'asc' | 'desc';
}

// Interfaces para estatísticas
export interface EstatisticasDashboard {
  totalSolicitacoes: number;
  solicitacoesPendentes: number;
  solicitacoesAprovadas: number;
  solicitacoesRejeitadas: number;
  valorTotalSolicitado: number;
  valorTotalAprovado: number;
  valorTotalPago: number;
  solicitacoesPorMes: { mes: string; quantidade: number; valor: number }[];
  solicitacoesPorTipo: { tipo: string; quantidade: number; valor: number }[];
  solicitacoesPorDepartamento: { departamento: string; quantidade: number; valor: number }[];
}

// Interfaces para notificações
export interface Notificacao {
  id: string;
  titulo: string;
  mensagem: string;
  tipo: 'info' | 'success' | 'warning' | 'error';
  lida: boolean;
  criadoEm: string;
}

// Interface para contexto de autenticação
export interface AuthContextType {
  usuario: Usuario | null;
  token: string | null;
  login: (email: string, senha: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
  updateAuthState: () => void;
}

// Interface para upload de arquivos
export interface UploadResponse {
  fileName: string;
  filePath: string;
  fileSize: number;
  fileType: string;
}

// Interface para configurações da aplicação
export interface AppConfig {
  apiBaseUrl: string;
  maxFileSize: number;
  allowedFileTypes: string[];
  itemsPerPage: number;
}

// Tipos utilitários
export type SortDirection = 'asc' | 'desc';
export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

// Interface para erros da API
export interface ApiError {
  message: string;
  status: number;
  errors?: Record<string, string[]>;
}