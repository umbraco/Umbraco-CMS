import { UmbMenuItemLayoutElement } from './menu-item-layout.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbMenuItemLayoutElement', () => {
	const originalPath = window.location.pathname;

	beforeEach(() => {
		window.history.replaceState(null, '', '/section/test/workspace/item-1');
	});

	afterEach(() => {
		window.history.replaceState(null, '', originalPath);
	});

	function isActive(element: UmbMenuItemLayoutElement): boolean {
		return element.shadowRoot!.querySelector('uui-menu-item')!.hasAttribute('active');
	}

	it('is defined with its own instance', async () => {
		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item"></umb-menu-item-layout>`,
		);
		expect(element).to.be.instanceOf(UmbMenuItemLayoutElement);
	});

	it('is active when it renders after the navigation to its own href has ended', async () => {
		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item" href="/section/test/workspace/item-1"></umb-menu-item-layout>`,
		);

		expect(isActive(element)).to.be.true;
	});

	it('is active when its href is set after it has connected', async () => {
		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item"></umb-menu-item-layout>`,
		);
		expect(isActive(element)).to.be.false;

		element.href = '/section/test/workspace/item-1';
		await element.updateComplete;

		expect(isActive(element)).to.be.true;
	});

	it('is no longer active when its href changes to a different location', async () => {
		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item" href="/section/test/workspace/item-1"></umb-menu-item-layout>`,
		);

		element.href = '/section/test/workspace/item-2';
		await element.updateComplete;

		expect(isActive(element)).to.be.false;
	});

	it('is not active when its href only shares a prefix with the current location', async () => {
		window.history.replaceState(null, '', '/section/test/workspace/item-12');

		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item" href="/section/test/workspace/item-1"></umb-menu-item-layout>`,
		);

		expect(isActive(element)).to.be.false;
	});

	it('updates when a navigation ends', async () => {
		const element = await fixture<UmbMenuItemLayoutElement>(
			html`<umb-menu-item-layout label="Item" href="/section/test/workspace/item-2"></umb-menu-item-layout>`,
		);
		expect(isActive(element)).to.be.false;

		window.history.replaceState(null, '', '/section/test/workspace/item-2');
		window.dispatchEvent(new CustomEvent('navigationend'));
		await element.updateComplete;

		expect(isActive(element)).to.be.true;
	});
});
