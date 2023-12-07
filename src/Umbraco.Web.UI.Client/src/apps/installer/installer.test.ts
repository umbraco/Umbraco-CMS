import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerElement } from './installer.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerElement', () => {
	let element: UmbInstallerElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer></umb-installer>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
