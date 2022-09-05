import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../../core/helpers/chai';
import { UmbInstallerUserElement } from './installer-user.element';

// TODO: Write tests
describe('UmbInstallerUser', () => {
	let element: UmbInstallerUserElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-user></umb-installer-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerUserElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
