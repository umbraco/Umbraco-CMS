import { expect, test as base } from '@playwright/test';
import { createWorkerFixture } from 'playwright-msw';

import { handlers } from './src/mocks/e2e-handlers';

import type { MockServiceWorker } from 'playwright-msw';
const test = base.extend<{
	worker: MockServiceWorker;
}>({
	worker: createWorkerFixture(...handlers),
});

export { test, expect };
