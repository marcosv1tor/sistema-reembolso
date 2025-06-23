import React from 'react';
import {
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  FormHelperText,
  Checkbox,
  FormControlLabel,
  RadioGroup,
  Radio,
  Switch,
  Autocomplete,
  Chip,
  Box,
  InputAdornment,
  IconButton,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  CalendarToday,
  AttachFile,
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { ptBR } from 'date-fns/locale';
import { formatCurrency, formatCPF, formatPhone } from '@/utils';

export interface Option {
  value: string | number;
  label: string;
  disabled?: boolean;
}

interface BaseFieldProps {
  name: string;
  label: string;
  value?: any;
  onChange: (name: string, value: any) => void;
  error?: string;
  disabled?: boolean;
  required?: boolean;
  fullWidth?: boolean;
  size?: 'small' | 'medium';
  variant?: 'outlined' | 'filled' | 'standard';
  helperText?: string;
}

interface TextFieldProps extends BaseFieldProps {
  type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url';
  placeholder?: string;
  multiline?: boolean;
  rows?: number;
  maxLength?: number;
  mask?: 'cpf' | 'phone' | 'currency';
  showPasswordToggle?: boolean;
}

interface SelectFieldProps extends BaseFieldProps {
  type: 'select';
  options: Option[];
  multiple?: boolean;
}

interface AutocompleteFieldProps extends BaseFieldProps {
  type: 'autocomplete';
  options: Option[];
  multiple?: boolean;
  freeSolo?: boolean;
  loading?: boolean;
}

interface CheckboxFieldProps extends BaseFieldProps {
  type: 'checkbox';
  color?: 'primary' | 'secondary' | 'default';
}

interface SwitchFieldProps extends BaseFieldProps {
  type: 'switch';
  color?: 'primary' | 'secondary' | 'default';
}

interface RadioFieldProps extends BaseFieldProps {
  type: 'radio';
  options: Option[];
  row?: boolean;
}

interface DateFieldProps extends BaseFieldProps {
  type: 'date';
  minDate?: Date;
  maxDate?: Date;
  disablePast?: boolean;
  disableFuture?: boolean;
}

interface FileFieldProps extends BaseFieldProps {
  type: 'file';
  accept?: string;
  multiple?: boolean;
  maxSize?: number; // in MB
}

type FormFieldProps =
  | TextFieldProps
  | SelectFieldProps
  | AutocompleteFieldProps
  | CheckboxFieldProps
  | SwitchFieldProps
  | RadioFieldProps
  | DateFieldProps
  | FileFieldProps;

const FormField: React.FC<FormFieldProps> = (props) => {
  const [showPassword, setShowPassword] = React.useState(false);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  const {
    name,
    label,
    value,
    onChange,
    error,
    disabled = false,
    required = false,
    fullWidth = true,
    size = 'medium',
    variant = 'outlined',
    helperText,
  } = props;

  const handleChange = (newValue: any) => {
    onChange(name, newValue);
  };

  const applyMask = (value: string, mask?: string) => {
    if (!mask || !value) return value;
    
    switch (mask) {
      case 'cpf':
        return formatCPF(value);
      case 'phone':
        return formatPhone(value);
      case 'currency':
        return formatCurrency(parseFloat(value) || 0);
      default:
        return value;
    }
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files) return;

    const fileProps = props as FileFieldProps;
    const maxSize = fileProps.maxSize || 10; // 10MB default
    
    const validFiles = Array.from(files).filter(file => {
      if (file.size > maxSize * 1024 * 1024) {
        alert(`Arquivo ${file.name} é muito grande. Tamanho máximo: ${maxSize}MB`);
        return false;
      }
      return true;
    });

    if (fileProps.multiple) {
      handleChange(validFiles);
    } else {
      handleChange(validFiles[0] || null);
    }
  };

  switch (props.type) {
    case 'text':
    case 'email':
    case 'password':
    case 'number':
    case 'tel':
    case 'url': {
      const textProps = props as TextFieldProps;
      const inputType = props.type === 'password' && showPassword ? 'text' : props.type;
      
      return (
        <TextField
          name={name}
          label={label}
          type={inputType}
          value={textProps.mask ? applyMask(value || '', textProps.mask) : (value || '')}
          onChange={(e) => {
            let newValue = e.target.value;
            if (textProps.maxLength && newValue.length > textProps.maxLength) {
              newValue = newValue.slice(0, textProps.maxLength);
            }
            handleChange(newValue);
          }}
          error={Boolean(error)}
          helperText={error || helperText}
          disabled={disabled}
          required={required}
          fullWidth={fullWidth}
          size={size}
          variant={variant}
          placeholder={textProps.placeholder}
          multiline={textProps.multiline}
          rows={textProps.rows}
          InputProps={{
            endAdornment: props.type === 'password' && textProps.showPasswordToggle && (
              <InputAdornment position="end">
                <IconButton
                  onClick={() => setShowPassword(!showPassword)}
                  edge="end"
                >
                  {showPassword ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              </InputAdornment>
            ),
          }}
        />
      );
    }

    case 'select': {
      const selectProps = props as SelectFieldProps;
      
      return (
        <FormControl
          fullWidth={fullWidth}
          size={size}
          variant={variant}
          error={Boolean(error)}
          disabled={disabled}
          required={required}
        >
          <InputLabel>{label}</InputLabel>
          <Select
            name={name}
            value={value || (selectProps.multiple ? [] : '')}
            onChange={(e) => handleChange(e.target.value)}
            label={label}
            multiple={selectProps.multiple}
            renderValue={selectProps.multiple ? (selected) => (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {(selected as string[]).map((val) => {
                  const option = selectProps.options.find(opt => opt.value === val);
                  return (
                    <Chip key={val} label={option?.label || val} size="small" />
                  );
                })}
              </Box>
            ) : undefined}
          >
            {selectProps.options.map((option) => (
              <MenuItem
                key={option.value}
                value={option.value}
                disabled={option.disabled}
              >
                {option.label}
              </MenuItem>
            ))}
          </Select>
          {(error || helperText) && (
            <FormHelperText>{error || helperText}</FormHelperText>
          )}
        </FormControl>
      );
    }

    case 'autocomplete': {
      const autocompleteProps = props as AutocompleteFieldProps;
      
      return (
        <Autocomplete
          options={autocompleteProps.options}
          getOptionLabel={(option) => 
            typeof option === 'string' ? option : option.label
          }
          value={value || (autocompleteProps.multiple ? [] : null)}
          onChange={(_, newValue) => handleChange(newValue)}
          multiple={autocompleteProps.multiple}
          freeSolo={autocompleteProps.freeSolo}
          loading={autocompleteProps.loading}
          disabled={disabled}
          renderInput={(params) => (
            <TextField
              {...params}
              name={name}
              label={label}
              error={Boolean(error)}
              helperText={error || helperText}
              required={required}
              fullWidth={fullWidth}
              size={size}
              variant={variant}
            />
          )}
          renderTags={autocompleteProps.multiple ? (value, getTagProps) =>
            value.map((option, index) => (
              <Chip
                {...getTagProps({ index })}
                key={typeof option === 'string' ? option : option.value}
                label={typeof option === 'string' ? option : option.label}
                size="small"
              />
            ))
          : undefined}
        />
      );
    }

    case 'checkbox': {
      const checkboxProps = props as CheckboxFieldProps;
      
      return (
        <FormControlLabel
          control={
            <Checkbox
              name={name}
              checked={Boolean(value)}
              onChange={(e) => handleChange(e.target.checked)}
              disabled={disabled}
              required={required}
              color={checkboxProps.color}
            />
          }
          label={label}
        />
      );
    }

    case 'switch': {
      const switchProps = props as SwitchFieldProps;
      
      return (
        <FormControlLabel
          control={
            <Switch
              name={name}
              checked={Boolean(value)}
              onChange={(e) => handleChange(e.target.checked)}
              disabled={disabled}
              color={switchProps.color}
            />
          }
          label={label}
        />
      );
    }

    case 'radio': {
      const radioProps = props as RadioFieldProps;
      
      return (
        <FormControl
          component="fieldset"
          error={Boolean(error)}
          disabled={disabled}
          required={required}
        >
          <RadioGroup
            name={name}
            value={value || ''}
            onChange={(e) => handleChange(e.target.value)}
            row={radioProps.row}
          >
            {radioProps.options.map((option) => (
              <FormControlLabel
                key={option.value}
                value={option.value}
                control={<Radio />}
                label={option.label}
                disabled={option.disabled}
              />
            ))}
          </RadioGroup>
          {(error || helperText) && (
            <FormHelperText>{error || helperText}</FormHelperText>
          )}
        </FormControl>
      );
    }

    case 'date': {
      const dateProps = props as DateFieldProps;
      
      return (
        <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={ptBR}>
          <DatePicker
            label={label}
            value={value || null}
            onChange={(newValue) => handleChange(newValue)}
            disabled={disabled}
            minDate={dateProps.minDate}
            maxDate={dateProps.maxDate}
            disablePast={dateProps.disablePast}
            disableFuture={dateProps.disableFuture}
            slotProps={{
              textField: {
                name,
                error: Boolean(error),
                helperText: error || helperText,
                required,
                fullWidth,
                size,
                variant,
                InputProps: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <CalendarToday />
                    </InputAdornment>
                  ),
                },
              },
            }}
          />
        </LocalizationProvider>
      );
    }

    case 'file': {
      const fileProps = props as FileFieldProps;
      
      return (
        <Box>
          <input
            ref={fileInputRef}
            type="file"
            accept={fileProps.accept}
            multiple={fileProps.multiple}
            onChange={handleFileChange}
            style={{ display: 'none' }}
          />
          <TextField
            name={name}
            label={label}
            value={
              fileProps.multiple
                ? value?.length ? `${value.length} arquivo(s) selecionado(s)` : ''
                : value?.name || ''
            }
            onClick={() => fileInputRef.current?.click()}
            error={Boolean(error)}
            helperText={error || helperText}
            disabled={disabled}
            required={required}
            fullWidth={fullWidth}
            size={size}
            variant={variant}
            InputProps={{
              readOnly: true,
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton onClick={() => fileInputRef.current?.click()}>
                    <AttachFile />
                  </IconButton>
                </InputAdornment>
              ),
            }}
            sx={{ cursor: 'pointer' }}
          />
        </Box>
      );
    }

    default:
      return null;
  }
};

export default FormField;