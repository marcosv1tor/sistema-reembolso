import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { ptBR } from 'date-fns/locale';
import { toast } from 'react-toastify';

// Contexts
import { AuthProvider, useAuth } from './contexts/AuthContext';

// Components
import { MainLayout } from './components/Layout';
import { ToastProvider } from './components/Common';
import {
  Dashboard,
  SolicitacoesList,
  SolicitacaoForm,
  SolicitacaoDetails,
  UsuariosList,
  UsuarioForm,
  FuncionariosList,
  FuncionarioForm,
  FuncionarioDetails,
  RelatoriosList,
} from './components';

// Protected Route Component
import ProtectedRoute from './components/ProtectedRoute';

// Theme configuration
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
        },
      },
    },
  },
});

const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={ptBR}>
        <ToastProvider>
          <AuthProvider>
            <Router>
              <Routes>
                {/* Public Routes */}
                <Route path="/login" element={<LoginPage />} />
                
                {/* Protected Routes */}
                <Route
                  path="/*"
                  element={
                    <ProtectedRoute>
                      <MainLayout>
                        <Routes>
                          {/* Dashboard */}
                          <Route path="/" element={<Dashboard />} />
                          <Route path="/dashboard" element={<Dashboard />} />
                          
                          {/* Solicitações */}
                          <Route path="/solicitacoes" element={<SolicitacoesList />} />
                          <Route path="/solicitacoes/nova" element={<SolicitacaoForm mode="create" />} />
                          <Route path="/solicitacoes/:id" element={<SolicitacaoDetails />} />
                          <Route path="/solicitacoes/:id/edit" element={<SolicitacaoForm mode="edit" />} />
                          
                          {/* Usuários */}
                          <Route path="/usuarios" element={<UsuariosList />} />
                          <Route path="/usuarios/novo" element={<UsuarioForm mode="create" />} />
                          <Route path="/usuarios/:id/edit" element={<UsuarioForm mode="edit" />} />
                          
                          {/* Funcionários */}
                          <Route path="/funcionarios" element={<FuncionariosList />} />
                          <Route path="/funcionarios/novo" element={<FuncionarioForm mode="create" />} />
                          <Route path="/funcionarios/:id" element={<FuncionarioDetails />} />
                          <Route path="/funcionarios/:id/edit" element={<FuncionarioForm mode="edit" />} />
                          
                          {/* Relatórios */}
                          <Route path="/relatorios" element={<RelatoriosList />} />
                          
                          {/* Redirect */}
                          <Route path="*" element={<Navigate to="/" replace />} />
                        </Routes>
                      </MainLayout>
                    </ProtectedRoute>
                  }
                />
              </Routes>
            </Router>
          </AuthProvider>
        </ToastProvider>
      </LocalizationProvider>
    </ThemeProvider>
  );
};

// Simple Login Page Component (placeholder)
const LoginPage: React.FC = () => {
  const { isLoading, updateAuthState } = useAuth();
  const navigate = useNavigate();
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  const handleDemoLogin = async () => {
    try {
      setIsLoggingIn(true);
      // Simular login de demonstração
      const demoUser = {
        id: 'demo-user-id',
        nome: 'Usuário Demo',
        email: 'demo@exemplo.com',
        tipo: 'FUNCIONARIO' as const,
        ativo: true,
        criadoEm: new Date().toISOString(),
        atualizadoEm: new Date().toISOString()
      };
      
      const demoToken = 'demo-token-' + Date.now();
      
      // Salvar dados de demo no localStorage
      localStorage.setItem('token', demoToken);
      localStorage.setItem('usuario', JSON.stringify(demoUser));
      
      // Atualizar estado do contexto
      updateAuthState();
      
      // Simular delay de login
      await new Promise(resolve => setTimeout(resolve, 500));
      
      toast.success(`Bem-vindo(a), ${demoUser.nome}!`);
      
      // Redirecionar para dashboard
      navigate('/', { replace: true });
    } catch (error) {
      console.error('Erro no login demo:', error);
      toast.error('Erro ao fazer login de demonstração.');
    } finally {
      setIsLoggingIn(false);
    }
  };

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
      backgroundColor: '#f5f5f5'
    }}>
      <div style={{
        padding: '2rem',
        backgroundColor: 'white',
        borderRadius: '8px',
        boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
        textAlign: 'center',
        minWidth: '300px'
      }}>
        <h1>Sistema de Reembolsos</h1>
        <p>Clique no botão abaixo para acessar o sistema em modo demonstração</p>
        <button 
          onClick={handleDemoLogin}
          disabled={isLoggingIn || isLoading}
          style={{
            padding: '12px 24px',
            backgroundColor: isLoggingIn ? '#ccc' : '#1976d2',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: isLoggingIn ? 'not-allowed' : 'pointer',
            fontSize: '16px',
            fontWeight: 'bold',
            transition: 'background-color 0.3s'
          }}
        >
          {isLoggingIn ? 'Entrando...' : 'Entrar (Demo)'}
        </button>
      </div>
    </div>
  );
};

export default App;