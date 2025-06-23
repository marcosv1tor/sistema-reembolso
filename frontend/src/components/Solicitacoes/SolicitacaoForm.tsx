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
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
} from '@mui/material';
import {
  Save as SaveIcon,
  Send as SendIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
  AttachFile as AttachFileIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { useApi } from '@/hooks/useApi';
import { apiService } from '@/services/api';
import { FormField, Loading, useConfirmationModal } from '@/components/Common';
import { useSimpleToast } from '@/components/Common';
import {
  SolicitacaoReembolso,

  TipoDespesa,
  StatusSolicitacao,
  TipoUsuario,
  Funcionario,
  Comprovante,
  CriarSolicitacaoDto,
  AtualizarSolicitacaoDto,
} from '@/types';
import {
  formatCurrency,

  isValidEmail,
  isValidFileSize,
  isValidFileExtension,
} from '@/utils';

interface SolicitacaoFormProps {
  mode: 'create' | 'edit';
}

interface FormData {
  funcionarioId: string;
  descricao: string;
  valorTotal: number;
  tipoDespesa: TipoDespesa;
  dataVencimento: Date | null;
  observacoes: string;
  comprovantes: File[];
}

interface FormErrors {
  [key: string]: string;
}

const SolicitacaoForm: React.FC<SolicitacaoFormProps> = ({ mode }) => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { usuario } = useAuth();
  const toast = useSimpleToast();
  const { showConfirmation, ConfirmationModal } = useConfirmationModal();
  
  const [activeStep, setActiveStep] = useState(0);
  const [formData, setFormData] = useState<FormData>({
    funcionarioId: usuario?.id || '',
    descricao: '',
    valorTotal: 0,
    tipoDespesa: TipoDespesa.ALIMENTACAO,
    dataVencimento: null,
    observacoes: '',
    comprovantes: [],
  });
  const [errors, setErrors] = useState<FormErrors>({});
  const [existingComprovantes, setExistingComprovantes] = useState<Comprovante[]>([]);

  // API calls
  const {
    data: solicitacao,
    loading: loadingSolicitacao,
    execute: fetchSolicitacao,
  } = useApi<SolicitacaoReembolso>(() => apiService.obterSolicitacao(id!).then(r => r.data));

  const {
    data: funcionarios,
    loading: loadingFuncionarios,
    execute: fetchFuncionarios,
  } = useApi<Funcionario[]>(() => apiService.obterFuncionarios({ page: 0, pageSize: 100 }).then((r: any) => r.data.data));

  const {
    loading: saving,
    execute: saveSolicitacao,
  } = useApi<SolicitacaoReembolso>(() => Promise.resolve({} as SolicitacaoReembolso));

  useEffect(() => {
    if (mode === 'edit' && id) {
      fetchSolicitacao();
    }
    fetchFuncionarios();
  }, [mode, id]);

  useEffect(() => {
    if (solicitacao && mode === 'edit') {
      setFormData({
        funcionarioId: solicitacao.funcionarioId,
        descricao: solicitacao.descricao,
        valorTotal: solicitacao.valor,
        tipoDespesa: solicitacao.tipoDespesa,
        dataVencimento: solicitacao.dataGasto ? new Date(solicitacao.dataGasto) : null,
        observacoes: solicitacao.observacoes || '',
        comprovantes: [],
      });
      setExistingComprovantes(solicitacao.comprovantes || []);
    }
  }, [solicitacao, mode]);

  const handleFieldChange = (name: string, value: any) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when field is updated
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.funcionarioId || formData.funcionarioId.trim() === '') {
      newErrors.funcionarioId = 'Funcionário é obrigatório';
    }

    if (!formData.descricao || formData.descricao.trim() === '') {
      newErrors.descricao = 'Descrição é obrigatória';
    }

    if (!formData.valorTotal || formData.valorTotal <= 0) {
      newErrors.valorTotal = 'Valor deve ser maior que zero';
    }

    if (!formData.tipoDespesa) {
      newErrors.tipoDespesa = 'Tipo de despesa é obrigatório';
    }

    // Validate file uploads
    formData.comprovantes.forEach((file, index) => {
      if (!isValidFileSize(file, 10)) { // 10MB max
        newErrors[`comprovante_${index}`] = `Arquivo ${file.name} é muito grande (máx. 10MB)`;
      }
      
      if (!isValidFileExtension(file.name, ['pdf', 'jpg', 'jpeg', 'png'])) {
        newErrors[`comprovante_${index}`] = `Arquivo ${file.name} tem formato inválido`;
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (activeStep === 0 && !validateBasicInfo()) return;
    setActiveStep(prev => prev + 1);
  };

  const handleBack = () => {
    setActiveStep(prev => prev - 1);
  };

  const validateBasicInfo = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.funcionarioId || formData.funcionarioId.trim() === '') {
      newErrors.funcionarioId = 'Funcionário é obrigatório';
    }

    if (!formData.descricao || formData.descricao.trim() === '') {
      newErrors.descricao = 'Descrição é obrigatória';
    }

    if (!formData.valorTotal || formData.valorTotal <= 0) {
      newErrors.valorTotal = 'Valor deve ser maior que zero';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async (submit: boolean = false) => {
    if (!validateForm()) {
      toast.error('Por favor, corrija os erros no formulário');
      return;
    }

    try {
      let result;
      if (mode === 'create') {
        const createDto: CriarSolicitacaoDto = {
          funcionarioId: formData.funcionarioId,
          descricao: formData.descricao,
          valor: formData.valorTotal,
          tipoDespesa: formData.tipoDespesa,
          dataGasto: formData.dataVencimento?.toISOString().split('T')[0] || '',
          observacoes: formData.observacoes,
        };
        result = await saveSolicitacao(() => apiService.criarSolicitacao(createDto));
      } else {
        const updateDto: AtualizarSolicitacaoDto = {
          descricao: formData.descricao,
          valor: formData.valorTotal,
          tipoDespesa: formData.tipoDespesa,
          dataGasto: formData.dataVencimento?.toISOString().split('T')[0],
          observacoes: formData.observacoes,
        };
        result = await saveSolicitacao(() => apiService.atualizarSolicitacao(id!, updateDto));
      }

      // Upload files if any
      if (formData.comprovantes.length > 0 && result) {
        const solicitacaoId = (result as SolicitacaoReembolso).id;
        for (const file of formData.comprovantes) {
          await apiService.uploadComprovante(solicitacaoId, file);
        }
      }

      toast.success(
        mode === 'create' 
          ? (submit ? 'Solicitação criada e enviada para aprovação!' : 'Solicitação criada com sucesso!')
          : 'Solicitação atualizada com sucesso!'
      );

      navigate('/solicitacoes');
    } catch (error: any) {
      toast.error(error.message || 'Erro ao salvar solicitação');
    }
  };

  const handleDeleteComprovante = (index: number, isExisting: boolean = false) => {
    if (isExisting) {
      setExistingComprovantes(prev => prev.filter((_, i) => i !== index));
    } else {
      setFormData(prev => ({
        ...prev,
        comprovantes: prev.comprovantes.filter((_, i) => i !== index),
      }));
    }
  };

  const handleCancel = () => {
    showConfirmation({
      title: 'Cancelar Edição',
      message: 'Tem certeza que deseja cancelar? Todas as alterações serão perdidas.',
      type: 'warning',
      onConfirm: () => navigate('/solicitacoes'),
    });
  };

  const funcionarioOptions = funcionarios?.map(f => ({
    value: f.id,
    label: `${f.nome} - ${f.departamento}`,
  })) || [];

  const tipoDespesaOptions = Object.values(TipoDespesa).map(tipo => ({
    value: tipo,
    label: tipo,
  }));

  const steps = [
    {
      label: 'Informações Básicas',
      description: 'Dados principais da solicitação',
    },
    {
      label: 'Comprovantes',
      description: 'Upload de documentos',
    },
    {
      label: 'Revisão',
      description: 'Confirmar dados antes de salvar',
    },
  ];

  if (mode === 'edit' && loadingSolicitacao) {
    return <Loading message="Carregando solicitação..." />;
  }

  if (mode === 'edit' && !solicitacao) {
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
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <IconButton onClick={() => navigate('/solicitacoes')}>
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" component="h1">
          {mode === 'create' ? 'Nova Solicitação' : 'Editar Solicitação'}
        </Typography>
        {mode === 'edit' && solicitacao && (
          <Chip
            label={solicitacao.status}
            color={solicitacao.status === StatusSolicitacao.APROVADA ? 'success' : 
                   solicitacao.status === StatusSolicitacao.REJEITADA ? 'error' : 'warning'}
          />
        )}
      </Box>

      {/* Form */}
      <Card>
        <CardContent>
          <Stepper activeStep={activeStep} orientation="vertical">
            {/* Step 1: Basic Information */}
            <Step>
              <StepLabel>{steps[0].label}</StepLabel>
              <StepContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {steps[0].description}
                </Typography>
                
                <Grid container spacing={3} sx={{ mt: 1 }}>
                  <Grid item xs={12} md={6}>
                    <FormField
                      name="funcionarioId"
                      label="Funcionário"
                      type="select"
                      value={formData.funcionarioId}
                      onChange={handleFieldChange}
                      options={funcionarioOptions}
                      error={errors.funcionarioId}
                      required
                      disabled={mode === 'edit' || usuario?.tipo !== TipoUsuario.ADMIN}
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <FormField
                      name="tipoDespesa"
                      label="Tipo de Despesa"
                      type="select"
                      value={formData.tipoDespesa}
                      onChange={handleFieldChange}
                      options={tipoDespesaOptions}
                      error={errors.tipoDespesa}
                      required
                    />
                  </Grid>
                  
                  <Grid item xs={12}>
                    <FormField
                      name="descricao"
                      label="Descrição"
                      type="text"
                      value={formData.descricao}
                      onChange={handleFieldChange}
                      error={errors.descricao}
                      required
                      multiline
                      rows={3}
                      placeholder="Descreva detalhadamente a despesa..."
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <FormField
                      name="valorTotal"
                      label="Valor Total"
                      type="number"
                      value={formData.valorTotal}
                      onChange={handleFieldChange}
                      error={errors.valorTotal}
                      required
                      mask="currency"
                    />
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <FormField
                      name="dataVencimento"
                      label="Data de Vencimento"
                      type="date"
                      value={formData.dataVencimento}
                      onChange={handleFieldChange}
                      error={errors.dataVencimento}
                      disablePast
                    />
                  </Grid>
                  
                  <Grid item xs={12}>
                    <FormField
                      name="observacoes"
                      label="Observações"
                      type="text"
                      value={formData.observacoes}
                      onChange={handleFieldChange}
                      error={errors.observacoes}
                      multiline
                      rows={2}
                      placeholder="Observações adicionais (opcional)..."
                    />
                  </Grid>
                </Grid>
                
                <Box sx={{ mt: 3 }}>
                  <Button
                    variant="contained"
                    onClick={handleNext}
                    sx={{ mr: 1 }}
                  >
                    Próximo
                  </Button>
                  <Button onClick={handleCancel}>
                    Cancelar
                  </Button>
                </Box>
              </StepContent>
            </Step>

            {/* Step 2: Attachments */}
            <Step>
              <StepLabel>{steps[1].label}</StepLabel>
              <StepContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {steps[1].description}
                </Typography>
                
                <Box sx={{ mt: 2 }}>
                  <FormField
                    name="comprovantes"
                    label="Adicionar Comprovantes"
                    type="file"
                    value={formData.comprovantes}
                    onChange={handleFieldChange}
                    accept=".pdf,.jpg,.jpeg,.png"
                    multiple
                    maxSize={10}
                  />
                  
                  {/* Existing attachments */}
                  {existingComprovantes.length > 0 && (
                    <Box sx={{ mt: 2 }}>
                      <Typography variant="subtitle2" gutterBottom>
                        Comprovantes Existentes:
                      </Typography>
                      <List>
                        {existingComprovantes.map((comprovante, index) => (
                          <ListItem key={index}>
                            <ListItemText
                              primary={comprovante.nomeArquivo}
                              secondary={`Tipo: ${comprovante.tipoArquivo}`}
                            />
                            <ListItemSecondaryAction>
                              <IconButton
                                edge="end"
                                onClick={() => handleDeleteComprovante(index, true)}
                              >
                                <DeleteIcon />
                              </IconButton>
                            </ListItemSecondaryAction>
                          </ListItem>
                        ))}
                      </List>
                    </Box>
                  )}
                  
                  {/* New attachments */}
                  {formData.comprovantes.length > 0 && (
                    <Box sx={{ mt: 2 }}>
                      <Typography variant="subtitle2" gutterBottom>
                        Novos Comprovantes:
                      </Typography>
                      <List>
                        {formData.comprovantes.map((file, index) => (
                          <ListItem key={index}>
                            <ListItemText
                              primary={file.name}
                              secondary={`Tamanho: ${(file.size / 1024 / 1024).toFixed(2)} MB`}
                            />
                            <ListItemSecondaryAction>
                              <IconButton
                                edge="end"
                                onClick={() => handleDeleteComprovante(index)}
                              >
                                <DeleteIcon />
                              </IconButton>
                            </ListItemSecondaryAction>
                          </ListItem>
                        ))}
                      </List>
                    </Box>
                  )}
                </Box>
                
                <Box sx={{ mt: 3 }}>
                  <Button
                    variant="contained"
                    onClick={handleNext}
                    sx={{ mr: 1 }}
                  >
                    Próximo
                  </Button>
                  <Button onClick={handleBack} sx={{ mr: 1 }}>
                    Voltar
                  </Button>
                  <Button onClick={handleCancel}>
                    Cancelar
                  </Button>
                </Box>
              </StepContent>
            </Step>

            {/* Step 3: Review */}
            <Step>
              <StepLabel>{steps[2].label}</StepLabel>
              <StepContent>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {steps[2].description}
                </Typography>
                
                <Paper variant="outlined" sx={{ p: 2, mt: 2 }}>
                  <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle2">Funcionário:</Typography>
                      <Typography variant="body2">
                        {funcionarioOptions.find(f => f.value === formData.funcionarioId)?.label || 'N/A'}
                      </Typography>
                    </Grid>
                    
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle2">Tipo de Despesa:</Typography>
                      <Typography variant="body2">{formData.tipoDespesa}</Typography>
                    </Grid>
                    
                    <Grid item xs={12}>
                      <Typography variant="subtitle2">Descrição:</Typography>
                      <Typography variant="body2">{formData.descricao}</Typography>
                    </Grid>
                    
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle2">Valor Total:</Typography>
                      <Typography variant="body2" fontWeight="medium">
                        {formatCurrency(formData.valorTotal)}
                      </Typography>
                    </Grid>
                    
                    <Grid item xs={12} md={6}>
                      <Typography variant="subtitle2">Data de Vencimento:</Typography>
                      <Typography variant="body2">
                        {formData.dataVencimento ? formData.dataVencimento.toLocaleDateString() : 'N/A'}
                      </Typography>
                    </Grid>
                    
                    {formData.observacoes && (
                      <Grid item xs={12}>
                        <Typography variant="subtitle2">Observações:</Typography>
                        <Typography variant="body2">{formData.observacoes}</Typography>
                      </Grid>
                    )}
                    
                    <Grid item xs={12}>
                      <Typography variant="subtitle2">Comprovantes:</Typography>
                      <Typography variant="body2">
                        {existingComprovantes.length + formData.comprovantes.length} arquivo(s)
                      </Typography>
                    </Grid>
                  </Grid>
                </Paper>
                
                <Box sx={{ mt: 3 }}>
                  <Button
                    variant="contained"
                    onClick={() => handleSave(false)}
                    disabled={saving}
                    startIcon={<SaveIcon />}
                    sx={{ mr: 1 }}
                  >
                    {mode === 'create' ? 'Salvar Rascunho' : 'Salvar Alterações'}
                  </Button>
                  
                  {mode === 'create' && (
                    <Button
                      variant="contained"
                      color="success"
                      onClick={() => handleSave(true)}
                      disabled={saving}
                      startIcon={<SendIcon />}
                      sx={{ mr: 1 }}
                    >
                      Salvar e Enviar
                    </Button>
                  )}
                  
                  <Button onClick={handleBack} sx={{ mr: 1 }}>
                    Voltar
                  </Button>
                  <Button onClick={handleCancel}>
                    Cancelar
                  </Button>
                </Box>
              </StepContent>
            </Step>
          </Stepper>
        </CardContent>
      </Card>

      <ConfirmationModal />
    </Box>
  );
};

export default SolicitacaoForm;