import { applyBarrier, createBarrier, createQuery } from '@farfetched/core';
import { createEvent, createStore, sample } from 'effector';
import { not } from 'patronum';
import { router, routes } from './routing';
import { RouteInstance, RouteParams, RouteParamsAndQuery, chainRoute } from 'atomic-router';
import { client } from './api/client';
import { components } from './api/schema';

export const viewerQuery = createQuery({
  handler: async () => {
    return await client.GET('/auth/manage/info');
  },
});

export const signInQuery = createQuery({
  handler: async (args: components['schemas']['LoginRequest']) => {
    return await client.POST('/auth/login', {
      body: args,
      params: { query: { useCookies: true } },
    });
  },
});

export const authorized = createEvent();
const unauthorized = createEvent();

export const $authorized = createStore(false);
export const $next = createStore('/');
export const $nextRoute = createStore<null | RouteInstance<{}>>(null);
$authorized.on(authorized, () => true).on(unauthorized, () => false);
sample({
  clock: [viewerQuery.finished.success, signInQuery.finished.success],
  filter: ({ result }) => {
    return result.response.status === 401;
  },
  target: unauthorized,
});
{
  sample({
    clock: unauthorized,
    source: router.$path,
    filter: not(routes.signin.$isOpened),
    target: $next,
  });
  sample({
    clock: unauthorized,
    target: routes.signin.open,
  });
}
sample({
  clock: [viewerQuery.finished.success, signInQuery.finished.success],
  filter: ({ result }) => {
    return result.error != null ? false : true;
  },
  target: authorized,
});

export function chainAuthorized<Params extends RouteParams>(route: RouteInstance<Params>) {
  const sessionCheckStarted = createEvent<RouteParamsAndQuery<Params>>();

  const alreadyAuthorized = sample({
    clock: sessionCheckStarted,
    filter: $authorized,
  });

  sample({
    clock: sessionCheckStarted,
    filter: not($authorized),
    fn: () => {},
    target: viewerQuery.start,
  });

  return chainRoute({
    route,
    beforeOpen: sessionCheckStarted,
    openOn: [alreadyAuthorized, authorized],
  });
}

const authBarrier = createBarrier({
  active: not($authorized),
});

type RemoteOperation = Parameters<typeof applyBarrier>[0][0];

export function applyAuthBarrier(operation: RemoteOperation | RemoteOperation[]) {
  const operations = Array.isArray(operation) ? operation : [operation];
  for (const operation of operations) {
    sample({
      clock: operation.finished.failure,
      filter: ({ error }) => {
        return (error as any)?.response?.status === 401;
      },
      target: unauthorized,
    });
  }

  applyBarrier(operations, { barrier: authBarrier });
}
