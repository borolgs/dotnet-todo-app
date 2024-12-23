import { combine, createEvent, createStore, sample } from 'effector';
import { $next, authorized, signInQuery } from '~/shared/auth';
import { router, routes } from '~/shared/routing';

export const loginRoute = routes.signin;

export const updateEmail = createEvent<string>();
export const updatePassword = createEvent<string>();

export const $email = createStore<string | null>(null).on(updateEmail, (_, v) => v);
export const $password = createStore<string | null>(null).on(updatePassword, (_, v) => v);
export const submitLogin = createEvent('');
export const $ready = combine([$email, $password], (fields) => fields.every((v) => v !== null && v !== ''));

sample({
  clock: submitLogin,
  filter: $ready,
  source: {
    email: $email,
    password: $password,
  },
  target: signInQuery.start,
});

sample({
  clock: authorized,
  filter: loginRoute.$isOpened,
  source: $next,
  fn: (next) => ({ path: next, params: {}, query: {}, method: 'push' } as const),
  target: router.push,
});
