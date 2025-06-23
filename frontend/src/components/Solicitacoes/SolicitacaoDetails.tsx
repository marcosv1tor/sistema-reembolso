import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Chip,
  Button,
  Grid,
  Divider,
  Avatar,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  ListItemSecondaryAction,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Paper,
} from '@mui/material';
// Timeline components removed due to @mui/lab dependency issues
// Using alternative layout components
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Download as DownloadIcon,
  AttachFile as AttachFileIcon,
  Check as CheckIcon,
  Close as CloseIcon,
  Send as SendIcon,
  ArrowBack as ArrowBackIcon,
  Visibility as VisibilityIcon,
  Comment as CommentIcon,
  Person as PersonIcon,
  CalendarToday as CalendarIcon,
  AccountBalance as AccountBalanceIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { Loading, useConfirmationModal } from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  SolicitacaoReembolso,
  StatusSolicitacao,
  TipoUsuario,
  Comprovante,
} from '@/types';
import {
  formatCurrency,
  formatDate,
  formatDateTime,
  downloadFile,
} from '@/utils';

interface ApprovalDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (observacoes: string) => void;
  type: 'approve' | 'reject';
  loading: boolean;
}

const ApprovalDialog: React.FC<ApprovalDialogProps> = ({
  open,
  onClose,
  onConfirm,
  type,
  loading,
}) => {
  const [observacoes, setObservacoes] = useState('');

  const handleConfirm = () => {
    onConfirm(observacoes);
    setObservacoes('');
  };

  const handleClose = () => {
    setObservacoes('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {type === 'approve' ? 'Aprovar Solicitação' : 'Rejeitar Solicitação'}
      </DialogTitle>
      <DialogContent>
        <TextField
          autoFocus
          margin="dense"
          label="Observações"
          fullWidth
          multiline
          rows={4}
          variant="outlined"
          value={observacoes}
          onChange={(e) => setObservacoes(e.target.value)}
          placeholder={`Adicione observações sobre a ${type === 'approve' ? 'aprovação' : 'rejeição'}...`}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={loading}>
          Cancelar
        </Button>
        <Button
          onClick={handleConfirm}
          variant="contained"
          color={type === 'approve' ? 'success' : 'error'}
          disabled={loading}
          startIcon={type === 'approve' ? <CheckIcon /> : <CloseIcon />}
        >
          {type === 'approve' ? 'Aprovar' : 'Rejeitar'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

const SolicitacaoDetails: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [approvalDialog, setApprovalDialog] = useState<{
    open: boolean;
    type: 'approve' | 'reject';
  }>({ open: false, type: 'approve' });

  // API calls
  const {
    data: solicitacao,
    loading: loadingSolicitacao,
    execute: fetchSolicitacao,
  } = useApi<SolicitacaoReembolso>(() => apiService.obterSolicitacao(id!).then(response => response.data));

  const {
    loading: actionLoading,
    execute: executeAction,
  } = useApi(() => Promise.resolve({} as SolicitacaoReembolso));

  useEffect(() => {
    if (id) {
      fetchSolicitacao();
    }
  }, [id]);

  const canEdit = () => {
    if (!solicitacao || !usuario) return false;
    
    // Admin can edit any draft
    if (usuario.tipo === TipoUsuario.ADMIN && solicitacao.status === StatusSolicitacao.PENDENTE) {
      return true;
    }
    
    // Employee can edit their own drafts
    if (usuario.tipo === TipoUsuario.FUNCIONARIO &&
        solicitacao.funcionarioId === usuario.id && 
        solicitacao.status === StatusSolicitacao.PENDENTE) {
      return true;
    }
    
    return false;
  };

  const canDelete = () => {
    if (!solicitacao || !usuario) return false;
    
    // Admin can delete any draft
    if (usuario.tipo === TipoUsuario.ADMIN && solicitacao.status === StatusSolicitacao.PENDENTE) {
      return true;
    }
    
    // Employee can delete their own drafts
    if (usuario.tipo === TipoUsuario.FUNCIONARIO &&
        solicitacao.funcionarioId === usuario.id && 
        solicitacao.status === StatusSolicitacao.PENDENTE) {
      return true;
    }
    
    return false;
  };

  const canApprove = () => {
    if (!solicitacao || !usuario) return false;
    
    return usuario.tipo === TipoUsuario.GESTOR && 
           solicitacao.status === StatusSolicitacao.PENDENTE;
  };

  const canSubmit = () => {
    if (!solicitacao || !usuario) return false;
    
    return solicitacao.status === StatusSolicitacao.PENDENTE && 
           (usuario.tipo === TipoUsuario.ADMIN || 
            (usuario.tipo === TipoUsuario.FUNCIONARIO && solicitacao.funcionarioId === usuario.id));
  };

  const handleEdit = () => {
    navigate(`/solicitacoes/${id}/edit`);
  };

  const handleDelete = () => {
    showConfirmation({
      title: 'Excluir Solicitação',
      message: 'Tem certeza que deseja excluir esta solicitação? Esta ação não pode ser desfeita.',
      type: 'error',
      onConfirm: async () => {
        try {
          await executeAction(() => apiService.excluirSolicitacao(id!));
          toast.success('Solicitação excluída com sucesso!');
          navigate('/solicitacoes');
        } catch (error: any) {
          toast.error(error.message || 'Erro ao excluir solicitação');
        }
      },
    });
  };

  const handleSubmit = () => {
    showConfirmation({
      title: 'Enviar Solicitação',
      message: 'Tem certeza que deseja enviar esta solicitação para aprovação? Após o envio, não será possível editá-la.',
      type: 'warning',
      onConfirm: async () => {
        try {
          // TODO: Implement submit functionality when API endpoint is available
          console.log('Submit functionality not yet implemented');
          toast.success('Solicitação enviada para aprovação!');
          fetchSolicitacao();
        } catch (error: any) {
          toast.error(error.message || 'Erro ao enviar solicitação');
        }
      },
    });
  };

  const handleApproval = async (observacoes: string) => {
    try {
      if (approvalDialog.type === 'approve') {
        await executeAction(() => apiService.aprovarSolicitacao(id!, { observacoes }));
        toast.success('Solicitação aprovada com sucesso!');
      } else {
        await executeAction(() => apiService.rejeitarSolicitacao(id!, { observacoes }));
        toast.success('Solicitação rejeitada com sucesso!');
      }
      
      setApprovalDialog({ open: false, type: 'approve' });
      fetchSolicitacao();
    } catch (error: any) {
      toast.error(error.message || 'Erro ao processar solicitação');
    }
  };

  const handleDownloadComprovante = async (comprovante: Comprovante) => {
    try {
      const blob = await apiService.downloadComprovante(id!, comprovante.id);
      downloadFile(blob, comprovante.nomeArquivo);
    } catch (error: any) {
      toast.error(error.message || 'Erro ao baixar comprovante');
    }
  };

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

  const getStatusIcon = (status: StatusSolicitacao) => {
    switch (status) {
      case StatusSolicitacao.PENDENTE:
        return <SendIcon />;
      case StatusSolicitacao.APROVADA:
        return <CheckIcon />;
      case StatusSolicitacao.REJEITADA:
        return <CloseIcon />;
      case StatusSolicitacao.PAGA:
        return <AccountBalanceIcon />;
      default:
        return <EditIcon />;
    }
  };

  if (loadingSolicitacao) {
    return <Loading message="Carregando solicitação..." />;
  }

  if (!solicitacao) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="h6" color="error">
          Solicitação não encontrada
        </Typography>
        <Button
          variant="contained"
          onClick={() => navigate('/solicitacoes')}
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
          <IconButton onClick={() => navigate('/solicitacoes')}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h4" component="h1">
            Solicitação #{solicitacao.id}
          </Typography>
          <Chip
            label={solicitacao.status}
            color={getStatusColor(solicitacao.status) as any}
            icon={getStatusIcon(solicitacao.status)}
          />
        </Box>
        
        <Box display="flex" gap={1}>
          {canEdit() && (
            <Button
              variant="outlined"
              startIcon={<EditIcon />}
              onClick={handleEdit}
            >
              Editar
            </Button>
          )}
          
          {canSubmit() && (
            <Button
              variant="contained"
              color="primary"
              startIcon={<SendIcon />}
              onClick={handleSubmit}
              disabled={actionLoading}
            >
              Enviar
            </Button>
          )}
          
          {canApprove() && (
            <>
              <Button
                variant="contained"
                color="success"
                startIcon={<CheckIcon />}
                onClick={() => setApprovalDialog({ open: true, type: 'approve' })}
                disabled={actionLoading}
              >
                Aprovar
              </Button>
              <Button
                variant="contained"
                color="error"
                startIcon={<CloseIcon />}
                onClick={() => setApprovalDialog({ open: true, type: 'reject' })}
                disabled={actionLoading}
              >
                Rejeitar
              </Button>
            </>
          )}
          
          {canDelete() && (
            <Button
              variant="outlined"
              color="error"
              startIcon={<DeleteIcon />}
              onClick={handleDelete}
              disabled={actionLoading}
            >
              Excluir
            </Button>
          )}
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Main Information */}
        <Grid item xs={12} md={8}>
          <Card>
            <CardHeader
              title="Informações da Solicitação"
              avatar={<PersonIcon />}
            />
            <CardContent>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Funcionário
                  </Typography>
                  <Typography variant="body1" fontWeight="medium">
                    {solicitacao.funcionario?.nome || 'N/A'}
                  </Typography>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Departamento
                  </Typography>
                  <Typography variant="body1">
                    {solicitacao.funcionario?.departamento || 'N/A'}
                  </Typography>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Tipo de Despesa
                  </Typography>
                  <Typography variant="body1">
                    {solicitacao.tipoDespesa}
                  </Typography>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Valor Total
                  </Typography>
                  <Typography variant="h6" color="primary" fontWeight="bold">
                    {formatCurrency(solicitacao.valor)}
                  </Typography>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Descrição
                  </Typography>
                  <Typography variant="body1">
                    {solicitacao.descricao}
                  </Typography>
                </Grid>
                
                {solicitacao.observacoes && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Observações
                    </Typography>
                    <Typography variant="body1">
                      {solicitacao.observacoes}
                    </Typography>
                  </Grid>
                )}
                
                <Grid item xs={12} md={6}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Data de Criação
                  </Typography>
                  <Typography variant="body1">
                    {formatDateTime(solicitacao.criadoEm)}
                  </Typography>
                </Grid>
                
                {solicitacao.dataGasto && (
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Data do Gasto
                    </Typography>
                    <Typography variant="body1">
                      {formatDate(solicitacao.dataGasto)}
                    </Typography>
                  </Grid>
                )}
              </Grid>
            </CardContent>
          </Card>
          
          {/* Attachments */}
          {solicitacao.comprovantes && solicitacao.comprovantes.length > 0 && (
            <Card sx={{ mt: 3 }}>
              <CardHeader
                title="Comprovantes"
                avatar={<AttachFileIcon />}
              />
              <CardContent>
                <List>
                  {solicitacao.comprovantes.map((comprovante, index) => (
                    <ListItem key={index} divider={index < solicitacao.comprovantes!.length - 1}>
                      <ListItemIcon>
                        <AttachFileIcon />
                      </ListItemIcon>
                      <ListItemText
                        primary={comprovante.nomeArquivo}
                        secondary={`Tipo: ${comprovante.tipoArquivo} • Enviado em: ${formatDateTime(comprovante.criadoEm)}`}
                      />
                      <ListItemSecondaryAction>
                        <IconButton
                          edge="end"
                          onClick={() => handleDownloadComprovante(comprovante)}
                        >
                          <DownloadIcon />
                        </IconButton>
                      </ListItemSecondaryAction>
                    </ListItem>
                  ))}
                </List>
              </CardContent>
            </Card>
          )}
        </Grid>
        
        {/* Timeline */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardHeader
              title="Histórico"
              avatar={<CalendarIcon />}
            />
            <CardContent>
              <List>
                <ListItem>
                  <ListItemIcon>
                    <EditIcon color="primary" />
                  </ListItemIcon>
                  <ListItemText
                    primary="Criada"
                    secondary={`Solicitação criada - ${formatDateTime(solicitacao.criadoEm)}`}
                  />
                </ListItem>
                
                {solicitacao.criadoEm && (
                  <ListItem>
                    <ListItemIcon>
                      <SendIcon color="warning" />
                    </ListItemIcon>
                    <ListItemText
                      primary="Enviada"
                      secondary={`Enviada para aprovação - ${formatDateTime(solicitacao.criadoEm)}`}
                    />
                  </ListItem>
                )}
                
                {solicitacao.dataAprovacao && (
                  <ListItem>
                    <ListItemIcon>
                      {solicitacao.status === StatusSolicitacao.APROVADA ? <CheckIcon color="success" /> : <CloseIcon color="error" />}
                    </ListItemIcon>
                    <ListItemText
                      primary={solicitacao.status === StatusSolicitacao.APROVADA ? 'Aprovada' : 'Rejeitada'}
                      secondary={
                        <Box>
                          <Typography variant="body2">
                            {solicitacao.status === StatusSolicitacao.APROVADA ? 'Solicitação aprovada' : 'Solicitação rejeitada'} - {formatDateTime(solicitacao.dataAprovacao)}
                          </Typography>
                          {solicitacao.observacoes && (
                            <Paper variant="outlined" sx={{ p: 1, mt: 1, bgcolor: 'grey.50' }}>
                              <Typography variant="body2">
                                {solicitacao.observacoes}
                              </Typography>
                            </Paper>
                          )}
                        </Box>
                      }
                    />
                  </ListItem>
                )}
              </List>
            </CardContent>
          </Card>
          
          {/* Quick Actions */}
          <Card sx={{ mt: 3 }}>
            <CardHeader title="Ações Rápidas" />
            <CardContent>
              <Box display="flex" flexDirection="column" gap={1}>
                <Button
                  variant="outlined"
                  startIcon={<VisibilityIcon />}
                  onClick={() => navigate('/solicitacoes')}
                  fullWidth
                >
                  Ver Todas
                </Button>
                
                {usuario?.tipo === TipoUsuario.ADMIN && (
                  <Button
                    variant="outlined"
                    startIcon={<AccountBalanceIcon />}
                    onClick={() => navigate('/relatorios')}
                    fullWidth
                  >
                    Relatórios
                  </Button>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Approval Dialog */}
      <ApprovalDialog
        open={approvalDialog.open}
        onClose={() => setApprovalDialog({ open: false, type: 'approve' })}
        onConfirm={handleApproval}
        type={approvalDialog.type}
        loading={actionLoading}
      />

      <ConfirmationModal />
    </Box>
  );
};

export default SolicitacaoDetails;