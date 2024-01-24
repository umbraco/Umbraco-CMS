import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerUserElement } from './installer-user.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerUserElement', () => {
	let element: UmbInstallerUserElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-user></umb-installer-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerUserElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
