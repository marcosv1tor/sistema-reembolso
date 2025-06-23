import React from 'react';
import {
  Box,
  CircularProgress,
  Typography,
  Backdrop,
  Card,
  CardContent,
} from '@mui/material';

interface LoadingProps {
  message?: string;
  size?: number;
  overlay?: boolean;
  fullScreen?: boolean;
  color?: 'primary' | 'secondary' | 'inherit';
}

const Loading: React.FC<LoadingProps> = ({
  message = 'Carregando...',
  size = 40,
  overlay = false,
  fullScreen = false,
  color = 'primary',
}) => {
  const loadingContent = (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      gap={2}
      p={3}
    >
      <CircularProgress size={size} color={color} />
      {message && (
        <Typography variant="body2" color="text.secondary" textAlign="center">
          {message}
        </Typography>
      )}
    </Box>
  );

  if (fullScreen) {
    return (
      <Backdrop
        sx={{
          color: '#fff',
          zIndex: (theme) => theme.zIndex.drawer + 1,
          backgroundColor: 'rgba(0, 0, 0, 0.7)',
        }}
        open
      >
        <Card>
          <CardContent>
            {loadingContent}
          </CardContent>
        </Card>
      </Backdrop>
    );
  }

  if (overlay) {
    return (
      <Box
        position="absolute"
        top={0}
        left={0}
        right={0}
        bottom={0}
        display="flex"
        alignItems="center"
        justifyContent="center"
        bgcolor="rgba(255, 255, 255, 0.8)"
        zIndex={1}
      >
        {loadingContent}
      </Box>
    );
  }

  return loadingContent;
};

export default Loading;