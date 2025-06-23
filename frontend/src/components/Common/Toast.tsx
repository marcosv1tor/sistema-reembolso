import React, { createContext, useContext, useState, useCallback } from 'react';
import {
  Snackbar,
  Alert,
  AlertTitle,
  Slide,
  Fade,
  Grow,
  IconButton,
  Box,
} from '@mui/material';
import {
  Close as CloseIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { TransitionProps } from '@mui/material/transitions';

type ToastType = 'success' | 'error' | 'warning' | 'info';
type ToastPosition = 
  | 'top-left'
  | 'top-center'
  | 'top-right'
  | 'bottom-left'
  | 'bottom-center'
  | 'bottom-right';

interface ToastMessage {
  id: string;
  type: ToastType;
  title?: string;
  message: string;
  duration?: number;
  persistent?: boolean;
  action?: React.ReactNode;
}

interface ToastContextType {
  showToast: (toast: Omit<ToastMessage, 'id'>) => void;
  showSuccess: (message: string, title?: string) => void;
  showError: (message: string, title?: string) => void;
  showWarning: (message: string, title?: string) => void;
  showInfo: (message: string, title?: string) => void;
  hideToast: (id: string) => void;
  hideAllToasts: () => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export const useToast = () => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

interface ToastProviderProps {
  children: React.ReactNode;
  maxToasts?: number;
  position?: ToastPosition;
  defaultDuration?: number;
}

const SlideTransition = (props: TransitionProps & { children: React.ReactElement }) => {
  return <Slide {...props} direction="left" />;
};

const FadeTransition = (props: TransitionProps & { children: React.ReactElement }) => {
  return <Fade {...props} />;
};

const GrowTransition = (props: TransitionProps & { children: React.ReactElement }) => {
  return <Grow {...props} />;
};

const getAnchorOrigin = (position: ToastPosition) => {
  const [vertical, horizontal] = position.split('-') as [string, string];
  return {
    vertical: vertical as 'top' | 'bottom',
    horizontal: horizontal === 'center' ? 'center' as const : horizontal as 'left' | 'right',
  };
};

const getToastIcon = (type: ToastType) => {
  switch (type) {
    case 'success':
      return <SuccessIcon />;
    case 'error':
      return <ErrorIcon />;
    case 'warning':
      return <WarningIcon />;
    case 'info':
      return <InfoIcon />;
    default:
      return null;
  }
};

export const ToastProvider: React.FC<ToastProviderProps> = ({
  children,
  maxToasts = 5,
  position = 'top-right',
  defaultDuration = 6000,
}) => {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const generateId = () => {
    return `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  };

  const showToast = useCallback((toast: Omit<ToastMessage, 'id'>) => {
    const id = generateId();
    const newToast: ToastMessage = {
      ...toast,
      id,
      duration: toast.duration ?? defaultDuration,
    };

    setToasts(prev => {
      const updated = [newToast, ...prev];
      return updated.slice(0, maxToasts);
    });

    // Auto-hide toast if not persistent
    if (!newToast.persistent && newToast.duration && newToast.duration > 0) {
      setTimeout(() => {
        hideToast(id);
      }, newToast.duration);
    }
  }, [defaultDuration, maxToasts]);

  const showSuccess = useCallback((message: string, title?: string) => {
    showToast({ type: 'success', message, title });
  }, [showToast]);

  const showError = useCallback((message: string, title?: string) => {
    showToast({ type: 'error', message, title, persistent: true });
  }, [showToast]);

  const showWarning = useCallback((message: string, title?: string) => {
    showToast({ type: 'warning', message, title });
  }, [showToast]);

  const showInfo = useCallback((message: string, title?: string) => {
    showToast({ type: 'info', message, title });
  }, [showToast]);

  const hideToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  }, []);

  const hideAllToasts = useCallback(() => {
    setToasts([]);
  }, []);

  const contextValue: ToastContextType = {
    showToast,
    showSuccess,
    showError,
    showWarning,
    showInfo,
    hideToast,
    hideAllToasts,
  };

  return (
    <ToastContext.Provider value={contextValue}>
      {children}
      <ToastContainer toasts={toasts} position={position} onClose={hideToast} />
    </ToastContext.Provider>
  );
};

interface ToastContainerProps {
  toasts: ToastMessage[];
  position: ToastPosition;
  onClose: (id: string) => void;
}

const ToastContainer: React.FC<ToastContainerProps> = ({
  toasts,
  position,
  onClose,
}) => {
  const anchorOrigin = getAnchorOrigin(position);

  return (
    <Box
      sx={{
        position: 'fixed',
        zIndex: (theme) => theme.zIndex.snackbar,
        ...anchorOrigin,
        ...(anchorOrigin.vertical === 'top' ? { top: 24 } : { bottom: 24 }),
        ...(anchorOrigin.horizontal === 'left' && { left: 24 }),
        ...(anchorOrigin.horizontal === 'right' && { right: 24 }),
        ...(anchorOrigin.horizontal === 'center' && {
          left: '50%',
          transform: 'translateX(-50%)',
        }),
        display: 'flex',
        flexDirection: anchorOrigin.vertical === 'top' ? 'column' : 'column-reverse',
        gap: 1,
        maxWidth: 400,
        width: '100%',
      }}
    >
      {toasts.map((toast, index) => (
        <ToastItem
          key={toast.id}
          toast={toast}
          onClose={onClose}
          delay={index * 100}
        />
      ))}
    </Box>
  );
};

interface ToastItemProps {
  toast: ToastMessage;
  onClose: (id: string) => void;
  delay?: number;
}

const ToastItem: React.FC<ToastItemProps> = ({ toast, onClose, delay = 0 }) => {
  const [open, setOpen] = useState(false);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setOpen(true);
    }, delay);

    return () => clearTimeout(timer);
  }, [delay]);

  const handleClose = () => {
    setOpen(false);
    setTimeout(() => {
      onClose(toast.id);
    }, 300); // Wait for exit animation
  };

  return (
    <Snackbar
      open={open}
      TransitionComponent={SlideTransition}
      sx={{
        position: 'relative',
        transform: 'none !important',
        left: 'auto !important',
        right: 'auto !important',
        top: 'auto !important',
        bottom: 'auto !important',
      }}
    >
      <Alert
        severity={toast.type}
        variant="filled"
        icon={getToastIcon(toast.type)}
        action={
          toast.action || (
            <IconButton
              size="small"
              color="inherit"
              onClick={handleClose}
            >
              <CloseIcon fontSize="small" />
            </IconButton>
          )
        }
        sx={{
          width: '100%',
          alignItems: 'flex-start',
          '& .MuiAlert-message': {
            width: '100%',
          },
        }}
      >
        {toast.title && (
          <AlertTitle sx={{ mb: toast.message ? 0.5 : 0 }}>
            {toast.title}
          </AlertTitle>
        )}
        {toast.message}
      </Alert>
    </Snackbar>
  );
};

export default ToastProvider;

// Hook para usar toasts de forma mais simples
export const useSimpleToast = () => {
  const { showSuccess, showError, showWarning, showInfo } = useToast();

  return {
    success: showSuccess,
    error: showError,
    warning: showWarning,
    info: showInfo,
  };
};