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
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  MoreVert as MoreVertIcon,
  Person as PersonIcon,
  AdminPanelSettings as AdminIcon,
  SupervisorAccount as GestorIcon,
  Refresh as RefreshIcon,
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
  FilterField,
  Column,
} from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  Usuario,
  TipoUsuario,
  PaginatedResponse,
} from '@/types';
import { formatDate, formatDateTime } from '@/utils';

interface UsuariosFilters {
  search: string;
  tipo: TipoUsuario | '';
  ativo: boolean | '';
}

const UsuariosList: React.FC = () => {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState<keyof Usuario>('nome');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [filters, setFilters] = useState<UsuariosFilters>({
    search: '',
    tipo: '',
    ativo: '',
  });
  const [selectedRows, setSelectedRows] = useState<Usuario[]>([]);
  const [actionMenuAnchor, setActionMenuAnchor] = useState<null | HTMLElement>(null);
  const [selectedUsuario, setSelectedUsuario] = useState<Usuario | null>(null);

  // API calls
  const {
    data: usuariosResponse,
    loading: loadingUsuarios,
    execute: fetchUsuarios,
  } = useApi<PaginatedResponse<Usuario>>(() =>
    apiService.obterUsuarios({
      page,
      pageSize,
      sortBy,
      sortDirection,
      search: filters.search || undefined,
      tipo: filters.tipo || undefined,
      ativo: filters.ativo !== '' ? filters.ativo : undefined,
    })
  );

  const {
    loading: deletingUsuario,
    execute: deleteUsuario,
  } = useApi(() => Promise.resolve());

  useEffect(() => {
    fetchUsuarios();
  }, [fetchUsuarios, page, pageSize, sortBy, sortDirection, filters]);

  const handleFilterChange = (newFilters: Partial<UsuariosFilters>) => {
    setFilters(prev => ({ ...prev, ...newFilters }));
    setPage(0); // Reset to first page when filtering
  };

  const handleSort = (field: keyof Usuario) => {
    if (sortBy === field) {
      setSortDirection(prev => prev === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(field);
      setSortDirection('asc');
    }
  };

  const handleActionMenuOpen = (event: React.MouseEvent<HTMLElement>, usuario: Usuario) => {
    setActionMenuAnchor(event.currentTarget);
    setSelectedUsuario(usuario);
  };

  const handleActionMenuClose = () => {
    setActionMenuAnchor(null);
    setSelectedUsuario(null);
  };

  const handleEdit = (usuario: Usuario) => {
    navigate(`/usuarios/${usuario.id}/edit`);
    handleActionMenuClose();
  };

  const handleDelete = (usuario: Usuario) => {
    showConfirmation({
      title: 'Excluir Usuário',
      message: `Tem certeza que deseja excluir o usuário "${usuario.nome}"? Esta ação não pode ser desfeita.`,
      type: 'error',
      onConfirm: async () => {
        try {
          await deleteUsuario(() => apiService.excluirUsuario(usuario.id));
          toast.success('Usuário excluído com sucesso!');
          fetchUsuarios();
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir usuário');
        }
      },
    });
    handleActionMenuClose();
  };

  const handleBulkDelete = () => {
    if (selectedRows.length === 0) return;
    
    showConfirmation({
      title: 'Excluir Usuários',
      message: `Tem certeza que deseja excluir ${selectedRows.length} usuário(s) selecionado(s)? Esta ação não pode ser desfeita.`,
      type: 'error',
      onConfirm: async () => {
        try {
          await Promise.all(
            selectedRows.map(usuario => deleteUsuario(() => apiService.excluirUsuario(usuario.id)))
          );
          toast.success(`${selectedRows.length} usuário(s) excluído(s) com sucesso!`);
          setSelectedRows([]);
          fetchUsuarios();
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir usuários');
        }
      },
    });
  };

  const getTipoIcon = (tipo: TipoUsuario) => {
    switch (tipo) {
      case TipoUsuario.ADMIN:
        return <AdminIcon />;
      case TipoUsuario.GESTOR:
        return <GestorIcon />;
      case TipoUsuario.FUNCIONARIO:
        return <PersonIcon />;
      default:
        return <PersonIcon />;
    }
  };

  const getTipoColor = (tipo: TipoUsuario) => {
    switch (tipo) {
      case TipoUsuario.ADMIN:
        return 'error';
      case TipoUsuario.GESTOR:
        return 'warning';
      case TipoUsuario.FUNCIONARIO:
        return 'primary';
      default:
        return 'default';
    }
  };

  const columns: Column<Usuario>[] = [
    {
      id: 'nome',
      label: 'Nome',
      sortable: true,
      render: (usuario: Usuario) => (
        <Box display="flex" alignItems="center" gap={1}>
          {getTipoIcon(usuario.tipo)}
          <Box>
            <Typography variant="body2" fontWeight="medium">
              {usuario.nome}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {usuario.email}
            </Typography>
          </Box>
        </Box>
      ),
    },
    {
      id: 'tipo',
      label: 'Tipo',
      sortable: true,
      render: (usuario: Usuario) => (
        <Chip
          label={usuario.tipo}
          color={getTipoColor(usuario.tipo) as any}
          size="small"
          icon={getTipoIcon(usuario.tipo)}
        />
      ),
    },
    {
      id: 'ativo',
      label: 'Status',
      sortable: true,
      render: (usuario: Usuario) => (
        <Chip
          label={usuario.ativo ? 'Ativo' : 'Inativo'}
          color={usuario.ativo ? 'success' : 'default'}
          size="small"
        />
      ),
    },
    {
      id: 'criadoEm',
      label: 'Data de Criação',
      sortable: true,
      render: (usuario: Usuario) => (
        <Typography variant="body2">
          {formatDate(usuario.criadoEm)}
        </Typography>
      ),
    },
    {
      id: 'atualizadoEm',
      label: 'Última Atualização',
      sortable: true,
      render: (usuario: Usuario) => (
        <Typography variant="body2" color="text.secondary">
          {formatDateTime(usuario.atualizadoEm)}
        </Typography>
      ),
    },
    {
      id: 'actions',
      label: 'Ações',
      sortable: false,
      render: (usuario: Usuario) => (
        <IconButton
          size="small"
          onClick={(e) => handleActionMenuOpen(e, usuario)}
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
      placeholder: 'Nome ou email...',
    },
    {
      name: 'tipo',
      label: 'Tipo de Usuário',
      type: 'select',
      options: [
        { value: '', label: 'Todos' },
        { value: TipoUsuario.ADMIN, label: 'Administrador' },
        { value: TipoUsuario.GESTOR, label: 'Gestor' },
        { value: TipoUsuario.FUNCIONARIO, label: 'Funcionário' },
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

  const canManageUsuarios = usuario?.tipo === TipoUsuario.ADMIN;

  if (!canManageUsuarios) {
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
          Usuários
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => fetchUsuarios()}
            disabled={loadingUsuarios}
          >
            Atualizar
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/usuarios/new')}
          >
            Novo Usuário
          </Button>
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
          title="Lista de Usuários"
          action={
            selectedRows.length > 0 && (
              <Button
                variant="outlined"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={handleBulkDelete}
                disabled={deletingUsuario}
              >
                Excluir Selecionados ({selectedRows.length})
              </Button>
            )
          }
        />
        <CardContent>
          {loadingUsuarios ? (
            <Loading message="Carregando usuários..." />
          ) : (
            <DataTable
              columns={columns}
              data={usuariosResponse?.data || []}
              totalCount={usuariosResponse?.totalItems || 0}
              page={page}
              pageSize={pageSize}
              sortBy={sortBy}
              sortDirection={sortDirection}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              onSort={handleSort}
              selectable
              selectedRows={selectedRows}
              onSelectionChange={setSelectedRows}
              emptyMessage="Nenhum usuário encontrado"
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
        <MenuItem onClick={() => selectedUsuario && handleEdit(selectedUsuario)}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Editar</ListItemText>
        </MenuItem>
        <MenuItem 
          onClick={() => selectedUsuario && handleDelete(selectedUsuario)}
          sx={{ color: 'error.main' }}
        >
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText>Excluir</ListItemText>
        </MenuItem>
      </Menu>

      <ConfirmationModal />
    </Box>
  );
};

export default UsuariosList;