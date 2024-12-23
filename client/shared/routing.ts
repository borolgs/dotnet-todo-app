import { createHistoryRouter, createRoute } from 'atomic-router';
import { sample } from 'effector';
import { createBrowserHistory } from 'history';
import { appStarted } from '~/shared/config';

export const routes = {
  home: createRoute(),
  profile: createRoute(),
  signin: createRoute(),
  signup: createRoute(),
  todos: createRoute(),
};

export const routesMap = [
  { path: '/', route: routes.home },
  { path: '/profile', route: routes.profile },
  { path: '/login', route: routes.signin },
  { path: '/todos', route: routes.todos },
];

export const router = createHistoryRouter({
  routes: routesMap,
});

sample({
  clock: appStarted,
  fn: () => createBrowserHistory(),
  target: router.setHistory,
});
