import React, { ReactNode } from 'react';
import styles from './layout.module.css';

export const Layout: React.FC<{
  header?: ReactNode;
  footer?: ReactNode;
  children: ReactNode;
}> = ({ header, children, footer }) => {
  return (
    <div className={styles.layout}>
      <header className="container">{header}</header>
      <main className="container">{children}</main>
      <footer className="container">{footer}</footer>
    </div>
  );
};
