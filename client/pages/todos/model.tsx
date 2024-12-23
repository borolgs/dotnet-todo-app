import { chainAuthorized } from '~/shared/auth';
import { routes } from '~/shared/routing';

export const todosRoute = chainAuthorized(routes.todos);
