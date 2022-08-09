import { expect, fixture, html } from '@open-wc/testing';

import { defaultA11yConfig } from '../../core/helpers/chai';
import UmbLogin from './login.element';

describe('UmbLogin', () => {
	let element: UmbLogin;

	beforeEach(async () => {
		element = await fixture(html`<umb-login></umb-login>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbLogin);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
