import { UmbAppOauthElement } from './app-oauth.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbAppOauthElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-app-oauth></umb-app-oauth>`);
		expect(element).to.be.instanceOf(UmbAppOauthElement);
	});

	it('centers the loader in the available space', async () => {
		const container = await fixture<HTMLDivElement>(
			html`<div style="width: 800px; height: 600px;"><umb-app-oauth></umb-app-oauth></div>`,
		);
		const element = container.querySelector('umb-app-oauth')!;
		await element.updateComplete;

		const loader = element.shadowRoot!.querySelector('uui-loader')!;
		const containerRect = container.getBoundingClientRect();
		const loaderRect = loader.getBoundingClientRect();

		const containerCenterY = containerRect.top + containerRect.height / 2;
		const loaderCenterY = loaderRect.top + loaderRect.height / 2;

		expect(Math.abs(loaderCenterY - containerCenterY)).to.be.lessThan(2);
	});
});
