import { expect, fixture, html } from '@open-wc/testing';

import { UmbInstallerLayoutElement } from './installer-layout.element';
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

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
