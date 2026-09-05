import { UmbImagingThumbnailElement } from './imaging-thumbnail.element.js';
import { UmbMediaThumbnailElement } from './media-thumbnail.element.js';
import { expect, fixture, html } from '@open-wc/testing';

// Behaviour (the "img" part, checkerboard default, --umb-media-thumbnail-background) is covered by
// media-thumbnail.element.test.ts and inherited from UmbMediaThumbnailElement. This suite only guards
// that the deprecated `umb-imaging-thumbnail` alias stays registered and on the inheritance chain.
describe('UmbImagingThumbnailElement (deprecated alias)', () => {
	let element: UmbImagingThumbnailElement;

	beforeEach(async () => {
		element = await fixture<UmbImagingThumbnailElement>(html`<umb-imaging-thumbnail></umb-imaging-thumbnail>`);
	});

	it('is still registered and extends UmbMediaThumbnailElement', () => {
		expect(element).to.be.instanceOf(UmbImagingThumbnailElement);
		expect(element).to.be.instanceOf(UmbMediaThumbnailElement);
	});
});
