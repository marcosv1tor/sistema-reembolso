import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  CardHeader,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  IconButton,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
} from '@mui/material';
import {
  Download as DownloadIcon,
  Refresh as RefreshIcon,
  Assessment as AssessmentIcon,
  TrendingUp as TrendingUpIcon,
  People as PeopleIcon,
  AttachMoney as MoneyIcon,
  DateRange as DateRangeIcon,
  Business as BusinessIcon,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { ptBR } from 'date-fns/locale';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { Loading } from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  TipoUsuario,
  StatusSolicitacao,
  RelatorioTipo,
  RelatorioFiltros,
} from '@/types';
import {
  formatDate,
  formatCurrency,
  downloadFile,
} from '@/utils';

interface RelatorioItem {
  id: string;
  nome: string;
  descricao: string;
  tipo: RelatorioTipo;
  icon: React.ReactNode;
  color: string;
  disponivel: boolean;
}

const RelatoriosList: React.FC = () => {
  const { usuario } = useAuth();
  const toast = useSimpleToast();

  // State
  const [filtros, setFiltros] = useState<RelatorioFiltros>({
    dataInicio: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
    dataFim: new Date().toISOString().split('T')[0],
    departamento: '',
    status: undefined,
    funcionarioId: '',
  });

  // API calls
  const {
    data: departamentos,
    execute: fetchDepartamentos,
  } = useApi<string[]>(() => apiService.obterDepartamentos().then((response: any) => response.data || response));

  const {
    data: funcionarios,
    execute: fetchFuncionarios,
  } = useApi<any[]>(() => apiService.obterFuncionarios({ page: 0, pageSize: 1000 })
    .then((response: any) => response.items));

  const {
    loading: generatingReport,
    execute: generateReport,
  } = useApi(() => Promise.resolve(null));

  useEffect(() => {
    fetchDepartamentos();
    if (usuario?.tipo === TipoUsuario.ADMIN || usuario?.tipo === TipoUsuario.GESTOR) {
      fetchFuncionarios();
    }
  }, []);

  const relatorios: RelatorioItem[] = [
    {
      id: 'solicitacoes-periodo',
      nome: 'Solicitações por Período',
      descricao: 'Relatório detalhado de todas as solicitações em um período específico',
      tipo: RelatorioTipo.SolicitacoesPeriodo,
      icon: <AssessmentIcon />,
      color: 'primary',
      disponivel: true,
    },
    {
      id: 'aprovacoes-gestores',
      nome: 'Aprovações por Gestores',
      descricao: 'Relatório de aprovações realizadas por cada gestor',
      tipo: RelatorioTipo.AprovacoesPorGestor,
      icon: <PeopleIcon />,
      color: 'secondary',
      disponivel: usuario?.tipo === TipoUsuario.ADMIN || usuario?.tipo === TipoUsuario.GESTOR,
    },
    {
      id: 'valores-departamento',
      nome: 'Valores por Departamento',
      descricao: 'Análise de gastos por departamento',
      tipo: RelatorioTipo.ValoresPorDepartamento,
      icon: <BusinessIcon />,
      color: 'success',
      disponivel: usuario?.tipo === TipoUsuario.ADMIN,
    },
    {
      id: 'tendencias-gastos',
      nome: 'Tendências de Gastos',
      descricao: 'Análise de tendências e padrões de gastos',
      tipo: RelatorioTipo.TendenciasGastos,
      icon: <TrendingUpIcon />,
      color: 'warning',
      disponivel: usuario?.tipo === TipoUsuario.ADMIN,
    },
    {
      id: 'funcionarios-gastos',
      nome: 'Gastos por Funcionário',
      descricao: 'Relatório detalhado de gastos por funcionário',
      tipo: RelatorioTipo.GastosPorFuncionario,
      icon: <MoneyIcon />,
      color: 'info',
      disponivel: usuario?.tipo === TipoUsuario.ADMIN || usuario?.tipo === TipoUsuario.GESTOR,
    },
    {
      id: 'auditoria',
      nome: 'Auditoria de Solicitações',
      descricao: 'Relatório completo para auditoria',
      tipo: RelatorioTipo.Auditoria,
      icon: <AssessmentIcon />,
      color: 'error',
      disponivel: usuario?.tipo === TipoUsuario.ADMIN,
    },
  ];

  const handleGenerateReport = async (tipo: RelatorioTipo, formato: 'pdf' | 'excel' = 'pdf') => {
    try {
      // TODO: Implementar método generateReport no apiService
      console.log('Gerando relatório:', { tipo, formato, filtros });
      toast.error('Funcionalidade de geração de relatórios ainda não implementada');
      
      // const response = await generateReport(() => 
      //   apiService.generateReport(tipo, { ...filtros, formato })
      // ) as any;
      
      // if (response && response.url) {
      //   downloadFile(response.url, `relatorio-${tipo}-${formatDate(new Date(), 'yyyy-MM-dd')}.${formato}`);
      //   toast.success('Relatório gerado com sucesso!');
      // }
    } catch (error: any) {
      toast.error(error.message || 'Erro ao gerar relatório');
    }
  };

  const handleFilterChange = (field: keyof RelatorioFiltros, value: any) => {
    setFiltros((prev: RelatorioFiltros) => ({
      ...prev,
      [field]: value,
    }));
  };

  const relatoriosDisponiveis = relatorios.filter(r => r.disponivel);

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={ptBR}>
      <Box>
        {/* Header */}
        <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
          <Box>
            <Typography variant="h4" component="h1" gutterBottom>
              Relatórios
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Gere relatórios detalhados sobre reembolsos e gastos
            </Typography>
          </Box>
          
          <IconButton onClick={() => window.location.reload()}>
            <RefreshIcon />
          </IconButton>
        </Box>

        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardHeader
            title="Filtros"
            avatar={<DateRangeIcon />}
          />
          <CardContent>
            <Grid container spacing={3}>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Data Início"
                  value={filtros.dataInicio}
                  onChange={(date) => handleFilterChange('dataInicio', date)}
                  slots={{
                    textField: TextField
                  }}
                  slotProps={{
                    textField: {
                      fullWidth: true
                    }
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Data Fim"
                  value={filtros.dataFim}
                  onChange={(date) => handleFilterChange('dataFim', date)}
                  slots={{
                    textField: TextField
                  }}
                  slotProps={{
                    textField: {
                      fullWidth: true
                    }
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Departamento</InputLabel>
                  <Select
                    value={filtros.departamento}
                    onChange={(e) => handleFilterChange('departamento', e.target.value)}
                    label="Departamento"
                  >
                    <MenuItem value="">
                      <em>Todos</em>
                    </MenuItem>
                    {departamentos?.map((dept) => (
                      <MenuItem key={dept} value={dept}>
                        {dept}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={filtros.status}
                    onChange={(e) => handleFilterChange('status', e.target.value)}
                    label="Status"
                  >
                    <MenuItem value="">
                      <em>Todos</em>
                    </MenuItem>
                    {Object.values(StatusSolicitacao).map((status) => (
                      <MenuItem key={status} value={status}>
                        {status}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              
              {(usuario?.tipo === TipoUsuario.ADMIN || usuario?.tipo === TipoUsuario.GESTOR) && (
                <Grid item xs={12} sm={6} md={3}>
                  <FormControl fullWidth>
                    <InputLabel>Funcionário</InputLabel>
                    <Select
                      value={filtros.funcionarioId}
                      onChange={(e) => handleFilterChange('funcionarioId', e.target.value)}
                      label="Funcionário"
                    >
                      <MenuItem value="">
                        <em>Todos</em>
                      </MenuItem>
                      {funcionarios?.map((func) => (
                        <MenuItem key={func.id} value={func.id}>
                          {func.nome}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
              )}
            </Grid>
          </CardContent>
        </Card>

        {/* Reports List */}
        <Grid container spacing={3}>
          {relatoriosDisponiveis.map((relatorio) => (
            <Grid item xs={12} md={6} lg={4} key={relatorio.id}>
              <Card>
                <CardHeader
                  title={relatorio.nome}
                  avatar={
                    <Box
                      sx={{
                        bgcolor: `${relatorio.color}.main`,
                        color: 'white',
                        borderRadius: '50%',
                        width: 40,
                        height: 40,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                      }}
                    >
                      {relatorio.icon}
                    </Box>
                  }
                />
                <CardContent>
                  <Typography variant="body2" color="text.secondary" paragraph>
                    {relatorio.descricao}
                  </Typography>
                  
                  <Box display="flex" gap={1} flexWrap="wrap">
                    <Button
                      variant="contained"
                      size="small"
                      startIcon={<DownloadIcon />}
                      onClick={() => handleGenerateReport(relatorio.tipo, 'pdf')}
                      disabled={generatingReport}
                      sx={{ minWidth: 100 }}
                    >
                      PDF
                    </Button>
                    
                    <Button
                      variant="outlined"
                      size="small"
                      startIcon={<DownloadIcon />}
                      onClick={() => handleGenerateReport(relatorio.tipo, 'excel')}
                      disabled={generatingReport}
                      sx={{ minWidth: 100 }}
                    >
                      Excel
                    </Button>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {relatoriosDisponiveis.length === 0 && (
          <Card>
            <CardContent>
              <Typography variant="h6" textAlign="center" color="text.secondary">
                Nenhum relatório disponível
              </Typography>
              <Typography variant="body2" textAlign="center" color="text.secondary">
                Você não tem permissão para acessar relatórios.
              </Typography>
            </CardContent>
          </Card>
        )}

        {generatingReport && (
          <Loading
            message="Gerando relatório..."
            overlay
          />
        )}
      </Box>
    </LocalizationProvider>
  );
};

export default RelatoriosList;