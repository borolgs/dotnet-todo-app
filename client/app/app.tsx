import { RouterProvider } from 'atomic-router-react';
import { useEffect } from 'react';
import { Pages } from '~/pages';
import { viewerQuery } from '~/shared/auth';
import { appStarted } from '~/shared/config';
import { router } from '~/shared/routing';

appStarted();

export function App() {
  useEffect(() => {
    viewerQuery.start();
  }, []);
  return (
    <RouterProvider router={router}>
      <Pages />
    </RouterProvider>
  );
}
