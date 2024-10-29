import { Layout } from '~/shared/ui/layout';
import { Header } from '~/widgets/header';

export function TodosPage() {
  return (
    <Layout header={<Header />}>
      <h1>Todos</h1>
    </Layout>
  );
}
