import { UmbCodeBlockElement } from './code-block.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbCodeBlockElement', () => {
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
