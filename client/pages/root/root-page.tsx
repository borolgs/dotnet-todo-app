import { Layout } from '~/shared/ui/layout';
import { Header } from '~/widgets/header';

export function RootPage() {
  return (
    <Layout header={<Header />}>
      <h1>Home</h1>
    </Layout>
  );
}
