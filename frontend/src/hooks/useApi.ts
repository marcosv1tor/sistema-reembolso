import { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { LoadingState, ApiError } from '@/types';

interface UseApiOptions {
  onSuccess?: (data: any) => void;
  onError?: (error: ApiError) => void;
  showSuccessToast?: boolean;
  showErrorToast?: boolean;
  successMessage?: string;
}

interface UseApiReturn<T> {
  data: T | null;
  loading: boolean;
  error: ApiError | null;
  execute: (...args: any[]) => Promise<T | void>;
  reset: () => void;
  state: LoadingState;
}

export function useApi<T>(
  apiFunction: (...args: any[]) => Promise<T>,
  options: UseApiOptions = {}
): UseApiReturn<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ApiError | null>(null);
  const [state, setState] = useState<LoadingState>('idle');

  const {
    onSuccess,
    onError,
    showSuccessToast = false,
    showErrorToast = true,
    successMessage = 'Operação realizada com sucesso!'
  } = options;

  const execute = useCallback(
    async (...args: any[]): Promise<T | void> => {
      try {
        setLoading(true);
        setState('loading');
        setError(null);

        const result = await apiFunction(...args);
        
        setData(result);
        setState('success');
        
        if (showSuccessToast) {
          toast.success(successMessage);
        }
        
        if (onSuccess) {
          onSuccess(result);
        }
        
        return result;
      } catch (err: any) {
        const apiError: ApiError = {
          message: err.response?.data?.message || err.message || 'Erro desconhecido',
          status: err.response?.status || 0,
          errors: err.response?.data?.errors
        };
        
        setError(apiError);
        setState('error');
        
        if (showErrorToast) {
          toast.error(apiError.message);
        }
        
        if (onError) {
          onError(apiError);
        }
        
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [apiFunction, onSuccess, onError, showSuccessToast, showErrorToast, successMessage]
  );

  const reset = useCallback(() => {
    setData(null);
    setError(null);
    setLoading(false);
    setState('idle');
  }, []);

  return {
    data,
    loading,
    error,
    execute,
    reset,
    state
  };
}

// Hook para operações que executam automaticamente
export function useApiEffect<T>(
  apiFunction: () => Promise<T>,
  dependencies: any[] = [],
  options: UseApiOptions = {}
): UseApiReturn<T> {
  const apiHook = useApi(apiFunction, options);

  useEffect(() => {
    apiHook.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, dependencies);

  return apiHook;
}

// Hook para paginação
interface UsePaginationOptions {
  initialPage?: number;
  initialPageSize?: number;
}

interface UsePaginationReturn {
  page: number;
  pageSize: number;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  resetPagination: () => void;
}

export function usePagination(options: UsePaginationOptions = {}): UsePaginationReturn {
  const { initialPage = 1, initialPageSize = 10 } = options;
  
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);

  const resetPagination = useCallback(() => {
    setPage(initialPage);
    setPageSize(initialPageSize);
  }, [initialPage, initialPageSize]);

  return {
    page,
    pageSize,
    setPage,
    setPageSize,
    resetPagination
  };
}

// Hook para filtros
export function useFilters<T>(initialFilters: T) {
  const [filters, setFilters] = useState<T>(initialFilters);

  const updateFilter = useCallback((key: keyof T, value: any) => {
    setFilters(prev => ({
      ...prev,
      [key]: value
    }));
  }, []);

  const updateFilters = useCallback((newFilters: Partial<T>) => {
    setFilters(prev => ({
      ...prev,
      ...newFilters
    }));
  }, []);

  const resetFilters = useCallback(() => {
    setFilters(initialFilters);
  }, [initialFilters]);

  const clearFilter = useCallback((key: keyof T) => {
    setFilters(prev => {
      const newFilters = { ...prev };
      delete newFilters[key];
      return newFilters;
    });
  }, []);

  return {
    filters,
    updateFilter,
    updateFilters,
    resetFilters,
    clearFilter,
    setFilters
  };
}

// Hook para debounce
export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

// Hook para localStorage
export function useLocalStorage<T>(key: string, initialValue: T) {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      const item = window.localStorage.getItem(key);
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      console.error(`Erro ao ler localStorage key "${key}":`, error);
      return initialValue;
    }
  });

  const setValue = useCallback((value: T | ((val: T) => T)) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value;
      setStoredValue(valueToStore);
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      console.error(`Erro ao salvar no localStorage key "${key}":`, error);
    }
  }, [key, storedValue]);

  const removeValue = useCallback(() => {
    try {
      window.localStorage.removeItem(key);
      setStoredValue(initialValue);
    } catch (error) {
      console.error(`Erro ao remover localStorage key "${key}":`, error);
    }
  }, [key, initialValue]);

  return [storedValue, setValue, removeValue] as const;
}