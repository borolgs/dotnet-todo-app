import { createRoutesView } from 'atomic-router-react';
import { RootPage } from './root';
import { routes } from '~/shared/routing';

import { ProfilePage, profileRoute } from './profile';
import { LoginPage, loginRoute } from './login';
import { TodosPage, todosRoute } from './todos';

const RoutesView = createRoutesView({
  routes: [
    { route: routes.home, view: RootPage },
    { route: profileRoute, view: ProfilePage },
    { route: loginRoute, view: LoginPage },
    { route: loginRoute, view: LoginPage },
    { route: todosRoute, view: TodosPage },
  ],
});

export const Pages = RoutesView;
