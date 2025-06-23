import React from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  Avatar,
  Skeleton,
  useTheme,
  alpha,
} from '@mui/material';
import {
  TrendingUp,
  TrendingDown,
  Receipt,
  PendingActions,
  CheckCircle,
  Cancel,
  AttachMoney,
  People,
  Schedule,
  Warning,
} from '@mui/icons-material';
import { formatCurrency } from '@/utils';


interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactElement;
  color: string;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  loading?: boolean;
}

const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon,
  color,
  trend,
  loading = false,
}) => {
  const theme = useTheme();

  if (loading) {
    return (
      <Card>
        <CardContent>
          <Box display="flex" alignItems="center" justifyContent="space-between">
            <Box flex={1}>
              <Skeleton variant="text" width="60%" />
              <Skeleton variant="text" width="40%" height={32} />
              <Skeleton variant="text" width="50%" />
            </Box>
            <Skeleton variant="circular" width={56} height={56} />
          </Box>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card
      sx={
        {
          height: '100%',
          transition: 'all 0.3s ease-in-out',
          '&:hover': {
            transform: 'translateY(-4px)',
            boxShadow: theme.shadows[8],
          },
        }
      }
    >
      <CardContent>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Box flex={1}>
            <Typography
              variant="body2"
              color="text.secondary"
              gutterBottom
              sx={{ fontWeight: 500 }}
            >
              {title}
            </Typography>
            <Typography
              variant="h4"
              component="div"
              sx={{
                fontWeight: 700,
                color: 'text.primary',
                mb: 1,
              }}
            >
              {typeof value === 'number' && title.toLowerCase().includes('valor')
                ? formatCurrency(value)
                : value}
            </Typography>
            {trend && (
              <Box display="flex" alignItems="center" gap={0.5}>
                {trend.isPositive ? (
                  <TrendingUp
                    fontSize="small"
                    sx={{ color: 'success.main' }}
                  />
                ) : (
                  <TrendingDown
                    fontSize="small"
                    sx={{ color: 'error.main' }}
                  />
                )}
                <Typography
                  variant="caption"
                  sx={{
                    color: trend.isPositive ? 'success.main' : 'error.main',
                    fontWeight: 600,
                  }}
                >
                  {Math.abs(trend.value)}%
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  vs mês anterior
                </Typography>
              </Box>
            )}
          </Box>
          <Avatar
            sx={{
              bgcolor: alpha(color, 0.1),
              color: color,
              width: 56,
              height: 56,
            }}
          >
            {icon}
          </Avatar>
        </Box>
      </CardContent>
    </Card>
  );
};

interface DashboardStatsProps {
  data?: any;
  loading?: boolean;
}

const DashboardStats: React.FC<DashboardStatsProps> = ({
  data,
  loading = false,
}) => {
  const theme = useTheme();

  const stats = [
    {
      title: 'Total de Solicitações',
      value: data?.totalSolicitacoes || 0,
      icon: <Receipt />,
      color: theme.palette.primary.main,
      trend: data?.tendenciaSolicitacoes
        ? {
            value: Math.abs(data.tendenciaSolicitacoes),
            isPositive: data.tendenciaSolicitacoes > 0,
          }
        : undefined,
    },
    {
      title: 'Pendentes',
      value: data?.solicitacoesPendentes || 0,
      icon: <PendingActions />,
      color: theme.palette.warning.main,
      trend: data?.tendenciaPendentes
        ? {
            value: Math.abs(data.tendenciaPendentes),
            isPositive: data.tendenciaPendentes < 0, // Menos pendentes é melhor
          }
        : undefined,
    },
    {
      title: 'Aprovadas',
      value: data?.solicitacoesAprovadas || 0,
      icon: <CheckCircle />,
      color: theme.palette.success.main,
      trend: data?.tendenciaAprovadas
        ? {
            value: Math.abs(data.tendenciaAprovadas),
            isPositive: data.tendenciaAprovadas > 0,
          }
        : undefined,
    },
    {
      title: 'Rejeitadas',
      value: data?.solicitacoesRejeitadas || 0,
      icon: <Cancel />,
      color: theme.palette.error.main,
      trend: data?.tendenciaRejeitadas
        ? {
            value: Math.abs(data.tendenciaRejeitadas),
            isPositive: data.tendenciaRejeitadas < 0, // Menos rejeitadas é melhor
          }
        : undefined,
    },
    {
      title: 'Valor Total Aprovado',
      value: data?.valorTotalAprovado || 0,
      icon: <AttachMoney />,
      color: theme.palette.info.main,
      trend: data?.tendenciaValorAprovado
        ? {
            value: Math.abs(data.tendenciaValorAprovado),
            isPositive: data.tendenciaValorAprovado > 0,
          }
        : undefined,
    },
    {
      title: 'Funcionários Ativos',
      value: data?.funcionariosAtivos || 0,
      icon: <People />,
      color: theme.palette.secondary.main,
    },
    {
      title: 'Tempo Médio Aprovação',
      value: data?.tempoMedioAprovacao
        ? `${data.tempoMedioAprovacao} dias`
        : '0 dias',
      icon: <Schedule />,
      color: theme.palette.info.main,
    },
    {
      title: 'Solicitações Vencidas',
      value: data?.solicitacoesVencidas || 0,
      icon: <Warning />,
      color: theme.palette.error.main,
    },
  ];

  return (
    <Grid container spacing={3}>
      {stats.map((stat, index) => (
        <Grid item xs={12} sm={6} md={4} lg={3} key={index}>
          <StatCard
            title={stat.title}
            value={stat.value}
            icon={stat.icon}
            color={stat.color}
            trend={stat.trend}
            loading={loading}
          />
        </Grid>
      ))}
    </Grid>
  );
};

export default DashboardStats;