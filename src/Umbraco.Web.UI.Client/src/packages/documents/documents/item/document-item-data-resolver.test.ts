import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbDocumentItemDataResolver } from './document-item-data-resolver.js';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

// ============================================
// SETUP: Create a test host element
// ============================================
// Controllers need a "host" element to attach to.
// This creates a simple HTML element that can host controllers.
@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbDocumentItemDataResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbDocumentItemDataResolver<any>;
	let variantContext: UmbVariantContext;

	// ============================================
	// beforeEach: Runs before EACH test
	// ============================================
	beforeEach(async () => {
		// 1. Create a host element
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		// 2. Create and set up the variant context
		//    This tells the resolver which culture to use
		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		// 3. Create the resolver (it will consume the context automatically)
		resolver = new UmbDocumentItemDataResolver(hostElement);
	});

	// ============================================
	// afterEach: Cleanup after EACH test
	// ============================================
	afterEach(() => {
		document.body.innerHTML = '';
	});

	// ============================================
	// Test Group: Public API
	// ============================================
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

	// ============================================
	// Test Group: Name Fallback Behavior (THE BUG FIX)
	// ============================================
	describe('name fallback behavior', () => {
		it('should use current variant name when available', async () => {
			// ARRANGE: Create mock data with a name for current culture
			const mockData = {
				entityType: 'document',
				unique: 'test-123',
				documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
				isTrashed: false,
				variants: [{ culture: 'en-US', name: 'English Title', state: 'Published' }],
			};

			// ACT: Set the data
			resolver.setData(mockData);

			// ASSERT: Name should be the variant name
			const name = await resolver.getName();
			expect(name).to.equal('English Title');
		});

		it('should fall back to fallback culture name in parentheses', async () => {
			// ARRANGE: Current culture (de-DE) has no name, fallback (en-US) has name
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

			// ACT
			resolver.setData(mockData);

			// ASSERT: Should use fallback name in parentheses
			const name = await resolver.getName();
			expect(name).to.equal('(English Title)');
		});

		it('should fall back to first variant with name when current and fallback have no name', async () => {
			// ARRANGE: This is THE BUG FIX test!
			// - Current culture (de-DE) has no name
			// - Fallback culture (es-ES) has no name
			// - But fr-FR has a name
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

			// ACT
			resolver.setData(mockData);

			// ASSERT: Should find and use the French name (first with a value)
			const name = await resolver.getName();
			expect(name).to.equal('(Titre Français)');
		});

		it('should return (Untitled) when no variants have names', async () => {
			// ARRANGE: No variant has a name
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

			// ACT
			resolver.setData(mockData);

			// ASSERT: Should show (Untitled) placeholder
			const name = await resolver.getName();
			expect(name).to.equal('(Untitled)');
		});
	});
});
