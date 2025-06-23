import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Avatar,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  MoreVert as MoreVertIcon,
  Person as PersonIcon,
  Refresh as RefreshIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import {
  DataTable,
  FilterPanel,
  Loading,
  useConfirmationModal,
} from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  Funcionario,
  TipoUsuario,
  PaginatedResponse,
} from '@/types';
import { FilterField, Column } from '@/components/Common';
import { formatDate, formatCPF, formatPhone, formatCurrency } from '@/utils';

interface FuncionariosFilters {
  search: string;
  departamento: string;
  cargo: string;
  ativo: boolean | '';
}

const FuncionariosList: React.FC = () => {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<keyof Funcionario>('nome');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [filters, setFilters] = useState<FuncionariosFilters>({
    search: '',
    departamento: '',
    cargo: '',
    ativo: '',
  });
  const [selectedRows, setSelectedRows] = useState<Funcionario[]>([]);
  const [actionMenuAnchor, setActionMenuAnchor] = useState<null | HTMLElement>(null);
  const [selectedFuncionario, setSelectedFuncionario] = useState<Funcionario | null>(null);

  // API calls
  const {
    data: funcionariosResponse,
    loading: loadingFuncionarios,
    execute: fetchFuncionarios,
  } = useApi<PaginatedResponse<Funcionario>>(() =>
    apiService.obterFuncionarios({
      page,
      pageSize,
      orderBy: sortBy,
      orderDirection: sortDirection,
      nome: filters.search || undefined,
      departamento: filters.departamento || undefined,
      cargo: filters.cargo || undefined,
      ativo: filters.ativo || undefined,
    })
  );

  const {
    data: departamentos,
    execute: fetchDepartamentos,
  } = useApi<string[]>(() => apiService.obterDepartamentos().then(response => response.data));

  const {
    data: cargos,
    execute: fetchCargos,
  } = useApi<string[]>(() => apiService.obterCargos().then(response => response.data));

  const {
    loading: deletingFuncionario,
    execute: deleteFuncionario,
  } = useApi<void>(() => Promise.resolve());

  useEffect(() => {
    fetchFuncionarios();
  }, [page, pageSize, sortBy, sortDirection, filters]);

  useEffect(() => {
    fetchDepartamentos();
    fetchCargos();
  }, []);

  const handleFilterChange = (newFilters: Partial<FuncionariosFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
    setPage(0); // Reset to first page when filtering
  };

  const handleSortChange = (column: keyof Funcionario, direction: 'asc' | 'desc') => {
    setSortBy(column);
    setSortDirection(direction);
  };

  const handleActionMenuOpen = (event: React.MouseEvent<HTMLElement>, funcionario: Funcionario) => {
    setActionMenuAnchor(event.currentTarget);
    setSelectedFuncionario(funcionario);
  };

  const handleActionMenuClose = () => {
    setActionMenuAnchor(null);
    setSelectedFuncionario(null);
  };

  const handleView = (funcionario: Funcionario) => {
    navigate(`/funcionarios/${funcionario.id}`);
    handleActionMenuClose();
  };

  const handleEdit = (funcionario: Funcionario) => {
    navigate(`/funcionarios/${funcionario.id}/edit`);
    handleActionMenuClose();
  };

  const handleDelete = (funcionario: Funcionario) => {
    showConfirmation({
      title: 'Excluir Funcionário',
      message: `Tem certeza que deseja excluir o funcionário "${funcionario.nome}"? Esta ação não pode ser desfeita.`,
      type: 'error',
      onConfirm: async () => {
        try {
          await deleteFuncionario(() => apiService.excluirFuncionario(funcionario.id));
          toast.success('Funcionário excluído com sucesso!');
          fetchFuncionarios();
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir funcionário');
        }
      },
    });
    handleActionMenuClose();
  };

  const handleBulkDelete = () => {
    if (selectedRows.length === 0) return;
    
    showConfirmation({
      title: 'Excluir Funcionários',
      message: `Tem certeza que deseja excluir ${selectedRows.length} funcionário(s) selecionado(s)? Esta ação não pode ser desfeita.`,
      type: 'error',
      onConfirm: async () => {
        try {
          await Promise.all(
            selectedRows.map(funcionario => deleteFuncionario(() => apiService.excluirFuncionario(funcionario.id)))
          );
          toast.success(`${selectedRows.length} funcionário(s) excluído(s) com sucesso!`);
          setSelectedRows([]);
          fetchFuncionarios();
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir funcionários');
        }
      },
    });
  };

  const getInitials = (nome: string) => {
    return nome
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const columns: Column<Funcionario>[] = [
    {
      id: 'nome',
      label: 'Funcionário',
      sortable: true,
      render: (funcionario: Funcionario) => (
        <Box display="flex" alignItems="center" gap={2}>
          <Avatar sx={{ bgcolor: 'primary.main' }}>
            {getInitials(funcionario.nome)}
          </Avatar>
          <Box>
            <Typography variant="body2" fontWeight="medium">
              {funcionario.nome}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {formatCPF(funcionario.cpf)}
            </Typography>
          </Box>
        </Box>
      ),
    },
    {
      id: 'departamento',
      label: 'Departamento',
      sortable: true,
      render: (funcionario: Funcionario) => (
        <Box>
          <Typography variant="body2">
            {funcionario.departamento}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {funcionario.cargo}
          </Typography>
        </Box>
      ),
    },
    {
      id: 'email',
      label: 'Contato',
      sortable: true,
      render: (funcionario: Funcionario) => (
        <Box>
          <Typography variant="body2">
            {funcionario.email}
          </Typography>
        </Box>
      ),
    },
    {
      id: 'ativo',
      label: 'Status',
      sortable: true,
      render: (funcionario: Funcionario) => (
        <Chip
          label={funcionario.ativo ? 'Ativo' : 'Inativo'}
          color={funcionario.ativo ? 'success' : 'default'}
          size="small"
        />
      ),
    },
    {
      id: 'actions',
      label: 'Ações',
      sortable: false,
      render: (funcionario: Funcionario) => (
        <IconButton
          size="small"
          onClick={(e) => handleActionMenuOpen(e, funcionario)}
        >
          <MoreVertIcon />
        </IconButton>
      ),
    },
  ];

  const filterFields: FilterField[] = [
    {
      name: 'search',
      label: 'Buscar',
      type: 'text',
      placeholder: 'Nome, CPF ou email...',
    },
    {
      name: 'departamento',
      label: 'Departamento',
      type: 'select',
      options: [
        { value: '', label: 'Todos' },
        ...(departamentos?.map(dept => ({ value: dept, label: dept })) || []),
      ],
    },
    {
      name: 'cargo',
      label: 'Cargo',
      type: 'select',
      options: [
        { value: '', label: 'Todos' },
        ...(cargos?.map(cargo => ({ value: cargo, label: cargo })) || []),
      ],
    },
    {
      name: 'ativo',
      label: 'Status',
      type: 'select',
      options: [
        { value: '', label: 'Todos' },
        { value: 'true', label: 'Ativo' },
        { value: 'false', label: 'Inativo' },
      ],
    },
  ];

  const canManageFuncionarios = usuario?.tipo === TipoUsuario.ADMIN || usuario?.tipo === TipoUsuario.GESTOR;

  if (!canManageFuncionarios) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="h6" color="error">
          Acesso negado
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Você não tem permissão para acessar esta página.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Funcionários
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => fetchFuncionarios()}
            disabled={loadingFuncionarios}
          >
            Atualizar
          </Button>
          {usuario?.tipo === TipoUsuario.ADMIN && (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/funcionarios/new')}
            >
              Novo Funcionário
            </Button>
          )}
        </Box>
      </Box>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <FilterPanel
            values={filters}
            onChange={(name, value) => handleFilterChange({ [name]: value })}
            fields={filterFields}
          />
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card>
        <CardHeader
          title="Lista de Funcionários"
          action={
            selectedRows.length > 0 && usuario?.tipo === TipoUsuario.ADMIN && (
              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={handleBulkDelete}
                disabled={deletingFuncionario}
              >
                Excluir Selecionados ({selectedRows.length})
              </Button>
            )
          }
        />
        <CardContent>
          {loadingFuncionarios ? (
            <Loading message="Carregando funcionários..." />
          ) : (
            <DataTable<Funcionario>
              columns={columns}
              data={funcionariosResponse?.data || []}
              totalCount={funcionariosResponse?.totalItems || 0}
              page={page}
              pageSize={pageSize}
              sortBy={sortBy}
              sortDirection={sortDirection}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              onSort={handleSortChange}
              selectable={usuario?.tipo === TipoUsuario.ADMIN}
              selectedRows={selectedRows}
              onSelectionChange={setSelectedRows}
              emptyMessage="Nenhum funcionário encontrado"
            />
          )}
        </CardContent>
      </Card>

      {/* Action Menu */}
      <Menu
        anchorEl={actionMenuAnchor}
        open={Boolean(actionMenuAnchor)}
        onClose={handleActionMenuClose}
      >
        <MenuItem onClick={() => selectedFuncionario && handleView(selectedFuncionario)}>
          <ListItemIcon>
            <VisibilityIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Visualizar</ListItemText>
        </MenuItem>
        
        {usuario?.tipo === TipoUsuario.ADMIN && (
          <>
            <MenuItem onClick={() => selectedFuncionario && handleEdit(selectedFuncionario)}>
              <ListItemIcon>
                <EditIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText>Editar</ListItemText>
            </MenuItem>
            <MenuItem 
              onClick={() => selectedFuncionario && handleDelete(selectedFuncionario)}
              sx={{ color: 'error.main' }}
            >
              <ListItemIcon>
                <DeleteIcon fontSize="small" color="error" />
              </ListItemIcon>
              <ListItemText>Excluir</ListItemText>
            </MenuItem>
          </>
        )}
      </Menu>

      <ConfirmationModal />
    </Box>
  );
};

export default FuncionariosList;