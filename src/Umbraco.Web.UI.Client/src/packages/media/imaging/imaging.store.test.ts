import { UmbImagingStore } from './imaging.store.js';
import type { UmbImagingResizeModel } from './types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-imaging-store-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbImagingStore', () => {
	let store: UmbImagingStore;
	const mediaKey = 'test-media-key-123';

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		store = new UmbImagingStore(hostElement);
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a getData method', () => {
				expect(store).to.have.property('getData').that.is.a('function');
			});

			it('has a getCrop method', () => {
				expect(store).to.have.property('getCrop').that.is.a('function');
			});

			it('has a addCrop method', () => {
				expect(store).to.have.property('addCrop').that.is.a('function');
			});

			it('has a clear method', () => {
				expect(store).to.have.property('clear').that.is.a('function');
			});

			it('has a clearCropByUnique method', () => {
				expect(store).to.have.property('clearCropByUnique').that.is.a('function');
			});

			it('has a clearCropByConfiguration method', () => {
				expect(store).to.have.property('clearCropByConfiguration').that.is.a('function');
			});
		});
	});

	describe('Cache key generation with format', () => {
		it('caches crops with different formats separately', () => {
			const webpModel: UmbImagingResizeModel = { width: 300, height: 300, mode: undefined, format: 'webp' };
			const pngModel: UmbImagingResizeModel = { width: 300, height: 300, mode: undefined, format: 'png' };

			const webpUrl = 'http://example.com/image.jpg?width=300&height=300&format=webp';
			const pngUrl = 'http://example.com/image.jpg?width=300&height=300&format=png';

			store.addCrop(mediaKey, webpUrl, webpModel);
			store.addCrop(mediaKey, pngUrl, pngModel);

			expect(store.getCrop(mediaKey, webpModel)).to.equal(webpUrl);
			expect(store.getCrop(mediaKey, pngModel)).to.equal(pngUrl);
		});

		it('caches crops without format separately from crops with format', () => {
			const withFormatModel: UmbImagingResizeModel = { width: 300, height: 300, format: 'webp' };
			const withoutFormatModel: UmbImagingResizeModel = { width: 300, height: 300 };

			const withFormatUrl = 'http://example.com/image.jpg?width=300&height=300&format=webp';
			const withoutFormatUrl = 'http://example.com/image.jpg?width=300&height=300';

			store.addCrop(mediaKey, withFormatUrl, withFormatModel);
			store.addCrop(mediaKey, withoutFormatUrl, withoutFormatModel);

			expect(store.getCrop(mediaKey, withFormatModel)).to.equal(withFormatUrl);
			expect(store.getCrop(mediaKey, withoutFormatModel)).to.equal(withoutFormatUrl);
		});

		it('returns undefined for uncached format variations', () => {
			const webpModel: UmbImagingResizeModel = { width: 300, height: 300, format: 'webp' };
			const jpgModel: UmbImagingResizeModel = { width: 300, height: 300, format: 'jpg' };

			const webpUrl = 'http://example.com/image.jpg?width=300&height=300&format=webp';

			store.addCrop(mediaKey, webpUrl, webpModel);

			expect(store.getCrop(mediaKey, webpModel)).to.equal(webpUrl);
			expect(store.getCrop(mediaKey, jpgModel)).to.be.undefined;
		});
	});

	describe('Add and Get Crop', () => {
		it('adds and retrieves a crop with all parameters', () => {
			const model: UmbImagingResizeModel = { width: 200, height: 200, mode: undefined, format: 'webp' };
			const url = 'http://example.com/thumbnail.webp';

			store.addCrop(mediaKey, url, model);

			expect(store.getCrop(mediaKey, model)).to.equal(url);
		});

		it('adds and retrieves a crop without format', () => {
			const model: UmbImagingResizeModel = { width: 200, height: 200 };
			const url = 'http://example.com/thumbnail.jpg';

			store.addCrop(mediaKey, url, model);

			expect(store.getCrop(mediaKey, model)).to.equal(url);
		});

		it('adds and retrieves a generic crop without model', () => {
			const url = 'http://example.com/original.jpg';

			store.addCrop(mediaKey, url);

			expect(store.getCrop(mediaKey)).to.equal(url);
		});
	});

	describe('Clear operations', () => {
		it('clears all crops', () => {
			const model: UmbImagingResizeModel = { width: 300, height: 300, format: 'webp' };
			store.addCrop(mediaKey, 'http://example.com/image.webp', model);

			store.clear();

			expect(store.getCrop(mediaKey, model)).to.be.undefined;
		});

		it('clears crops by unique identifier', () => {
			const model: UmbImagingResizeModel = { width: 300, height: 300, format: 'webp' };
			const otherMediaKey = 'other-media-key';

			store.addCrop(mediaKey, 'http://example.com/image1.webp', model);
			store.addCrop(otherMediaKey, 'http://example.com/image2.webp', model);

			store.clearCropByUnique(mediaKey);

			expect(store.getCrop(mediaKey, model)).to.be.undefined;
			expect(store.getCrop(otherMediaKey, model)).to.equal('http://example.com/image2.webp');
		});

		it('clears crops by configuration', () => {
			const webpModel: UmbImagingResizeModel = { width: 300, height: 300, format: 'webp' };
			const pngModel: UmbImagingResizeModel = { width: 300, height: 300, format: 'png' };

			store.addCrop(mediaKey, 'http://example.com/image.webp', webpModel);
			store.addCrop(mediaKey, 'http://example.com/image.png', pngModel);

			store.clearCropByConfiguration(mediaKey, webpModel);

			expect(store.getCrop(mediaKey, webpModel)).to.be.undefined;
			expect(store.getCrop(mediaKey, pngModel)).to.equal('http://example.com/image.png');
		});
	});
});
