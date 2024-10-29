import { useUnit } from 'effector-react';
import React, { ReactNode } from 'react';
import { $authorized } from '~/shared/auth';

export const Protected: React.FC<{
  children: ReactNode;
}> = ({ children }) => {
  const authorized = useUnit($authorized);
  return <>{authorized && children}</>;
};
