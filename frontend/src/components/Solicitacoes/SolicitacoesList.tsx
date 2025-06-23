import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  IconButton,
  Chip,
  Avatar,
  Tooltip,
  Menu,
  MenuItem,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Check as ApproveIcon,
  Close as RejectIcon,
  Download as DownloadIcon,
  MoreVert as MoreVertIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import {
  DataTable,
  FilterPanel,
  useFilters,
  useConfirmationModal,
  Column,
  Action,
  FilterField,
} from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  SolicitacaoReembolso,
  StatusSolicitacao,
  TipoUsuario,
  FiltroSolicitacoes,
  PaginatedResponse,
} from '@/types';
import {
  formatCurrency,
  formatDate,
  truncateText,
} from '@/utils';

interface SolicitacoesListProps {
  funcionarioId?: string;
  showFilters?: boolean;
  showActions?: boolean;
  title?: string;
}

const SolicitacoesList: React.FC<SolicitacoesListProps> = ({
  funcionarioId,
  showFilters = true,
  showActions = true,
  title = 'Solicitações de Reembolso',
}) => {
  const navigate = useNavigate();
  const { usuario } = useAuth();

  // Função para obter cor do status
  const getStatusColor = (status: StatusSolicitacao) => {
    switch (status) {
      case StatusSolicitacao.PENDENTE:
        return 'warning';
      case StatusSolicitacao.APROVADA:
        return 'success';
      case StatusSolicitacao.REJEITADA:
        return 'error';
      case StatusSolicitacao.PAGA:
        return 'info';
      default:
        return 'default';
    }
  };
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<keyof SolicitacaoReembolso>('criadoEm');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');

  // Filtros
  const initialFilters: FiltroSolicitacoes = {
    funcionarioId: funcionarioId || undefined,
    status: undefined,
    dataInicio: undefined,
    dataFim: undefined,
    departamento: undefined,
    tipoDespesa: undefined,
  };

  const {
    filters,
    appliedFilters,
    updateFilter,
    applyFilters,
    clearFilters,
    getActiveFiltersCount,
  } = useFilters(initialFilters);

  // API calls
  const {
    data: solicitacoesData,
    loading,
    error,
    execute: fetchSolicitacoes,
  } = useApi<PaginatedResponse<SolicitacaoReembolso>>(() =>
    apiService.obterSolicitacoes({
      ...appliedFilters,
      page,
      pageSize,
      orderBy: sortBy as string,
      orderDirection: sortDirection,
    })
  );

  const {
    loading: actionLoading,
    execute: executeAction,
  } = useApi(() => Promise.resolve());

  useEffect(() => {
    fetchSolicitacoes();
  }, [page, pageSize, sortBy, sortDirection, appliedFilters]);

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
  };

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setPage(0);
  };

  const handleSort = (column: keyof SolicitacaoReembolso, direction: 'asc' | 'desc') => {
    setSortBy(column);
    setSortDirection(direction);
    setPage(0);
  };

  const handleApplyFilters = () => {
    applyFilters();
    setPage(0);
  };

  const handleClearFilters = () => {
    clearFilters();
    setPage(0);
  };

  const handleView = (solicitacao: SolicitacaoReembolso) => {
    navigate(`/solicitacoes/${solicitacao.id}`);
  };

  const handleEdit = (solicitacao: SolicitacaoReembolso) => {
    navigate(`/solicitacoes/${solicitacao.id}/editar`);
  };

  const handleDelete = (solicitacao: SolicitacaoReembolso) => {
    showConfirmation({
      title: 'Confirmar Exclusão',
      message: `Tem certeza que deseja excluir a solicitação #${solicitacao.id}?`,
      type: 'error',
      confirmText: 'Excluir',
      onConfirm: async () => {
        try {
          await executeAction(() => apiService.excluirSolicitacao(solicitacao.id));
          toast.success('Solicitação excluída com sucesso!');
          fetchSolicitacoes();
        } catch (error) {
          toast.error('Erro ao excluir solicitação');
        }
      },
    });
  };

  const handleApprove = (solicitacao: SolicitacaoReembolso) => {
    showConfirmation({
      title: 'Confirmar Aprovação',
      message: `Tem certeza que deseja aprovar a solicitação #${solicitacao.id}?`,
      type: 'success',
      confirmText: 'Aprovar',
      onConfirm: async () => {
        try {
          await executeAction(() => apiService.aprovarSolicitacao(solicitacao.id, {
            observacoes: 'Aprovado via sistema',
          }));
          toast.success('Solicitação aprovada com sucesso!');
          fetchSolicitacoes();
        } catch (error) {
          toast.error('Erro ao aprovar solicitação');
        }
      },
    });
  };

  const handleReject = (solicitacao: SolicitacaoReembolso) => {
    showConfirmation({
      title: 'Confirmar Rejeição',
      message: `Tem certeza que deseja rejeitar a solicitação #${solicitacao.id}?`,
      type: 'error',
      confirmText: 'Rejeitar',
      onConfirm: async () => {
        try {
          await executeAction(() => apiService.rejeitarSolicitacao(solicitacao.id, {
            observacoes: 'Rejeitado via sistema',
          }));
          toast.success('Solicitação rejeitada!');
          fetchSolicitacoes();
        } catch (error) {
          toast.error('Erro ao rejeitar solicitação');
        }
      },
    });
  };

  const handleDownloadComprovante = async (solicitacao: SolicitacaoReembolso) => {
    try {
      // Implementar download do comprovante
      toast.info('Funcionalidade em desenvolvimento');
    } catch (error) {
      toast.error('Erro ao baixar comprovante');
    }
  };

  const canEdit = (solicitacao: SolicitacaoReembolso) => {
    return (
      usuario?.id === solicitacao.funcionarioId &&
      solicitacao.status === StatusSolicitacao.PENDENTE
    );
  };

  const canDelete = (solicitacao: SolicitacaoReembolso) => {
    return (
      (usuario?.id === solicitacao.funcionarioId &&
        solicitacao.status === StatusSolicitacao.PENDENTE) ||
      usuario?.tipo === TipoUsuario.ADMIN
    );
  };

  const canApproveReject = (solicitacao: SolicitacaoReembolso) => {
    return (
      (usuario?.tipo === TipoUsuario.GESTOR || usuario?.tipo === TipoUsuario.ADMIN) &&
      solicitacao.status === StatusSolicitacao.PENDENTE
    );
  };

  const columns: Column<SolicitacaoReembolso>[] = [
    {
      id: 'numero',
      label: 'Número',
      minWidth: 100,
      sortable: true,
      render: (value) => (
        <Typography variant="body2" fontWeight="medium">
          #{value}
        </Typography>
      ),
    },
    {
      id: 'funcionario',
      label: 'Funcionário',
      minWidth: 200,
      render: (funcionario) => (
        <Box display="flex" alignItems="center" gap={1}>
          <Avatar sx={{ width: 32, height: 32, fontSize: '0.875rem' }}>
            {funcionario?.nome?.charAt(0) || 'U'}
          </Avatar>
          <Box>
            <Typography variant="body2">
              {funcionario?.nome || 'N/A'}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {funcionario?.departamento || 'N/A'}
            </Typography>
          </Box>
        </Box>
      ),
    },
    {
      id: 'descricao',
      label: 'Descrição',
      minWidth: 200,
      render: (value) => (
        <Tooltip title={value || ''}>
          <Typography variant="body2">
            {truncateText(value || '', 50)}
          </Typography>
        </Tooltip>
      ),
    },
    {
      id: 'valorTotal',
      label: 'Valor',
      minWidth: 120,
      align: 'right',
      sortable: true,
      render: (value) => (
        <Typography variant="body2" fontWeight="medium">
          {formatCurrency(value)}
        </Typography>
      ),
    },
    {
      id: 'status',
      label: 'Status',
      minWidth: 120,
      sortable: true,
      render: (value) => (
        <Chip
          label={value}
          size="small"
          color={getStatusColor(value)}
          variant="outlined"
        />
      ),
    },
    {
      id: 'criadoEm',
      label: 'Data Criação',
      minWidth: 120,
      sortable: true,
      render: (value) => (
        <Typography variant="body2">
          {formatDate(value)}
        </Typography>
      ),
    },
  ];

  const actions: Action<SolicitacaoReembolso>[] = [
    {
      label: 'Visualizar',
      icon: <ViewIcon />,
      onClick: handleView,
    },
    {
      label: 'Editar',
      icon: <EditIcon />,
      onClick: handleEdit,
      disabled: (row) => !canEdit(row),
    },
    {
      label: 'Aprovar',
      icon: <ApproveIcon />,
      onClick: handleApprove,
      disabled: (row) => !canApproveReject(row),
      color: 'success',
    },
    {
      label: 'Rejeitar',
      icon: <RejectIcon />,
      onClick: handleReject,
      disabled: (row) => !canApproveReject(row),
      color: 'error',
    },
    {
      label: 'Download',
      icon: <DownloadIcon />,
      onClick: handleDownloadComprovante,
    },
    {
      label: 'Excluir',
      icon: <DeleteIcon />,
      onClick: handleDelete,
      disabled: (row) => !canDelete(row),
      color: 'error',
    },
  ];

  const filterFields: FilterField[] = [
    {
      name: 'status',
      label: 'Status',
      type: 'select',
      options: [
        { value: '', label: 'Todos' },
        { value: StatusSolicitacao.PENDENTE, label: 'Pendente' },
        { value: StatusSolicitacao.APROVADA, label: 'Aprovada' },
        { value: StatusSolicitacao.REJEITADA, label: 'Rejeitada' },
        { value: StatusSolicitacao.PAGA, label: 'Paga' },
      ],
      gridSize: 3,
    },
    {
      name: 'dataInicio',
      label: 'Data Início',
      type: 'date',
      gridSize: 3,
    },
    {
      name: 'dataFim',
      label: 'Data Fim',
      type: 'date',
      gridSize: 3,
    },
    {
      name: 'departamento',
      label: 'Departamento',
      type: 'text',
      placeholder: 'Filtrar por departamento',
      gridSize: 3,
    },
    {
      name: 'valorMinimo',
      label: 'Valor Mínimo',
      type: 'number',
      placeholder: '0,00',
      gridSize: 3,
    },
    {
      name: 'valorMaximo',
      label: 'Valor Máximo',
      type: 'number',
      placeholder: '0,00',
      gridSize: 3,
    },
  ];

  if (error) {
    return (
      <Box
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        minHeight="400px"
        gap={2}
      >
        <Typography variant="h6" color="error">
          Erro ao carregar solicitações
        </Typography>
        <Button variant="contained" onClick={fetchSolicitacoes}>
          Tentar Novamente
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
        flexWrap="wrap"
        gap={2}
      >
        <Typography variant="h4" component="h1">
          {title}
        </Typography>
        
        {showActions && (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/solicitacoes/nova')}
          >
            Nova Solicitação
          </Button>
        )}
      </Box>

      {/* Filters */}
      {showFilters && (
        <Box mb={3}>
          <FilterPanel
            fields={filterFields}
            values={filters}
            onChange={updateFilter}
            onApply={handleApplyFilters}
            onClear={handleClearFilters}
            loading={loading}
            activeFiltersCount={getActiveFiltersCount()}
          />
        </Box>
      )}

      {/* Data Table */}
      <DataTable
        data={solicitacoesData?.data || []}
        columns={columns}
        loading={loading || actionLoading}
        totalCount={solicitacoesData?.totalItems}
        page={page}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
        onSort={handleSort}
        sortBy={sortBy}
        sortDirection={sortDirection}
        actions={showActions ? actions : []}
        onRowClick={handleView}
        emptyMessage="Nenhuma solicitação encontrada"
      />

      <ConfirmationModal />
    </Box>
  );
};

export default SolicitacoesList;