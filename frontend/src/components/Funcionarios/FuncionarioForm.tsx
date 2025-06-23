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
  Funcionario,
  CriarFuncionarioDto,
  TipoUsuario,
} from '@/types';
import {
  isValidEmail,
  formatCPF,
  formatPhone,
} from '@/utils';

interface FuncionarioFormProps {
  mode: 'create' | 'edit';
}

interface FormData {
  nome: string;
  email: string;
  cpf: string;
  departamento: string;
  cargo: string;
  gestorId: string;
  ativo: boolean;
}

interface FormErrors {
  [key: string]: string;
}

const FuncionarioForm: React.FC<FuncionarioFormProps> = ({ mode }) => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [formData, setFormData] = useState<FormData>({
    nome: '',
    email: '',
    cpf: '',
    departamento: '',
    cargo: '',
    gestorId: '',
    ativo: true,
  });
  const [errors, setErrors] = useState<FormErrors>({});

  // API calls
  const {
    data: funcionario,
    loading: loadingFuncionario,
    execute: fetchFuncionario,
  } = useApi<Funcionario>(() => apiService.obterFuncionario(id!).then(response => response.data));

  const {
    data: departamentos,
    execute: fetchDepartamentos,
  } = useApi<string[]>(() => apiService.obterDepartamentos().then(response => response.data));

  const {
    data: cargos,
    execute: fetchCargos,
  } = useApi<string[]>(() => apiService.obterCargos().then(response => response.data));

  const {
    data: gestores,
    execute: fetchGestores,
  } = useApi<Funcionario[]>(() => apiService.obterFuncionarios({ ativo: true }).then(response => response.data));

  const {
    loading: saving,
    execute: saveFuncionario,
  } = useApi<void>(() => Promise.resolve());

  useEffect(() => {
    if (mode === 'edit' && id) {
      fetchFuncionario();
    }
    fetchDepartamentos();
    fetchCargos();
    fetchGestores();
  }, [mode, id]);

  useEffect(() => {
    if (funcionario && mode === 'edit') {
      setFormData({
        nome: funcionario.nome,
        email: funcionario.email,
        cpf: funcionario.cpf,
        departamento: funcionario.departamento,
        cargo: funcionario.cargo,
        gestorId: funcionario.gestorId || '',
        ativo: funcionario.ativo,
      });
    }
  }, [funcionario, mode]);

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

    if (!formData.nome.trim()) {
      newErrors.nome = 'Nome é obrigatório';
    }

    if (!formData.email.trim()) {
      newErrors.email = 'Email é obrigatório';
    } else if (!isValidEmail(formData.email)) {
      newErrors.email = 'Email inválido';
    }

    if (!formData.cpf.trim()) {
      newErrors.cpf = 'CPF é obrigatório';
    }

    if (!formData.departamento.trim()) {
      newErrors.departamento = 'Departamento é obrigatório';
    }

    if (!formData.cargo.trim()) {
      newErrors.cargo = 'Cargo é obrigatório';
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
        const createDto: CriarFuncionarioDto = {
          nome: formData.nome,
          email: formData.email,
          cpf: formData.cpf.replace(/\D/g, ''), // Remove formatting
          departamento: formData.departamento,
          cargo: formData.cargo,
          gestorId: formData.gestorId || undefined,
        };
        
        await saveFuncionario(() => apiService.criarFuncionario(createDto));
        toast.success('Funcionário criado com sucesso!');
      } else {
        const updateDto: Partial<CriarFuncionarioDto> = {
          nome: formData.nome,
          email: formData.email,
          cpf: formData.cpf.replace(/\D/g, ''), // Remove formatting
          departamento: formData.departamento,
          cargo: formData.cargo,
          gestorId: formData.gestorId || undefined,
        };
        
        await saveFuncionario(() => apiService.atualizarFuncionario(id!, updateDto));
        toast.success('Funcionário atualizado com sucesso!');
      }

      navigate('/funcionarios');
    } catch (error: any) {
      toast.error(error.message || 'Erro ao salvar funcionário');
    }
  };

  const handleCancel = () => {
    showConfirmation({
      title: 'Cancelar Edição',
      message: 'Tem certeza que deseja cancelar? Todas as alterações serão perdidas.',
      type: 'warning',
      onConfirm: () => navigate('/funcionarios'),
    });
  };

  const departamentoOptions = departamentos?.map(dept => ({
    value: dept,
    label: dept,
  })) || [];

  const cargoOptions = cargos?.map(cargo => ({
    value: cargo,
    label: cargo,
  })) || [];

  const gestorOptions = gestores?.map(gestor => ({
    value: gestor.id,
    label: `${gestor.nome} - ${gestor.departamento}`,
  })) || [];

  const canManageFuncionarios = usuario?.tipo === TipoUsuario.ADMIN;

  if (!canManageFuncionarios) {
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
          onClick={() => navigate('/funcionarios')}
          sx={{ mt: 2 }}
        >
          Voltar
        </Button>
      </Box>
    );
  }

  if (mode === 'edit' && loadingFuncionario) {
    return <Loading message="Carregando funcionário..." />;
  }

  if (mode === 'edit' && !funcionario) {
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
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <IconButton onClick={() => navigate('/funcionarios')}>
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" component="h1">
          {mode === 'create' ? 'Novo Funcionário' : 'Editar Funcionário'}
        </Typography>
      </Box>

      {/* Form */}
      <Card>
        <CardHeader title="Informações do Funcionário" />
        <CardContent>
          <Grid container spacing={3}>
            {/* Personal Information */}
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
                placeholder="funcionario@empresa.com"
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
            


            {/* Professional Information */}
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
                Informações Profissionais
              </Typography>
              <Divider sx={{ mb: 2 }} />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="departamento"
                label="Departamento"
                type="select"
                value={formData.departamento}
                onChange={handleFieldChange}
                options={departamentoOptions}
                error={errors.departamento}
                required
              />
            </Grid>
            
            <Grid item xs={12} md={6}>
              <FormField
                name="cargo"
                label="Cargo"
                type="select"
                value={formData.cargo}
                onChange={handleFieldChange}
                options={cargoOptions}
                error={errors.cargo}
                required
              />
            </Grid>
            

            
            <Grid item xs={12} md={6}>
              <FormField
                name="gestorId"
                label="Gestor"
                type="select"
                value={formData.gestorId}
                onChange={handleFieldChange}
                options={[
                  { value: '', label: 'Nenhum gestor' },
                  ...gestorOptions,
                ]}
                error={errors.gestorId}
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
                  label="Funcionário Ativo"
                />
              </Box>
            </Grid>
          </Grid>

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
              {mode === 'create' ? 'Criar Funcionário' : 'Salvar Alterações'}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <ConfirmationModal />
    </Box>
  );
};

export default FuncionarioForm;