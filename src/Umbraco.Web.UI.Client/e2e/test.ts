import { handlers } from '../mocks/e2e-handlers.js';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import { expect, test as base } from '@playwright/test';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import { createWorkerFixture } from 'playwright-msw';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import type { MockServiceWorker } from 'playwright-msw';

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
