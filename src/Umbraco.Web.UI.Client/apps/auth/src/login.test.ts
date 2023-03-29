import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import UmbLoginElement from './login.element';

describe('UmbLogin', () => {
	let element: UmbLoginElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-login></umb-login>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbLoginElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).to.be.accessible(defaultA11yConfig);
	});
});
