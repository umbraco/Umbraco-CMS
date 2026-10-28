import { expect } from '@open-wc/testing';
import { UmbMediaPickerModalElement } from './media-picker-modal.element.js';

describe('UmbMediaPickerModalElement', () => {
	it('resets the media grid scroll when the page changes', async () => {
		const element = document.createElement('umb-media-picker-modal') as UmbMediaPickerModalElement;
		document.body.appendChild(element);
		await element.updateComplete;

		const bodyLayout = element.shadowRoot?.querySelector('umb-body-layout');
		const scrollContainer = bodyLayout?.shadowRoot?.querySelector('#main') as HTMLElement | null;

		expect(scrollContainer).to.exist;
		scrollContainer!.scrollTop = 400;

		(element as any)._resetScrollPosition();

		expect(scrollContainer!.scrollTop).to.equal(0);
	});
});
