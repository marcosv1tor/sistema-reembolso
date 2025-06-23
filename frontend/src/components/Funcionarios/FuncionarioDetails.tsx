import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Avatar,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Paper,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  ArrowBack as ArrowBackIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Business as BusinessIcon,
  Work as WorkIcon,
  CalendarToday as CalendarIcon,
  AttachMoney as MoneyIcon,
  Person as PersonIcon,
  Assignment as AssignmentIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { Loading, useConfirmationModal } from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  Funcionario,
  SolicitacaoReembolso,
  TipoUsuario,
  StatusSolicitacao,
} from '@/types';
import {
  formatDate,
  formatCPF,
  formatPhone,
  formatCurrency,
} from '@/utils';

const FuncionarioDetails: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();

  // API calls
  const {
    data: funcionario,
    loading: loadingFuncionario,
    execute: fetchFuncionario,
  } = useApi<Funcionario>(() => apiService.obterFuncionario(id!).then(response => response.data));

  const {
    data: solicitacoes,
    loading: loadingSolicitacoes,
    execute: fetchSolicitacoes,
  } = useApi<SolicitacaoReembolso[]>(() => 
    apiService.obterSolicitacoes({ funcionarioId: id, page: 0, pageSize: 10 })
      .then(response => response.data)
  );

  const {
    loading: deletingFuncionario,
    execute: deleteFuncionario,
  } = useApi<void>(() => Promise.resolve());

  useEffect(() => {
    if (id) {
      fetchFuncionario();
      fetchSolicitacoes();
    }
  }, [id]);

  const handleEdit = () => {
    navigate(`/funcionarios/${id}/edit`);
  };

  const handleDelete = () => {
    showConfirmation({
      title: 'Excluir Funcionário',
      message: `Tem certeza que deseja excluir o funcionário "${funcionario?.nome}"? Esta ação não pode ser desfeita.`,
      type: 'error',
      onConfirm: async () => {
        try {
          await deleteFuncionario(() => apiService.excluirFuncionario(id!));
          toast.success('Funcionário excluído com sucesso!');
          navigate('/funcionarios');
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir funcionário');
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

  const getStatusColor = (status: StatusSolicitacao) => {
    switch (status) {

      case StatusSolicitacao.PENDENTE:
        return 'warning';
      case StatusSolicitacao.APROVADA:
        return 'success';
      case StatusSolicitacao.REJEITADA:
        return 'error';
      default:
        return 'default';
    }
  };

  const canEdit = usuario?.tipo === TipoUsuario.ADMIN;
  const canDelete = usuario?.tipo === TipoUsuario.ADMIN;
  const canView = usuario?.tipo === TipoUsuario.ADMIN ||
    usuario?.tipo === TipoUsuario.GESTOR ||
    usuario?.id === id;

  if (!canView) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="h6" color="error">
          Acesso negado
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Você não tem permissão para visualizar este funcionário.
        </Typography>
        <Button
          variant="contained"
          onClick={() => navigate('/funcionarios')}
          sx={{ mt: 2 }}
        >
          Voltar
        </Button>
      </Box>
    );
  }

  if (loadingFuncionario) {
    return <Loading message="Carregando funcionário..." />;
  }

  if (!funcionario) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="h6" color="error">
          Funcionário não encontrado
        </Typography>
        <Button
          variant="contained"
          onClick={() => navigate('/funcionarios')}
          sx={{ mt: 2 }}
        >
          Voltar
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
        <Box display="flex" alignItems="center" gap={2}>
          <IconButton onClick={() => navigate('/funcionarios')}>
            <ArrowBackIcon />
          </IconButton>
          <Avatar sx={{ bgcolor: 'primary.main', width: 56, height: 56 }}>
            {getInitials(funcionario.nome)}
          </Avatar>
          <Box>
            <Typography variant="h4" component="h1">
              {funcionario.nome}
            </Typography>
            <Typography variant="subtitle1" color="text.secondary">
              {funcionario.cargo} • {funcionario.departamento}
            </Typography>
          </Box>
          <Chip
            label={funcionario.ativo ? 'Ativo' : 'Inativo'}
            color={funcionario.ativo ? 'success' : 'default'}
          />
        </Box>
        
        <Box display="flex" gap={1}>
          {canEdit && (
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              onClick={handleEdit}
            >
              Editar
            </Button>
          )}
          
          {canDelete && (
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteIcon />}
              onClick={handleDelete}
              disabled={deletingFuncionario}
            >
              Excluir
            </Button>
          )}
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Personal Information */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Informações Pessoais"
              avatar={<PersonIcon />}
            />
            <CardContent>
              <List>
                <ListItem>
                  <ListItemIcon>
                    <EmailIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="Email"
                    secondary={funcionario.email}
                  />
                </ListItem>
                
                <ListItem>
                  <ListItemIcon>
                    <PersonIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="CPF"
                    secondary={formatCPF(funcionario.cpf)}
                  />
                </ListItem>
                

              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Professional Information */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Informações Profissionais"
              avatar={<WorkIcon />}
            />
            <CardContent>
              <List>
                <ListItem>
                  <ListItemIcon>
                    <BusinessIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="Departamento"
                    secondary={funcionario.departamento}
                  />
                </ListItem>
                
                <ListItem>
                  <ListItemIcon>
                    <WorkIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="Cargo"
                    secondary={funcionario.cargo}
                  />
                </ListItem>
                

                

                
                {funcionario.gestor && (
                  <ListItem>
                    <ListItemIcon>
                      <PersonIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary="Gestor"
                      secondary={funcionario.gestor.nome}
                    />
                  </ListItem>
                )}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Statistics */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Estatísticas"
              avatar={<AssignmentIcon />}
            />
            <CardContent>
              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <Paper variant="outlined" sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">
                      {solicitacoes?.length || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Solicitações
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={6}>
                  <Paper variant="outlined" sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h4" color="success.main">
                      {solicitacoes?.filter(s => s.status === StatusSolicitacao.APROVADA).length || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Aprovadas
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={6}>
                  <Paper variant="outlined" sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h4" color="warning.main">
                      {solicitacoes?.filter(s => s.status === StatusSolicitacao.PENDENTE).length || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Pendentes
                    </Typography>
                  </Paper>
                </Grid>
                
                <Grid item xs={6}>
                  <Paper variant="outlined" sx={{ p: 2, textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">
                      {formatCurrency(
                        solicitacoes
                          ?.filter(s => s.status === StatusSolicitacao.APROVADA)
                          ?.reduce((total, s) => total + s.valor, 0) || 0
                      )}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Total Aprovado
                    </Typography>
                  </Paper>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Requests */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader
              title="Solicitações Recentes"
              action={
                <Button
                  size="small"
                  onClick={() => navigate(`/solicitacoes?funcionarioId=${id}`)}
                >
                  Ver Todas
                </Button>
              }
            />
            <CardContent>
              {loadingSolicitacoes ? (
                <Loading message="Carregando solicitações..." />
              ) : solicitacoes && solicitacoes.length > 0 ? (
                <List>
                  {solicitacoes.slice(0, 5).map((solicitacao, index) => (
                    <React.Fragment key={solicitacao.id}>
                      <ListItem
                        button
                        onClick={() => navigate(`/solicitacoes/${solicitacao.id}`)}
                      >
                        <ListItemText
                          primary={
                            <Box display="flex" justifyContent="space-between" alignItems="center">
                              <Typography variant="body2" noWrap>
                                {solicitacao.descricao}
                              </Typography>
                              <Chip
                                label={solicitacao.status}
                                color={getStatusColor(solicitacao.status) as any}
                                size="small"
                              />
                            </Box>
                          }
                          secondary={
                            <Box display="flex" justifyContent="space-between" alignItems="center">
                              <Typography variant="caption" color="text.secondary">
                                {formatDate(solicitacao.criadoEm)}
                              </Typography>
                              <Typography variant="caption" fontWeight="medium">
                                {formatCurrency(solicitacao.valor)}
                              </Typography>
                            </Box>
                          }
                        />
                      </ListItem>
                      {index < Math.min(solicitacoes.length - 1, 4) && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="text.secondary" textAlign="center" py={2}>
                  Nenhuma solicitação encontrada
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <ConfirmationModal />
    </Box>
  );
};

export default FuncionarioDetails;