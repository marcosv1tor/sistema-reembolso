import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  IconButton,
  Typography,
  Box,
  Slide,
  Fade,
  Zoom,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  Close as CloseIcon,
  Warning as WarningIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
  CheckCircle as SuccessIcon,
} from '@mui/icons-material';
import { TransitionProps } from '@mui/material/transitions';

type ModalSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
type ModalType = 'default' | 'confirmation' | 'warning' | 'error' | 'info' | 'success';
type TransitionType = 'fade' | 'slide' | 'zoom';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children?: React.ReactNode;
  size?: ModalSize;
  type?: ModalType;
  transition?: TransitionType;
  showCloseButton?: boolean;
  disableBackdropClick?: boolean;
  disableEscapeKeyDown?: boolean;
  fullScreen?: boolean;
  actions?: React.ReactNode;
  confirmText?: string;
  cancelText?: string;
  onConfirm?: () => void;
  onCancel?: () => void;
  loading?: boolean;
  maxWidth?: false | ModalSize;
  scroll?: 'paper' | 'body';
}

const SlideTransition = React.forwardRef<
  unknown,
  TransitionProps & { children: React.ReactElement }
>((props, ref) => {
  return <Slide direction="up" ref={ref} {...props} />;
});

const FadeTransition = React.forwardRef<
  unknown,
  TransitionProps & { children: React.ReactElement }
>((props, ref) => {
  return <Fade ref={ref} {...props} />;
});

const ZoomTransition = React.forwardRef<
  unknown,
  TransitionProps & { children: React.ReactElement }
>((props, ref) => {
  return <Zoom ref={ref} {...props} />;
});

const getTransitionComponent = (transition: TransitionType) => {
  switch (transition) {
    case 'slide':
      return SlideTransition;
    case 'fade':
      return FadeTransition;
    case 'zoom':
      return ZoomTransition;
    default:
      return FadeTransition;
  }
};

const getTypeIcon = (type: ModalType) => {
  switch (type) {
    case 'warning':
      return <WarningIcon color="warning" />;
    case 'error':
      return <ErrorIcon color="error" />;
    case 'info':
      return <InfoIcon color="info" />;
    case 'success':
      return <SuccessIcon color="success" />;
    default:
      return null;
  }
};

const getTypeColor = (type: ModalType) => {
  switch (type) {
    case 'warning':
      return 'warning.main';
    case 'error':
      return 'error.main';
    case 'info':
      return 'info.main';
    case 'success':
      return 'success.main';
    default:
      return 'primary.main';
  }
};

const Modal: React.FC<ModalProps> = ({
  open,
  onClose,
  title,
  children,
  size = 'md',
  type = 'default',
  transition = 'fade',
  showCloseButton = true,
  disableBackdropClick = false,
  disableEscapeKeyDown = false,
  fullScreen,
  actions,
  confirmText = 'Confirmar',
  cancelText = 'Cancelar',
  onConfirm,
  onCancel,
  loading = false,
  maxWidth,
  scroll = 'paper',
}) => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const TransitionComponent = getTransitionComponent(transition);
  const typeIcon = getTypeIcon(type);
  const typeColor = getTypeColor(type);

  const handleClose = (event: {}, reason: 'backdropClick' | 'escapeKeyDown') => {
    if (reason === 'backdropClick' && disableBackdropClick) return;
    if (reason === 'escapeKeyDown' && disableEscapeKeyDown) return;
    onClose();
  };

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      onClose();
    }
  };

  const renderActions = () => {
    if (actions) {
      return actions;
    }

    if (type === 'confirmation' || onConfirm) {
      return (
        <>
          <Button
            onClick={handleCancel}
            disabled={loading}
            color="inherit"
          >
            {cancelText}
          </Button>
          <Button
            onClick={onConfirm}
            disabled={loading}
            variant="contained"
            color={type === 'error' ? 'error' : 'primary'}
            autoFocus
          >
            {confirmText}
          </Button>
        </>
      );
    }

    return null;
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth={maxWidth !== undefined ? maxWidth : size}
      fullWidth
      fullScreen={fullScreen || (isMobile && size === 'xl')}
      TransitionComponent={TransitionComponent}
      scroll={scroll}
      PaperProps={{
        sx: {
          borderRadius: fullScreen ? 0 : 2,
          ...(type !== 'default' && {
            borderTop: `4px solid ${typeColor}`,
          }),
        },
      }}
    >
      {title && (
        <DialogTitle
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            pb: 1,
          }}
        >
          <Box display="flex" alignItems="center" gap={1}>
            {typeIcon}
            <Typography variant="h6" component="span">
              {title}
            </Typography>
          </Box>
          {showCloseButton && (
            <IconButton
              onClick={onClose}
              size="small"
              sx={{
                color: 'grey.500',
                '&:hover': {
                  color: 'grey.700',
                },
              }}
            >
              <CloseIcon />
            </IconButton>
          )}
        </DialogTitle>
      )}
      
      {children && (
        <DialogContent
          sx={{
            py: title ? 2 : 3,
          }}
        >
          {children}
        </DialogContent>
      )}
      
      {renderActions() && (
        <DialogActions
          sx={{
            px: 3,
            pb: 2,
            gap: 1,
          }}
        >
          {renderActions()}
        </DialogActions>
      )}
    </Dialog>
  );
};

export default Modal;

// Hook para usar modais de confirmação
export const useConfirmationModal = () => {
  const [modalState, setModalState] = React.useState<{
    open: boolean;
    title?: string;
    message?: string;
    type?: ModalType;
    onConfirm?: () => void;
    confirmText?: string;
    cancelText?: string;
  }>({
    open: false,
  });

  const showConfirmation = React.useCallback((
    options: {
      title?: string;
      message?: string;
      type?: ModalType;
      onConfirm?: () => void;
      confirmText?: string;
      cancelText?: string;
    }
  ) => {
    setModalState({
      open: true,
      ...options,
    });
  }, []);

  const hideConfirmation = React.useCallback(() => {
    setModalState(prev => ({ ...prev, open: false }));
  }, []);

  const ConfirmationModal = React.useCallback(() => (
    <Modal
      open={modalState.open}
      onClose={hideConfirmation}
      title={modalState.title}
      type={modalState.type || 'confirmation'}
      size="sm"
      onConfirm={() => {
        modalState.onConfirm?.();
        hideConfirmation();
      }}
      onCancel={hideConfirmation}
      confirmText={modalState.confirmText}
      cancelText={modalState.cancelText}
    >
      {modalState.message && (
        <Typography>{modalState.message}</Typography>
      )}
    </Modal>
  ), [modalState, hideConfirmation]);

  return {
    showConfirmation,
    hideConfirmation,
    ConfirmationModal,
  };
};