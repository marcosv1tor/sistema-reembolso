import React from 'react';
import {
  Grid,
  Card,
  CardContent,
  CardHeader,
  Box,
  Typography,
  useTheme,
  Skeleton,
} from '@mui/material';
import {
  LineChart,
  Line,
  AreaChart,
  Area,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

import { formatCurrency } from '@/utils';

interface ChartCardProps {
  title: string;
  children: React.ReactNode;
  loading?: boolean;
  height?: number;
}

const ChartCard: React.FC<ChartCardProps> = ({
  title,
  children,
  loading = false,
  height = 300,
}) => {
  return (
    <Card sx={{ height: '100%' }}>
      <CardHeader
        title={
          <Typography variant="h6" component="div">
            {title}
          </Typography>
        }
      />
      <CardContent sx={{ pt: 0 }}>
        <Box height={height}>
          {loading ? (
            <Skeleton variant="rectangular" width="100%" height="100%" />
          ) : (
            children
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

interface CustomTooltipProps {
  active?: boolean;
  payload?: any[];
  label?: string;
  formatter?: (value: any, name: string) => [string, string];
}

const CustomTooltip: React.FC<CustomTooltipProps> = ({
  active,
  payload,
  label,
  formatter,
}) => {
  if (active && payload && payload.length) {
    return (
      <Box
        sx={{
          bgcolor: 'background.paper',
          border: 1,
          borderColor: 'divider',
          borderRadius: 1,
          p: 1.5,
          boxShadow: 2,
        }}
      >
        <Typography variant="body2" sx={{ mb: 1 }}>
          {label}
        </Typography>
        {payload.map((entry, index) => (
          <Typography
            key={index}
            variant="body2"
            sx={{ color: entry.color }}
          >
            {entry.name}: {formatter ? formatter(entry.value, entry.name)[0] : entry.value}
          </Typography>
        ))}
      </Box>
    );
  }
  return null;
};

interface DashboardChartsProps {
  data?: any;
  loading?: boolean;
}

const DashboardCharts: React.FC<DashboardChartsProps> = ({
  data,
  loading = false,
}) => {
  const theme = useTheme();

  const colors = {
    primary: theme.palette.primary.main,
    secondary: theme.palette.secondary.main,
    success: theme.palette.success.main,
    warning: theme.palette.warning.main,
    error: theme.palette.error.main,
    info: theme.palette.info.main,
  };

  // Dados para gráfico de linha - Solicitações por mês
  const solicitacoesPorMes = data?.solicitacoesPorMes || [];

  // Dados para gráfico de área - Valores por mês
  const valoresPorMes = data?.valoresPorMes || [];

  // Dados para gráfico de barras - Solicitações por departamento
  const solicitacoesPorDepartamento = data?.solicitacoesPorDepartamento || [];

  // Dados para gráfico de pizza - Status das solicitações
  const statusSolicitacoes = [
    {
      name: 'Pendentes',
      value: data?.statusDistribution?.pendentes || 0,
      color: colors.warning,
    },
    {
      name: 'Aprovadas',
      value: data?.statusDistribution?.aprovadas || 0,
      color: colors.success,
    },
    {
      name: 'Rejeitadas',
      value: data?.statusDistribution?.rejeitadas || 0,
      color: colors.error,
    },
    {
      name: 'Em Análise',
      value: data?.statusDistribution?.emAnalise || 0,
      color: colors.info,
    },
  ];

  // Dados para gráfico de barras - Top funcionários
  const topFuncionarios = data?.topFuncionarios || [];

  // Dados para gráfico de linha - Tendência de aprovação
  const tendenciaAprovacao = data?.tendenciaAprovacao || [];

  return (
    <Grid container spacing={3}>
      {/* Gráfico de Linha - Solicitações por Mês */}
      <Grid item xs={12} md={6}>
        <ChartCard title="Solicitações por Mês" loading={loading}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={solicitacoesPorMes}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis
                dataKey="mes"
                stroke={theme.palette.text.secondary}
                fontSize={12}
              />
              <YAxis stroke={theme.palette.text.secondary} fontSize={12} />
              <Tooltip
                content={<CustomTooltip />}
              />
              <Legend />
              <Line
                type="monotone"
                dataKey="total"
                stroke={colors.primary}
                strokeWidth={3}
                dot={{ fill: colors.primary, strokeWidth: 2, r: 4 }}
                name="Total"
              />
              <Line
                type="monotone"
                dataKey="aprovadas"
                stroke={colors.success}
                strokeWidth={2}
                dot={{ fill: colors.success, strokeWidth: 2, r: 3 }}
                name="Aprovadas"
              />
              <Line
                type="monotone"
                dataKey="rejeitadas"
                stroke={colors.error}
                strokeWidth={2}
                dot={{ fill: colors.error, strokeWidth: 2, r: 3 }}
                name="Rejeitadas"
              />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>

      {/* Gráfico de Pizza - Status das Solicitações */}
      <Grid item xs={12} md={6}>
        <ChartCard title="Distribuição por Status" loading={loading}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={statusSolicitacoes}
                cx="50%"
                cy="50%"
                innerRadius={60}
                outerRadius={100}
                paddingAngle={5}
                dataKey="value"
                label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                labelLine={false}
              >
                {statusSolicitacoes.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip
                content={<CustomTooltip />}
              />
            </PieChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>

      {/* Gráfico de Área - Valores por Mês */}
      <Grid item xs={12} md={8}>
        <ChartCard title="Valores Aprovados por Mês" loading={loading}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={valoresPorMes}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis
                dataKey="mes"
                stroke={theme.palette.text.secondary}
                fontSize={12}
              />
              <YAxis
                stroke={theme.palette.text.secondary}
                fontSize={12}
                tickFormatter={(value) => formatCurrency(value)}
              />
              <Tooltip
                content={
                  <CustomTooltip
                    formatter={(value) => [formatCurrency(value), '']}
                  />
                }
              />
              <Area
                type="monotone"
                dataKey="valor"
                stroke={colors.success}
                fill={colors.success}
                fillOpacity={0.3}
                strokeWidth={2}
              />
            </AreaChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>

      {/* Gráfico de Barras - Top Funcionários */}
      <Grid item xs={12} md={4}>
        <ChartCard title="Top Funcionários" loading={loading} height={300}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={topFuncionarios} layout="horizontal">
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis type="number" stroke={theme.palette.text.secondary} fontSize={12} />
              <YAxis
                type="category"
                dataKey="nome"
                stroke={theme.palette.text.secondary}
                fontSize={10}
                width={80}
              />
              <Tooltip
                content={<CustomTooltip />}
              />
              <Bar
                dataKey="total"
                fill={colors.primary}
                radius={[0, 4, 4, 0]}
                name="Solicitações"
              />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>

      {/* Gráfico de Barras - Solicitações por Departamento */}
      <Grid item xs={12} md={6}>
        <ChartCard title="Solicitações por Departamento" loading={loading}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={solicitacoesPorDepartamento}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis
                dataKey="departamento"
                stroke={theme.palette.text.secondary}
                fontSize={12}
                angle={-45}
                textAnchor="end"
                height={80}
              />
              <YAxis stroke={theme.palette.text.secondary} fontSize={12} />
              <Tooltip
                content={<CustomTooltip />}
              />
              <Legend />
              <Bar
                dataKey="pendentes"
                stackId="a"
                fill={colors.warning}
                name="Pendentes"
                radius={[0, 0, 0, 0]}
              />
              <Bar
                dataKey="aprovadas"
                stackId="a"
                fill={colors.success}
                name="Aprovadas"
                radius={[0, 0, 0, 0]}
              />
              <Bar
                dataKey="rejeitadas"
                stackId="a"
                fill={colors.error}
                name="Rejeitadas"
                radius={[4, 4, 0, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>

      {/* Gráfico de Linha - Tendência de Aprovação */}
      <Grid item xs={12} md={6}>
        <ChartCard title="Taxa de Aprovação (%)" loading={loading}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={tendenciaAprovacao}>
              <CartesianGrid strokeDasharray="3 3" stroke={theme.palette.divider} />
              <XAxis
                dataKey="mes"
                stroke={theme.palette.text.secondary}
                fontSize={12}
              />
              <YAxis
                stroke={theme.palette.text.secondary}
                fontSize={12}
                domain={[0, 100]}
                tickFormatter={(value) => `${value}%`}
              />
              <Tooltip
                content={
                  <CustomTooltip
                    formatter={(value) => [`${value}%`, '']}
                  />
                }
              />
              <Line
                type="monotone"
                dataKey="taxaAprovacao"
                stroke={colors.info}
                strokeWidth={3}
                dot={{ fill: colors.info, strokeWidth: 2, r: 4 }}
                name="Taxa de Aprovação"
              />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>
      </Grid>
    </Grid>
  );
};

export default DashboardCharts;