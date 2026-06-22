import { UmbImagingThumbnailElement } from './imaging-thumbnail.element.js';
import { expect, fixture, html } from '@open-wc/testing';

// 1x1 transparent PNG.
const TRANSPARENT_PNG =
	'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M8AAAMBAQDJ/IjVAAAAAElFTkSuQmCC';

describe('UmbImagingThumbnailElement', () => {
	let element: UmbImagingThumbnailElement;

	beforeEach(async () => {
		// No `unique` is set, so #generateThumbnailUrl returns early and never
		// overwrites the thumbnail URL we inject below.
		element = await fixture<UmbImagingThumbnailElement>(html`<umb-imaging-thumbnail></umb-imaging-thumbnail>`);
		(element as unknown as { _thumbnailUrl: string })._thumbnailUrl = TRANSPARENT_PNG;
		element.requestUpdate();
		await element.updateComplete;

		// Guard: if the private field is ever renamed, the injection above silently no-ops.
		// Fail loudly here rather than letting later assertions pass vacuously.
		expect(element.shadowRoot!.querySelector('#figure'), 'sample image should render').to.not.equal(null);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbImagingThumbnailElement);
	});

	it('renders the image with a stylable "img" part', () => {
		const img = element.shadowRoot!.querySelector<HTMLImageElement>('#figure');
		expect(img).to.not.equal(null);
		expect(img!.getAttribute('part')).to.equal('img');
	});

	it('shows the checkerboard background by default', () => {
		const img = element.shadowRoot!.querySelector<HTMLImageElement>('#figure')!;
		expect(getComputedStyle(img).backgroundImage).to.contain('svg');
	});

	it('removes the checkerboard when --umb-thumbnail-background is overridden', () => {
		const img = element.shadowRoot!.querySelector<HTMLImageElement>('#figure')!;
		element.style.setProperty('--umb-thumbnail-background', 'none');
		expect(getComputedStyle(img).backgroundImage).to.equal('none');
	});
});
