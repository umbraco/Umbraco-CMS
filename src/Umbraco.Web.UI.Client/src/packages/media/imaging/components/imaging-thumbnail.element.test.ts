import { UmbImagingThumbnailElement } from './imaging-thumbnail.element.js';
import { UmbThumbnailElement } from './thumbnail.element.js';
import { expect, fixture, html } from '@open-wc/testing';

// Behaviour (the "img" part, checkerboard default, --umb-thumbnail-background) is covered by
// thumbnail.element.test.ts and inherited from UmbThumbnailElement. This suite only guards that the
// deprecated `umb-imaging-thumbnail` alias stays registered and on the inheritance chain.
describe('UmbImagingThumbnailElement (deprecated alias)', () => {
	let element: UmbImagingThumbnailElement;

	beforeEach(async () => {
		element = await fixture<UmbImagingThumbnailElement>(html`<umb-imaging-thumbnail></umb-imaging-thumbnail>`);
	});

	it('is still registered and extends UmbThumbnailElement', () => {
		expect(element).to.be.instanceOf(UmbImagingThumbnailElement);
		expect(element).to.be.instanceOf(UmbThumbnailElement);
	});
});
