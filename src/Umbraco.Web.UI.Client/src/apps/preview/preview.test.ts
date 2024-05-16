import { expect, fixture, html } from '@open-wc/testing';
import { UmbPreviewElement } from './preview.element.js';

describe('UmbPreview', () => {
	let element: UmbPreviewElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-preview></umb-preview>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPreviewElement);
	});
});
