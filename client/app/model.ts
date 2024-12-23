import { createEvent, sample } from 'effector';
import { once } from 'patronum';
import { viewerQuery } from '~/shared/auth';

export const appLoaded = createEvent();

sample({
  clock: once(appLoaded),
  target: viewerQuery.start,
});
