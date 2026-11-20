import { UmbCodeBlockElement } from './code-block.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbCodeBlockElement', () => {
	it('lays out code left to right in a right-to-left document', async () => {
		const container = await fixture<HTMLDivElement>(
			html`<div dir="rtl">
				<umb-code-block><span>{ "id": 1 },</span></umb-code-block>
			</div>`,
		);
		await container.querySelector<UmbCodeBlockElement>('umb-code-block')!.updateComplete;

		expect(getComputedStyle(container.querySelector('span')!).direction).to.equal('ltr');
	});

	it('lays out the language label in the direction of its own text', async () => {
		const container = await fixture<HTMLDivElement>(
			html`<div dir="rtl"><umb-code-block language="C#">code</umb-code-block></div>`,
		);
		const element = container.querySelector<UmbCodeBlockElement>('umb-code-block')!;
		await element.updateComplete;

		expect(getComputedStyle(element.shadowRoot!.querySelector('#lang')!).direction).to.equal('ltr');
	});

	describe('copyCode', () => {
		it('does not throw when navigator.clipboard is unavailable', async () => {
			const element = await fixture<UmbCodeBlockElement>(html`<umb-code-block copy>some code</umb-code-block>`);

			// http://<ip> origins are insecure contexts where the browser removes navigator.clipboard;
			// simulate that by shadowing the prototype getter on the navigator instance.
			Object.defineProperty(navigator, 'clipboard', { value: undefined, configurable: true });
			try {
				await element.copyCode();
			} finally {
				// Deleting the own property restores the prototype getter.
				delete (navigator as { clipboard?: Clipboard }).clipboard;
			}

			expect(element).to.exist;
		});
	});
});
