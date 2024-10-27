import styles from './root-page.module.css';

export function RootPage() {
  return (
    <div className={styles.root}>
      <header>header</header>
      <section>controls</section>
      <section>main</section>
      <footer>footer</footer>
    </div>
  );
}
