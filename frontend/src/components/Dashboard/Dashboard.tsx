import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Chip,
  Avatar,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  ListItemSecondaryAction,
  Divider,
  useTheme,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  MoreVert as MoreVertIcon,
  TrendingUp,
  Schedule,
  Warning,
  CheckCircle,
  Person,
  Receipt,
  Notifications,
} from '@mui/icons-material';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { SolicitacaoReembolso, StatusSolicitacao } from '@/types';
import { formatCurrency, formatDate } from '@/utils';
import { Loading } from '@/components/Common';
import DashboardStats from './DashboardStats';
import DashboardCharts from './DashboardCharts';

interface RecentActivityItem {
  id: string;
  type: 'solicitacao' | 'aprovacao' | 'rejeicao';
  title: string;
  description: string;
  timestamp: Date;
  user: string;
  avatar?: string;
}

const Dashboard: React.FC = () => {
  const { usuario } = useAuth();
  const theme = useTheme();
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null);

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
  
  // API calls
  const {
    data: statsData,
    loading: statsLoading,
    error: statsError,
    execute: fetchStats,
  } = useApi<any>(() => apiService.obterEstatisticasDashboard());

  // Charts data will be derived from stats data
  const chartsData = statsData;
  const chartsLoading = statsLoading;
  const chartsError = statsError;
  const fetchCharts = fetchStats;

  const {
    data: recentSolicitacoes,
    loading: recentLoading,
    execute: fetchRecent,
  } = useApi<SolicitacaoReembolso[]>(() => 
    apiService.obterSolicitacoes({ page: 0, pageSize: 5 })
      .then(response => response.data)
  );

  useEffect(() => {
    fetchStats();
    fetchCharts();
    fetchRecent();
  }, []);

  const handleRefresh = () => {
    fetchStats();
    fetchCharts();
    fetchRecent();
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setMenuAnchor(event.currentTarget);
  };

  const handleMenuClose = () => {
    setMenuAnchor(null);
  };

  const getGreeting = () => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Bom dia';
    if (hour < 18) return 'Boa tarde';
    return 'Boa noite';
  };

  const getStatusIcon = (status: StatusSolicitacao) => {
    switch (status) {
      case StatusSolicitacao.PENDENTE:
        return <Schedule sx={{ color: 'warning.main' }} />;
      case StatusSolicitacao.APROVADA:
        return <CheckCircle sx={{ color: 'success.main' }} />;
      case StatusSolicitacao.REJEITADA:
        return <Warning sx={{ color: 'error.main' }} />;
      case StatusSolicitacao.PAGA:
        return <Schedule sx={{ color: 'info.main' }} />;
      default:
        return <Schedule sx={{ color: 'warning.main' }} />;
    }
  };

  const generateRecentActivity = (): RecentActivityItem[] => {
    if (!recentSolicitacoes) return [];
    
    return recentSolicitacoes.slice(0, 5).map(solicitacao => ({
      id: solicitacao.id,
      type: 'solicitacao',
      title: `Solicitação #${solicitacao.id}`,
      description: `${solicitacao.descricao} - ${formatCurrency(solicitacao.valor)}`,
      timestamp: new Date(solicitacao.criadoEm),
      user: solicitacao.funcionario?.nome || 'Usuário',
      avatar: solicitacao.funcionario?.email?.charAt(0).toUpperCase(),
    }));
  };

  const recentActivity = generateRecentActivity();

  if (statsError || chartsError) {
    return (
      <Box
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        minHeight="400px"
        gap={2}
      >
        <Warning color="error" sx={{ fontSize: 48 }} />
        <Typography variant="h6" color="error">
          Erro ao carregar dados do dashboard
        </Typography>
        <Button variant="contained" onClick={handleRefresh} startIcon={<RefreshIcon />}>
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
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            {getGreeting()}, {usuario?.nome?.split(' ')[0] || 'Usuário'}!
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Aqui está um resumo das atividades de reembolso
          </Typography>
        </Box>
        
        <Box display="flex" gap={1}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={statsLoading || chartsLoading}
          >
            Atualizar
          </Button>
          <IconButton onClick={handleMenuOpen}>
            <MoreVertIcon />
          </IconButton>
          <Menu
            anchorEl={menuAnchor}
            open={Boolean(menuAnchor)}
            onClose={handleMenuClose}
          >
            <MenuItem onClick={handleMenuClose}>
              Exportar Relatório
            </MenuItem>
            <MenuItem onClick={handleMenuClose}>
              Configurações
            </MenuItem>
          </Menu>
        </Box>
      </Box>

      {/* Stats Cards */}
      <Box mb={4}>
        <DashboardStats data={statsData} loading={statsLoading} />
      </Box>

      {/* Charts and Recent Activity */}
      <Grid container spacing={3}>
        {/* Charts */}
        <Grid item xs={12} lg={8}>
          <DashboardCharts data={chartsData} loading={chartsLoading} />
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} lg={4}>
          <Grid container spacing={3}>
            {/* Recent Requests */}
            <Grid item xs={12}>
              <Card>
                <CardHeader
                  title="Solicitações Recentes"
                  action={
                    <IconButton size="small">
                      <Receipt />
                    </IconButton>
                  }
                />
                <CardContent sx={{ pt: 0 }}>
                  {recentLoading ? (
                    <Loading size={30} />
                  ) : recentSolicitacoes && recentSolicitacoes.length > 0 ? (
                    <List disablePadding>
                      {recentSolicitacoes.slice(0, 5).map((solicitacao, index) => (
                        <React.Fragment key={solicitacao.id}>
                          <ListItem disablePadding>
                            <ListItemAvatar>
                              <Avatar sx={{ width: 32, height: 32, fontSize: '0.875rem' }}>
                                {solicitacao.funcionario?.nome?.charAt(0) || 'U'}
                              </Avatar>
                            </ListItemAvatar>
                            <ListItemText
                              primary={
                                <Box display="flex" alignItems="center" gap={1}>
                                  <Typography variant="body2" noWrap>
                                    #{solicitacao.id}
                                  </Typography>
                                  <Chip
                                    label={solicitacao.status}
                                    size="small"
                                    color={getStatusColor(solicitacao.status)}
                                    variant="outlined"
                                  />
                                </Box>
                              }
                              secondary={
                                <Box>
                                  <Typography variant="caption" color="text.secondary">
                                    {formatCurrency(solicitacao.valor)}
                                  </Typography>
                                  <br />
                                  <Typography variant="caption" color="text.secondary">
                                    {formatDate(solicitacao.criadoEm)}
                                  </Typography>
                                </Box>
                              }
                            />
                            <ListItemSecondaryAction>
                              {getStatusIcon(solicitacao.status)}
                            </ListItemSecondaryAction>
                          </ListItem>
                          {index < recentSolicitacoes.length - 1 && <Divider />}
                        </React.Fragment>
                      ))}
                    </List>
                  ) : (
                    <Typography variant="body2" color="text.secondary" textAlign="center" py={2}>
                      Nenhuma solicitação recente
                    </Typography>
                  )}
                </CardContent>
              </Card>
            </Grid>

            {/* Quick Actions */}
            <Grid item xs={12}>
              <Card>
                <CardHeader
                  title="Ações Rápidas"
                  action={
                    <IconButton size="small">
                      <TrendingUp />
                    </IconButton>
                  }
                />
                <CardContent sx={{ pt: 0 }}>
                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <Button
                        variant="outlined"
                        fullWidth
                        startIcon={<Receipt />}
                        href="/solicitacoes/nova"
                      >
                        Nova Solicitação
                      </Button>
                    </Grid>
                    <Grid item xs={12}>
                      <Button
                        variant="outlined"
                        fullWidth
                        startIcon={<Schedule />}
                        href="/solicitacoes?status=pendente"
                      >
                        Pendentes de Aprovação
                      </Button>
                    </Grid>
                    <Grid item xs={12}>
                      <Button
                        variant="outlined"
                        fullWidth
                        startIcon={<Person />}
                        href="/funcionarios"
                      >
                        Gerenciar Funcionários
                      </Button>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>

            {/* Notifications */}
            <Grid item xs={12}>
              <Card>
                <CardHeader
                  title="Notificações"
                  action={
                    <IconButton size="small">
                      <Notifications />
                    </IconButton>
                  }
                />
                <CardContent sx={{ pt: 0 }}>
                  <List disablePadding>
                    <ListItem disablePadding>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'warning.main', width: 32, height: 32 }}>
                          <Warning fontSize="small" />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary="Solicitações Vencidas"
                        secondary={`${statsData?.solicitacoesVencidas || 0} solicitações precisam de atenção`}
                      />
                    </ListItem>
                    <Divider />
                    <ListItem disablePadding>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'info.main', width: 32, height: 32 }}>
                          <Schedule fontSize="small" />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary="Tempo Médio"
                        secondary={`${statsData?.tempoMedioAprovacao || 0} dias para aprovação`}
                      />
                    </ListItem>
                  </List>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;