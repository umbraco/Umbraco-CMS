import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbDocumentItemDataResolver } from './document-item-data-resolver.js';
import { UmbDocumentVariantState } from '../variant-state.js';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Reads the current value of an observable once (resolves synchronously with the latest emission).
function observeFirst<T>(observable: Observable<T>): Promise<T> {
	return new Promise<T>((resolve) => {
		const subscription = observable.subscribe((value) => {
			resolve(value);
			queueMicrotask(() => subscription.unsubscribe());
		});
	});
}

function makeData(overrides: Record<string, unknown> = {}) {
	return {
		entityType: 'document',
		unique: 'test-123',
		documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
		isTrashed: false,
		flags: [],
		variants: [{ culture: 'en-US', segment: null, name: 'English Title', state: UmbDocumentVariantState.PUBLISHED }],
		...overrides,
	};
}

describe('UmbDocumentItemDataResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbDocumentItemDataResolver<any>;
	let variantContext: UmbVariantContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		resolver = new UmbDocumentItemDataResolver(hostElement);
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
	});

	describe('name fallback behavior', () => {
		it('should use current variant name when available', async () => {
			const mockData = {
				entityType: 'document',
				unique: 'test-123',
				documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
				isTrashed: false,
				variants: [{ culture: 'en-US', name: 'English Title', state: 'Published' }],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('English Title');
		});

		it('should fall back to fallback culture name in parentheses', async () => {
			await variantContext.setCulture('de-DE');
			await variantContext.setFallbackCulture('en-US');

			const mockData = {
				entityType: 'document',
				unique: 'test-123',
				documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
				isTrashed: false,
				variants: [
					{ culture: 'en-US', name: 'English Title', state: 'Published' },
					{ culture: 'de-DE', name: undefined, state: 'NotCreated' },
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
				entityType: 'document',
				unique: 'test-123',
				documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
				isTrashed: false,
				variants: [
					{ culture: 'de-DE', name: undefined, state: 'NotCreated' },
					{ culture: 'es-ES', name: undefined, state: 'NotCreated' },
					{ culture: 'fr-FR', name: 'Titre Français', state: 'Published' },
				],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('(Titre Français)');
		});

		it('should return (Untitled) when no variants have names', async () => {
			const mockData = {
				entityType: 'document',
				unique: 'test-123',
				documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
				isTrashed: false,
				variants: [
					{ culture: 'en-US', name: undefined, state: 'NotCreated' },
					{ culture: 'de-DE', name: undefined, state: 'NotCreated' },
				],
			};

			resolver.setData(mockData);

			const name = await resolver.getName();
			expect(name).to.equal('(Untitled)');
		});
	});

	describe('state and draft resolution', () => {
		it('resolves the current variant state', async () => {
			resolver.setData(
				makeData({ variants: [{ culture: 'en-US', name: 'x', state: UmbDocumentVariantState.PUBLISHED }] }),
			);
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.PUBLISHED);
		});

		it('marks the item as a draft when the current variant is a draft', async () => {
			resolver.setData(makeData({ variants: [{ culture: 'en-US', name: 'x', state: UmbDocumentVariantState.DRAFT }] }));
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.DRAFT);
			expect(await resolver.getIsDraft()).to.equal(true);
		});

		it('is not a draft when the current variant is published', async () => {
			resolver.setData(
				makeData({ variants: [{ culture: 'en-US', name: 'x', state: UmbDocumentVariantState.PUBLISHED }] }),
			);
			expect(await resolver.getIsDraft()).to.equal(false);
		});

		it('falls back to NotCreated when the current culture has no variant', async () => {
			await variantContext.setCulture('de-DE');
			await variantContext.setFallbackCulture('en-US');
			resolver.setData(
				makeData({ variants: [{ culture: 'en-US', name: 'English', state: UmbDocumentVariantState.PUBLISHED }] }),
			);
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.NOT_CREATED);
		});
	});

	describe('icon', () => {
		it('resolves the icon from the document type', async () => {
			resolver.setData(makeData({ documentType: { unique: 'dt-1', icon: 'icon-article', collection: null } }));
			expect(await resolver.getIcon()).to.equal('icon-article');
		});
	});

	describe('invariant documents', () => {
		it('uses the single invariant variant for name and state', async () => {
			resolver.setData(
				makeData({
					variants: [
						{ culture: null, segment: null, name: 'Invariant Name', state: UmbDocumentVariantState.PUBLISHED },
					],
				}),
			);
			expect(await resolver.getName()).to.equal('Invariant Name');
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.PUBLISHED);
		});
	});

	describe('reactive culture switching', () => {
		it('updates name and state when the display culture changes at runtime', async () => {
			await variantContext.setFallbackCulture('en-US');
			resolver.setData(
				makeData({
					variants: [
						{ culture: 'en-US', segment: null, name: 'English Title', state: UmbDocumentVariantState.PUBLISHED },
						{ culture: 'da-DK', segment: null, name: 'Dansk Titel', state: UmbDocumentVariantState.DRAFT },
					],
				}),
			);

			expect(await resolver.getName()).to.equal('English Title');
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.PUBLISHED);

			await variantContext.setCulture('da-DK');

			expect(await resolver.getName()).to.equal('Dansk Titel');
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.DRAFT);
		});
	});

	describe('current culture lookup', () => {
		it('getCulture returns the resolved display culture', async () => {
			resolver.setData(makeData());
			// Awaiting a variant-aware value guarantees the variant context has been consumed.
			await resolver.getName();
			expect(resolver.getCulture()).to.equal('en-US');
		});
	});

	describe('dates and flags', () => {
		it('resolves the create date from the current variant', async () => {
			const createDate = new Date('2024-01-01T00:00:00Z');
			resolver.setData(
				makeData({
					variants: [{ culture: 'en-US', name: 'x', state: UmbDocumentVariantState.PUBLISHED, createDate }],
				}),
			);
			expect(await resolver.getCreateDate()).to.equal(createDate);
		});

		it('combines document-level and current-variant flags', async () => {
			resolver.setData(
				makeData({
					flags: [{ alias: 'doc-flag' }],
					variants: [
						{
							culture: 'en-US',
							name: 'x',
							state: UmbDocumentVariantState.PUBLISHED,
							flags: [{ alias: 'variant-flag' }],
						},
					],
				}),
			);
			// Ensure variant-aware values are computed before reading the flags observable.
			await resolver.getName();
			const flags = await observeFirst(resolver.flags);
			expect(flags.map((flag) => flag.alias)).to.have.members(['doc-flag', 'variant-flag']);
		});
	});

	describe('pass-through item fields', () => {
		it('resolves entityType, unique and isTrashed', async () => {
			resolver.setData(makeData({ unique: 'abc', isTrashed: true }));
			expect(await resolver.getEntityType()).to.equal('document');
			expect(await resolver.getUnique()).to.equal('abc');
			expect(await resolver.getIsTrashed()).to.equal(true);
		});

		it('reports hasCollection based on the document type', () => {
			resolver.setData(
				makeData({ documentType: { unique: 'dt-1', icon: 'icon-document', collection: { unique: 'col-1' } } }),
			);
			expect(resolver.getHasCollection()).to.equal(true);
		});

		it('hasCollection is false when the document type has no collection', () => {
			resolver.setData(makeData());
			expect(resolver.getHasCollection()).to.equal(false);
		});
	});
});
