import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../core/helpers/chai';
import { UmbInstallerElement } from './installer.element';

// TODO: Write tests
describe('UmbInstaller', () => {
	let element: UmbInstallerElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer></umb-installer>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
