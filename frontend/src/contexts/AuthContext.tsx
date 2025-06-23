import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { toast } from 'react-toastify';
import { Usuario, AuthContextType } from '@/types';
import apiService from '@/services/api';

interface AuthProviderProps {
  children: ReactNode;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
};

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Verificar se há token salvo no localStorage
    const savedToken = localStorage.getItem('token');
    const savedUsuario = localStorage.getItem('usuario');

    if (savedToken && savedUsuario) {
      try {
        const parsedUsuario = JSON.parse(savedUsuario);
        setToken(savedToken);
        setUsuario(parsedUsuario);
      } catch (error) {
        console.error('Erro ao parsear dados do usuário:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('usuario');
      }
    }
    
    setIsLoading(false);
  }, []);

  // Função para atualizar estado quando dados são salvos externamente
  const updateAuthState = () => {
    const savedToken = localStorage.getItem('token');
    const savedUsuario = localStorage.getItem('usuario');

    if (savedToken && savedUsuario) {
      try {
        const parsedUsuario = JSON.parse(savedUsuario);
        setToken(savedToken);
        setUsuario(parsedUsuario);
      } catch (error) {
        console.error('Erro ao parsear dados do usuário:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('usuario');
      }
    }
  };

  // Listener para mudanças no localStorage
  useEffect(() => {
    const handleStorageChange = () => {
      updateAuthState();
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const login = async (email: string, senha: string): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await apiService.login({ email, senha });
      
      const { token: newToken, usuario: newUsuario } = response;
      
      // Salvar no localStorage
      localStorage.setItem('token', newToken);
      localStorage.setItem('usuario', JSON.stringify(newUsuario));
      
      // Atualizar estado
      setToken(newToken);
      setUsuario(newUsuario);
      
      toast.success(`Bem-vindo(a), ${newUsuario.nome}!`);
    } catch (error: any) {
      console.error('Erro no login:', error);
      
      if (error.response?.status === 401) {
        toast.error('Email ou senha incorretos.');
      } else if (error.response?.status === 403) {
        toast.error('Usuário inativo. Entre em contato com o administrador.');
      } else {
        toast.error('Erro ao fazer login. Tente novamente.');
      }
      
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = (): void => {
    // Remover do localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    
    // Limpar estado
    setToken(null);
    setUsuario(null);
    
    toast.info('Logout realizado com sucesso.');
  };

  const isAuthenticated = !!token && !!usuario;

  const value: AuthContextType = {
    usuario,
    token,
    login,
    logout,
    isAuthenticated,
    isLoading,
    updateAuthState,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;