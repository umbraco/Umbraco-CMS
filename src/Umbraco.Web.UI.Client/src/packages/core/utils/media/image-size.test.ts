import { imageSize } from './image-size.function';
import { expect } from '@open-wc/testing';

describe('imageSize', () => {
	let OriginalImage: typeof Image;

	before(() => {
		OriginalImage = window.Image;
	});

	after(() => {
		window.Image = OriginalImage;
	});

	function mockImage(naturalWidth: number, naturalHeight: number) {
		class MockImage {
			naturalWidth = naturalWidth;
			naturalHeight = naturalHeight;
			onload: (() => void) | null = null;
			onerror: (() => void) | null = null;
			set src(_url: string) {
				setTimeout(() => this.onload && this.onload(), 0);
			}
		}
		// @ts-ignore
		window.Image = MockImage;
	}

	it('returns natural size if no maxWidth or maxHeight is given', async () => {
		mockImage(800, 600);
		const result = await imageSize('fake-url');
		expect(result).to.deep.equal({
			width: 800,
			height: 600,
			naturalWidth: 800,
			naturalHeight: 600,
		});
	});

	it('scales down to maxWidth and maxHeight, ratio locked', async () => {
		mockImage(800, 600);
		const result = await imageSize('fake-url', { maxWidth: 400, maxHeight: 300 });
		expect(result).to.deep.equal({
			width: 400,
			height: 300,
			naturalWidth: 800,
			naturalHeight: 600,
		});
	});

	it('never upscales if maxWidth/maxHeight are larger than natural', async () => {
		mockImage(800, 600);
		const result = await imageSize('fake-url', { maxWidth: 1000, maxHeight: 1000 });
		expect(result).to.deep.equal({
			width: 800,
			height: 600,
			naturalWidth: 800,
			naturalHeight: 600,
		});
	});

	it('scales down by width if width is limiting', async () => {
		mockImage(800, 600);
		const result = await imageSize('fake-url', { maxWidth: 400 });
		expect(result).to.deep.equal({
			width: 400,
			height: 300,
			naturalWidth: 800,
			naturalHeight: 600,
		});
	});

	it('scales down by height if height is limiting', async () => {
		mockImage(800, 600);
		const result = await imageSize('fake-url', { maxHeight: 150 });
		expect(result).to.deep.equal({
			width: 200,
			height: 150,
			naturalWidth: 800,
			naturalHeight: 600,
		});
	});
});
