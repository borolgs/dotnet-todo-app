import { chainAuthorized } from '~/shared/auth';
import { routes } from '~/shared/routing';

export const profileRoute = chainAuthorized(routes.profile);
