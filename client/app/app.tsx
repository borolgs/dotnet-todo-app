import { RouterProvider } from 'atomic-router-react';
import { useEffect } from 'react';
import { Pages } from '~/pages';
import { appStarted } from '~/shared/config';
import { router } from '~/shared/routing';
import { appLoaded } from './model';

appStarted();

export function App() {
  useEffect(() => {
    appLoaded();
  }, []);
  return (
    <RouterProvider router={router}>
      <Pages />
    </RouterProvider>
  );
}
