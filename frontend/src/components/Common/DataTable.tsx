import React, { useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  TableSortLabel,
  Paper,
  Checkbox,
  IconButton,
  Menu,
  MenuItem,
  Typography,
  Box,
  Chip,
  Tooltip,
} from '@mui/material';
import {
  MoreVert as MoreVertIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';
import { SortDirection } from '@/types';
import Loading from './Loading';

export interface Column<T> {
  id: keyof T | string;
  label: string;
  minWidth?: number;
  align?: 'right' | 'left' | 'center';
  sortable?: boolean;
  format?: (value: any, row: T) => React.ReactNode;
  render?: (value: any, row: T) => React.ReactNode;
}

export interface Action<T> {
  label: string;
  icon?: React.ReactElement;
  onClick: (row: T) => void;
  disabled?: (row: T) => boolean;
  color?: 'primary' | 'secondary' | 'error' | 'warning' | 'info' | 'success';
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  loading?: boolean;
  totalCount?: number;
  page?: number;
  pageSize?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  onSort?: (column: keyof T, direction: SortDirection) => void;
  sortBy?: keyof T;
  sortDirection?: SortDirection;
  selectable?: boolean;
  selectedRows?: T[];
  onSelectionChange?: (selectedRows: T[]) => void;
  actions?: Action<T>[];
  emptyMessage?: string;
  rowKey?: keyof T;
  onRowClick?: (row: T) => void;
  stickyHeader?: boolean;
  maxHeight?: number;
}

function DataTable<T extends Record<string, any>>({
  data,
  columns,
  loading = false,
  totalCount,
  page = 0,
  pageSize = 10,
  onPageChange,
  onPageSizeChange,
  onSort,
  sortBy,
  sortDirection = 'asc',
  selectable = false,
  selectedRows = [],
  onSelectionChange,
  actions = [],
  emptyMessage = 'Nenhum registro encontrado',
  rowKey = 'id' as keyof T,
  onRowClick,
  stickyHeader = false,
  maxHeight = 600,
}: DataTableProps<T>) {
  const [actionMenuAnchor, setActionMenuAnchor] = useState<{
    element: HTMLElement;
    row: T;
  } | null>(null);

  const handleSort = (column: keyof T) => {
    if (!onSort) return;
    
    const isAsc = sortBy === column && sortDirection === 'asc';
    onSort(column, isAsc ? 'desc' : 'asc');
  };

  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!onSelectionChange) return;
    
    if (event.target.checked) {
      onSelectionChange(data);
    } else {
      onSelectionChange([]);
    }
  };

  const handleSelectRow = (row: T) => {
    if (!onSelectionChange) return;
    
    const selectedIndex = selectedRows.findIndex(
      (selected) => selected[rowKey] === row[rowKey]
    );
    
    let newSelected: T[] = [];
    
    if (selectedIndex === -1) {
      newSelected = [...selectedRows, row];
    } else {
      newSelected = selectedRows.filter(
        (selected) => selected[rowKey] !== row[rowKey]
      );
    }
    
    onSelectionChange(newSelected);
  };

  const isSelected = (row: T) => {
    return selectedRows.some(
      (selected) => selected[rowKey] === row[rowKey]
    );
  };

  const handleActionMenuOpen = (
    event: React.MouseEvent<HTMLElement>,
    row: T
  ) => {
    event.stopPropagation();
    setActionMenuAnchor({ element: event.currentTarget, row });
  };

  const handleActionMenuClose = () => {
    setActionMenuAnchor(null);
  };

  const handleActionClick = (action: Action<T>, row: T) => {
    action.onClick(row);
    handleActionMenuClose();
  };

  const renderCellContent = (column: Column<T>, row: T) => {
    const value = row[column.id];
    
    if (column.render) {
      return column.render(value, row);
    }
    
    if (column.format) {
      return column.format(value, row);
    }
    
    return value;
  };

  if (loading) {
    return (
      <Paper>
        <Box p={4}>
          <Loading message="Carregando dados..." />
        </Box>
      </Paper>
    );
  }

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      <TableContainer sx={{ maxHeight: maxHeight }}>
        <Table stickyHeader={stickyHeader}>
          <TableHead>
            <TableRow>
              {selectable && (
                <TableCell padding="checkbox">
                  <Checkbox
                    indeterminate={
                      selectedRows.length > 0 && selectedRows.length < data.length
                    }
                    checked={data.length > 0 && selectedRows.length === data.length}
                    onChange={handleSelectAll}
                  />
                </TableCell>
              )}
              {columns.map((column) => (
                <TableCell
                  key={String(column.id)}
                  align={column.align}
                  style={{ minWidth: column.minWidth }}
                >
                  {column.sortable && onSort ? (
                    <TableSortLabel
                      active={sortBy === column.id}
                      direction={sortBy === column.id ? sortDirection : 'asc'}
                      onClick={() => handleSort(column.id)}
                    >
                      {column.label}
                    </TableSortLabel>
                  ) : (
                    column.label
                  )}
                </TableCell>
              ))}
              {actions.length > 0 && (
                <TableCell align="center" style={{ minWidth: 100 }}>
                  Ações
                </TableCell>
              )}
            </TableRow>
          </TableHead>
          <TableBody>
            {data.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={
                    columns.length + (selectable ? 1 : 0) + (actions.length > 0 ? 1 : 0)
                  }
                  align="center"
                >
                  <Box py={4}>
                    <Typography variant="body2" color="text.secondary">
                      {emptyMessage}
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => {
                const isItemSelected = isSelected(row);
                
                return (
                  <TableRow
                    hover
                    key={String(row[rowKey])}
                    selected={isItemSelected}
                    onClick={onRowClick ? () => onRowClick(row) : undefined}
                    sx={{
                      cursor: onRowClick ? 'pointer' : 'default',
                    }}
                  >
                    {selectable && (
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={isItemSelected}
                          onChange={() => handleSelectRow(row)}
                          onClick={(e) => e.stopPropagation()}
                        />
                      </TableCell>
                    )}
                    {columns.map((column) => (
                      <TableCell
                        key={String(column.id)}
                        align={column.align}
                      >
                        {renderCellContent(column, row)}
                      </TableCell>
                    ))}
                    {actions.length > 0 && (
                      <TableCell align="center">
                        <IconButton
                          size="small"
                          onClick={(e) => handleActionMenuOpen(e, row)}
                        >
                          <MoreVertIcon />
                        </IconButton>
                      </TableCell>
                    )}
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>
      
      {(onPageChange || onPageSizeChange) && (
        <TablePagination
          rowsPerPageOptions={[5, 10, 25, 50]}
          component="div"
          count={totalCount || data.length}
          rowsPerPage={pageSize}
          page={page}
          onPageChange={(_, newPage) => onPageChange?.(newPage)}
          onRowsPerPageChange={(event) =>
            onPageSizeChange?.(parseInt(event.target.value, 10))
          }
          labelRowsPerPage="Linhas por página:"
          labelDisplayedRows={({ from, to, count }) =>
            `${from}-${to} de ${count !== -1 ? count : `mais de ${to}`}`
          }
        />
      )}

      <Menu
        anchorEl={actionMenuAnchor?.element}
        open={Boolean(actionMenuAnchor)}
        onClose={handleActionMenuClose}
        onClick={handleActionMenuClose}
      >
        {actions.map((action, index) => {
          const disabled = actionMenuAnchor?.row
            ? action.disabled?.(actionMenuAnchor.row) || false
            : false;
          
          return (
            <MenuItem
              key={index}
              onClick={() =>
                actionMenuAnchor?.row &&
                handleActionClick(action, actionMenuAnchor.row)
              }
              disabled={disabled}
            >
              {action.icon && (
                <Box mr={1} display="flex" alignItems="center">
                  {action.icon}
                </Box>
              )}
              {action.label}
            </MenuItem>
          );
        })}
      </Menu>
    </Paper>
  );
}

export default DataTable;