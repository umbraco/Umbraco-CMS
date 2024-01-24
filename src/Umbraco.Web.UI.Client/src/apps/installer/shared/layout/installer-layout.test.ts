import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerLayoutElement } from './installer-layout.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

// TODO: Write tests
describe('UmbInstallerLayoutElement', () => {
	let element: UmbInstallerLayoutElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-layout></umb-installer-layout>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerLayoutElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
