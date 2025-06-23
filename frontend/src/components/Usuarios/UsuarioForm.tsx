import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  CardHeader,
  Divider,
  Alert,
  FormControlLabel,
  Switch,
  IconButton,
} from '@mui/material';
import {
  Save as SaveIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { FormField, Loading, useConfirmationModal } from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  Usuario,
  CriarUsuarioDto,
  TipoUsuario,
  ApiResponse,
} from '@/types';
import {
  isValidEmail,
  isValidCPF,
  formatCPF,
  formatPhone,
} from '@/utils';

// Define UpdateUsuarioDto since it doesn't exist in types
interface UpdateUsuarioDto {
  nome?: string;
  email?: string;
  tipo?: TipoUsuario;
  ativo?: boolean;
  senha?: string;
}

// Simple validation functions
const validateRequired = (value: string): boolean => {
  return value.trim().length > 0;
};

const validateEmail = (email: string): boolean => {
  return isValidEmail(email);
};

const validateCPF = (cpf: string): boolean => {
  return isValidCPF(cpf);
};

interface UsuarioFormProps {
  mode: 'create' | 'edit';
}

interface FormData {
  nome: string;
  email: string;
  cpf: string;
  telefone: string;
  tipo: TipoUsuario;
  ativo: boolean;
  senha: string;
  confirmarSenha: string;
}

interface FormErrors {
  [key: string]: string;
}

const UsuarioForm: React.FC<UsuarioFormProps> = ({ mode }) => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [formData, setFormData] = useState<FormData>({
    nome: '',
    email: '',
    cpf: '',
    telefone: '',
    tipo: TipoUsuario.FUNCIONARIO,
    ativo: true,
    senha: '',
    confirmarSenha: '',
  });
  const [errors, setErrors] = useState<FormErrors>({});

  // API calls
  const {
    data: usuarioData,
    loading: loadingUsuario,
    execute: fetchUsuario,
  } = useApi<ApiResponse<Usuario>>(() => apiService.obterUsuario(id!));

  const {
    loading: saving,
    execute: saveUsuario,
  } = useApi(() => Promise.resolve());

  useEffect(() => {
    if (mode === 'edit' && id) {
      fetchUsuario();
    }
  }, [mode, id, fetchUsuario]);

  useEffect(() => {
    if (usuarioData && usuarioData.data && mode === 'edit') {
      const user = usuarioData.data;
      setFormData({
        nome: user.nome,
        email: user.email,
        cpf: '',
        telefone: '',
        tipo: user.tipo,
        ativo: user.ativo,
        senha: '',
        confirmarSenha: '',
      });
    }
  }, [usuarioData, mode]);

  const handleFieldChange = (name: string, value: any) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when field is updated
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }

    // Format CPF and phone as user types
    if (name === 'cpf') {
      setFormData(prev => ({ ...prev, cpf: formatCPF(value) }));
    } else if (name === 'telefone') {
      setFormData(prev => ({ ...prev, telefone: formatPhone(value) }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!validateRequired(formData.nome)) {
      newErrors.nome = 'Nome é obrigatório';
    }

    if (!validateRequired(formData.email)) {
      newErrors.email = 'Email é obrigatório';
    } else if (!validateEmail(formData.email)) {
      newErrors.email = 'Email inválido';
    }

    if (!validateRequired(formData.cpf)) {
      newErrors.cpf = 'CPF é obrigatório';
    } else if (!validateCPF(formData.cpf)) {
      newErrors.cpf = 'CPF inválido';
    }

    if (!validateRequired(formData.tipo)) {
      newErrors.tipo = 'Tipo de usuário é obrigatório';
    }

    // Password validation for create mode or when changing password
    if (mode === 'create' || formData.senha) {
      if (!validateRequired(formData.senha)) {
        newErrors.senha = 'Senha é obrigatória';
      } else if (formData.senha.length < 6) {
        newErrors.senha = 'Senha deve ter pelo menos 6 caracteres';
      }

      if (!validateRequired(formData.confirmarSenha)) {
        newErrors.confirmarSenha = 'Confirmação de senha é obrigatória';
      } else if (formData.senha !== formData.confirmarSenha) {
        newErrors.confirmarSenha = 'Senhas não coincidem';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm()) {
      toast.error('Por favor, corrija os erros no formulário');
      return;
    }

    try {
      if (mode === 'create') {
        const createDto: CriarUsuarioDto = {
          nome: formData.nome,
          email: formData.email,
          senha: formData.senha,
          tipo: formData.tipo,
        };
        
        await saveUsuario(() => apiService.criarUsuario(createDto));
        toast.success('Usuário criado com sucesso!');
      } else {
        const updateDto: UpdateUsuarioDto = {
          nome: formData.nome,
          email: formData.email,
          tipo: formData.tipo,
          ativo: formData.ativo,
        };
        
        // Only include password if it's being changed
        if (formData.senha) {
          (updateDto as any).senha = formData.senha;
        }
        
        await saveUsuario(() => apiService.atualizarUsuario(id!, updateDto));
        toast.success('Usuário atualizado com sucesso!');
      }

      navigate('/usuarios');
    } catch (error: any) {
      toast.error(error.message || 'Erro ao salvar usuário');
    }
  };

  const handleCancel = () => {
    showConfirmation({
      title: 'Cancelar Edição',
      message: 'Tem certeza que deseja cancelar? Todas as alterações serão perdidas.',
      type: 'warning',
      onConfirm: () => navigate('/usuarios'),
    });
  };

  const tipoUsuarioOptions = Object.values(TipoUsuario).map(tipo => ({
    value: tipo,
    label: tipo,
  }));

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
        <Button
          variant="contained"
          onClick={() => navigate('/usuarios')}
          sx={{ mt: 2 }}
        >
          Voltar
        </Button>
      </Box>
    );
  }

  if (mode === 'edit' && loadingUsuario) {
    return <Loading message="Carregando usuário..." />;
  }

  if (mode === 'edit' && !usuario) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="h6" color="error">
          Usuário não encontrado
        </Typography>
        <Button
          variant="contained"
          onClick={() => navigate('/usuarios')}
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
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <IconButton onClick={() => navigate('/usuarios')}>
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" component="h1">
          {mode === 'create' ? 'Novo Usuário' : 'Editar Usuário'}
        </Typography>
      </Box>

      {/* Form */}
      <Card>
        <CardHeader title="Informações do Usuário" />
        <CardContent>
          <Grid container spacing={3}>
            {/* Basic Information */}
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom>
                Dados Pessoais
              </Typography>
              <Divider sx={{ mb: 2 }} />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="nome"
                label="Nome Completo"
                type="text"
                value={formData.nome}
                onChange={handleFieldChange}
                error={errors.nome}
                required
                placeholder="Digite o nome completo"
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="email"
                label="Email"
                type="email"
                value={formData.email}
                onChange={handleFieldChange}
                error={errors.email}
                required
                placeholder="usuario@exemplo.com"
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="cpf"
                label="CPF"
                type="text"
                value={formData.cpf}
                onChange={handleFieldChange}
                error={errors.cpf}
                required
                mask="cpf"
                placeholder="000.000.000-00"
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="telefone"
                label="Telefone"
                type="text"
                value={formData.telefone}
                onChange={handleFieldChange}
                error={errors.telefone}
                mask="phone"
                placeholder="(00) 00000-0000"
              />
            </Grid>

            {/* System Information */}
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                Configurações do Sistema
              </Typography>
              <Divider sx={{ mb: 2 }} />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="tipo"
                label="Tipo de Usuário"
                type="select"
                value={formData.tipo}
                onChange={handleFieldChange}
                options={tipoUsuarioOptions}
                error={errors.tipo}
                required
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Box display="flex" alignItems="center" height="100%">
                <FormControlLabel
                  control={
                    <Switch
                      checked={formData.ativo}
                      onChange={(e) => handleFieldChange('ativo', e.target.checked)}
                      color="primary"
                    />
                  }
                  label="Usuário Ativo"
                />
              </Box>
            </Grid>

            {/* Password Section */}
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                {mode === 'create' ? 'Senha' : 'Alterar Senha'}
                {mode === 'edit' && (
                  <Typography variant="body2" color="text.secondary" component="span" sx={{ ml: 1 }}>
                    (deixe em branco para manter a senha atual)
                  </Typography>
                )}
              </Typography>
              <Divider sx={{ mb: 2 }} />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="senha"
                label="Senha"
                type="password"
                value={formData.senha}
                onChange={handleFieldChange}
                error={errors.senha}
                required={mode === 'create'}
                placeholder="Digite a senha"
                showPasswordToggle
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="confirmarSenha"
                label="Confirmar Senha"
                type="password"
                value={formData.confirmarSenha}
                onChange={handleFieldChange}
                error={errors.confirmarSenha}
                required={mode === 'create' || !!formData.senha}
                placeholder="Confirme a senha"
                showPasswordToggle
              />
            </Grid>
          </Grid>

          {/* Password Requirements */}
          {(mode === 'create' || formData.senha) && (
            <Alert severity="info" sx={{ mt: 3 }}>
              <Typography variant="body2">
                <strong>Requisitos da senha:</strong>
              </Typography>
              <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                <li>Mínimo de 6 caracteres</li>
                <li>Recomendado: use letras, números e símbolos</li>
              </ul>
            </Alert>
          )}

          {/* Actions */}
          <Box display="flex" gap={2} justifyContent="flex-end" sx={{ mt: 4 }}>
            <Button
              variant="outlined"
              onClick={handleCancel}
              disabled={saving}
            >
              Cancelar
            </Button>
            <Button
              variant="contained"
              onClick={handleSave}
              disabled={saving}
              startIcon={<SaveIcon />}
            >
              {mode === 'create' ? 'Criar Usuário' : 'Salvar Alterações'}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <ConfirmationModal />
    </Box>
  );
};

export default UsuarioForm;