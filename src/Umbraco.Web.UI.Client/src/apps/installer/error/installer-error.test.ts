import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerErrorElement } from './installer-error.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerErrorElement', () => {
	let element: UmbInstallerErrorElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-error></umb-installer-error>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerErrorElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
