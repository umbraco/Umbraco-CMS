import { UmbTextStyles } from './text-style.style.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbTextStyles', () => {
	let code: HTMLElement;
	let codeWithDir: HTMLElement;

	beforeEach(async () => {
		const container = await fixture<HTMLDivElement>(html`<div dir="rtl"><div></div></div>`);
		const shadowRoot = container.firstElementChild!.attachShadow({ mode: 'open' });
		shadowRoot.adoptedStyleSheets = [UmbTextStyles.styleSheet!];
		shadowRoot.innerHTML = '<p>نص <code>@RenderBody()</code> <code dir="auto">اسم</code></p>';
		[code, codeWithDir] = shadowRoot.querySelectorAll('code');
	});

	it('lays out inline code left to right in right-to-left text', () => {
		expect(getComputedStyle(code).direction).to.equal('ltr');
	});

	it('isolates inline code from the direction of the surrounding text', () => {
		expect(getComputedStyle(code).unicodeBidi).to.equal('isolate');
	});

	it('leaves the direction of inline code with a dir attribute to that attribute', () => {
		expect(getComputedStyle(codeWithDir).direction).to.equal('rtl');
	});
});
