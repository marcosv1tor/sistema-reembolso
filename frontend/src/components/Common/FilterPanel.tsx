import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Collapse,
  Grid,
  Chip,
  Divider,
  Badge,
} from '@mui/material';
import {
  FilterList as FilterIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
} from '@mui/icons-material';
import FormField, { Option } from './FormField';

export interface FilterField {
  name: string;
  label: string;
  type: 'text' | 'select' | 'date' | 'autocomplete' | 'number';
  options?: Option[];
  placeholder?: string;
  multiple?: boolean;
  freeSolo?: boolean;
  gridSize?: number; // 1-12 for Material-UI Grid
}

interface FilterPanelProps {
  fields: FilterField[];
  values: Record<string, any>;
  onChange: (name: string, value: any) => void;
  onApply?: () => void;
  onClear?: () => void;
  loading?: boolean;
  collapsible?: boolean;
  defaultExpanded?: boolean;
  showApplyButton?: boolean;
  showClearButton?: boolean;
  title?: string;
  activeFiltersCount?: number;
}

const FilterPanel: React.FC<FilterPanelProps> = ({
  fields,
  values,
  onChange,
  onApply,
  onClear,
  loading = false,
  collapsible = true,
  defaultExpanded = false,
  showApplyButton = true,
  showClearButton = true,
  title = 'Filtros',
  activeFiltersCount,
}) => {
  const [expanded, setExpanded] = useState(defaultExpanded);

  const handleToggleExpanded = () => {
    setExpanded(!expanded);
  };

  const handleClear = () => {
    fields.forEach(field => {
      onChange(field.name, field.multiple ? [] : '');
    });
    onClear?.();
  };

  const getActiveFiltersCount = () => {
    if (activeFiltersCount !== undefined) {
      return activeFiltersCount;
    }

    return fields.filter(field => {
      const value = values[field.name];
      if (Array.isArray(value)) {
        return value.length > 0;
      }
      return value !== undefined && value !== null && value !== '';
    }).length;
  };

  const renderFilterChips = () => {
    const activeFilters = fields.filter(field => {
      const value = values[field.name];
      if (Array.isArray(value)) {
        return value.length > 0;
      }
      return value !== undefined && value !== null && value !== '';
    });

    if (activeFilters.length === 0) {
      return null;
    }

    return (
      <Box mt={2} mb={1}>
        <Typography variant="caption" color="text.secondary" gutterBottom>
          Filtros ativos:
        </Typography>
        <Box display="flex" flexWrap="wrap" gap={1} mt={1}>
          {activeFilters.map(field => {
            const value = values[field.name];
            let displayValue = value;

            if (Array.isArray(value)) {
              if (field.options) {
                displayValue = value
                  .map(v => field.options?.find(opt => opt.value === v)?.label || v)
                  .join(', ');
              } else {
                displayValue = value.join(', ');
              }
            } else if (field.options) {
              const option = field.options.find(opt => opt.value === value);
              displayValue = option?.label || value;
            }

            return (
              <Chip
                key={field.name}
                label={`${field.label}: ${displayValue}`}
                size="small"
                onDelete={() => onChange(field.name, field.multiple ? [] : '')}
                color="primary"
                variant="outlined"
              />
            );
          })}
        </Box>
      </Box>
    );
  };

  const filterContent = (
    <Box p={2}>
      <Grid container spacing={2}>
        {fields.map(field => (
          <Grid
            key={field.name}
            item
            xs={12}
            sm={field.gridSize || 6}
            md={field.gridSize || 4}
          >
            <FormField
              name={field.name}
              label={field.label}
              type={field.type as any}
              value={values[field.name]}
              onChange={onChange}
              options={field.options}
              placeholder={field.placeholder}
              multiple={field.multiple}
              freeSolo={field.freeSolo}
              size="small"
              fullWidth
            />
          </Grid>
        ))}
      </Grid>

      {renderFilterChips()}

      {(showApplyButton || showClearButton) && (
        <>
          <Divider sx={{ my: 2 }} />
          <Box display="flex" gap={1} justifyContent="flex-end">
            {showClearButton && (
              <Button
                variant="outlined"
                color="inherit"
                onClick={handleClear}
                disabled={loading || getActiveFiltersCount() === 0}
                startIcon={<ClearIcon />}
                size="small"
              >
                Limpar
              </Button>
            )}
            {showApplyButton && onApply && (
              <Button
                variant="contained"
                color="primary"
                onClick={onApply}
                disabled={loading}
                size="small"
              >
                Aplicar Filtros
              </Button>
            )}
          </Box>
        </>
      )}
    </Box>
  );

  if (!collapsible) {
    return (
      <Paper variant="outlined">
        {filterContent}
      </Paper>
    );
  }

  return (
    <Paper variant="outlined">
      <Box
        display="flex"
        alignItems="center"
        justifyContent="space-between"
        p={2}
        sx={{
          cursor: 'pointer',
          '&:hover': {
            backgroundColor: 'action.hover',
          },
        }}
        onClick={handleToggleExpanded}
      >
        <Box display="flex" alignItems="center" gap={1}>
          <Badge badgeContent={getActiveFiltersCount()} color="primary">
            <FilterIcon color="action" />
          </Badge>
          <Typography variant="subtitle1" fontWeight="medium">
            {title}
          </Typography>
        </Box>
        
        <Box display="flex" alignItems="center" gap={1}>
          {getActiveFiltersCount() > 0 && (
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                handleClear();
              }}
              title="Limpar filtros"
            >
              <ClearIcon fontSize="small" />
            </IconButton>
          )}
          <IconButton size="small">
            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </IconButton>
        </Box>
      </Box>
      
      <Collapse in={expanded}>
        <Divider />
        {filterContent}
      </Collapse>
    </Paper>
  );
};

export default FilterPanel;

// Hook para gerenciar filtros
export const useFilters = <T extends Record<string, any>>(initialFilters: T) => {
  const [filters, setFilters] = useState<T>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<T>(initialFilters);

  const updateFilter = (name: string, value: any) => {
    setFilters(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const applyFilters = () => {
    setAppliedFilters(filters);
  };

  const clearFilters = () => {
    setFilters(initialFilters);
    setAppliedFilters(initialFilters);
  };

  const resetFilters = () => {
    setFilters(appliedFilters);
  };

  const hasChanges = () => {
    return JSON.stringify(filters) !== JSON.stringify(appliedFilters);
  };

  const getActiveFiltersCount = () => {
    return Object.values(appliedFilters).filter(value => {
      if (Array.isArray(value)) {
        return value.length > 0;
      }
      return value !== undefined && value !== null && value !== '';
    }).length;
  };

  return {
    filters,
    appliedFilters,
    updateFilter,
    applyFilters,
    clearFilters,
    resetFilters,
    hasChanges,
    getActiveFiltersCount,
  };
};