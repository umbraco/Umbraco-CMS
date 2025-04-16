import { expect, test as base } from '@playwright/test';
import { createWorkerFixture } from 'playwright-msw';
import type { MockServiceWorker } from 'playwright-msw';

import { handlers } from '../src/mocks/e2e-handlers.js';

const test = base.extend<{
	worker: MockServiceWorker;
}>({
	worker: createWorkerFixture(handlers),
	page: async ({ page }, use) => {
		// Set is-authenticated in sessionStorage to true
		await page.addInitScript(`window.sessionStorage.setItem('is-authenticated', 'true');`);

		// Use signed-in page in all tests
		await use(page);
	},
});

export { test, expect };
