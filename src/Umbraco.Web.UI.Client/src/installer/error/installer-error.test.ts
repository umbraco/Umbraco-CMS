import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../../core/helpers/chai';
import { UmbInstallerErrorElement } from './installer-error.element';

// TODO: Write tests
describe('UmbInstallerErrorElement', () => {
	let element: UmbInstallerErrorElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-installer-error></umb-installer-error>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInstallerErrorElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
