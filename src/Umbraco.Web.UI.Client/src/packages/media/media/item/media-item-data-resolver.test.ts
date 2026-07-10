import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbMediaItemDataResolver } from './media-item-data-resolver.js';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

@customElement('umb-test-media-item-data-resolver-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaItemDataResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbMediaItemDataResolver<any>;
	let variantContext: UmbVariantContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		resolver = new UmbMediaItemDataResolver(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('Public API', () => {
		it('has a name observable', () => {
			expect(resolver).to.have.property('name');
		});

		it('has setData method', () => {
			expect(resolver).to.have.property('setData').that.is.a('function');
		});

		it('has getData method', () => {
			expect(resolver).to.have.property('getData').that.is.a('function');
		});

		it('has getName method', () => {
			expect(resolver).to.have.property('getName').that.is.a('function');
		});

		it('has getIcon method', () => {
			expect(resolver).to.have.property('getIcon').that.is.a('function');
		});
	});

	describe('name fallback behavior', () => {
		it('should use current variant name when available', async () => {
			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [{ culture: 'en-US', name: 'English Title' }],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('English Title');
		});

		it('should fall back to fallback culture name in parentheses', async () => {
			await variantContext.setCulture('de-DE');
			await variantContext.setFallbackCulture('en-US');

			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [
					{ culture: 'en-US', name: 'English Title' },
					{ culture: 'de-DE', name: undefined },
				],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('(English Title)');
		});

		it('should fall back to first variant with name when current and fallback have no name', async () => {
			await variantContext.setCulture('de-DE');
			await variantContext.setFallbackCulture('es-ES');

			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [
					{ culture: 'de-DE', name: undefined },
					{ culture: 'es-ES', name: undefined },
					{ culture: 'fr-FR', name: 'Titre Français' },
				],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('(Titre Français)');
		});

		it('should return (Untitled) when no variants have names', async () => {
			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [
					{ culture: 'en-US', name: undefined },
					{ culture: 'de-DE', name: undefined },
				],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('(Untitled)');
		});

		it('should use the invariant variant name when content is invariant', async () => {
			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [{ culture: null, name: 'Invariant Name' }],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('Invariant Name');
		});
	});

	describe('icon', () => {
		it('should resolve the icon from the media type', async () => {
			const mockData = {
				entityType: 'media',
				unique: 'test-123',
				mediaType: { unique: 'mt-1', icon: 'icon-picture', collection: null },
				variants: [{ culture: 'en-US', name: 'English Title' }],
			};

			resolver.setData(mockData);

			const icon = await resolver.getIcon();
			expect(icon).to.equal('icon-picture');
		});
	});
});
