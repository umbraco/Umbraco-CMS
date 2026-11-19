import { UmbDocumentLinkPickerContext } from './document-link-picker.context.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '@umbraco-cms/backoffice/document';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

@customElement('test-document-link-picker-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface UmbTestSearchCall {
	query?: string;
	culture?: string | null;
}

const getConfigCulture = (context: UmbDocumentLinkPickerContext) =>
	(context.search.getConfig()?.queryParams as { culture?: string | null } | undefined)?.culture;

// The picker context configures search on construction, which resolves this provider from the
// registry. Registering a stub keeps the run free of "Failed to get manifest by alias" noise, and
// lets the tests observe which args each search call was made with.
describe('UmbDocumentLinkPickerContext', () => {
	// The inherited variant context (e.g. from the workspace that opened this picker) lives on an
	// ancestor host, distinct from the picker's own host - mirroring how the modal element is a
	// descendant of whatever opened it, not the same node.
	let inheritedContextHost: UmbTestControllerHostElement;
	let pickerHost: UmbTestControllerHostElement;
	let searchCalls: Array<UmbTestSearchCall>;

	const searchProviderManifest = {
		type: 'searchProvider' as const,
		alias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		name: 'Test Document Search Provider',
		api: class {
			destroy() {}
			async search(args: UmbTestSearchCall) {
				searchCalls.push(args);
				return { data: { items: [], total: 0 } };
			}
		},
		meta: { label: 'Documents' },
	};

	before(() => {
		umbExtensionsRegistry.register(searchProviderManifest as never);
	});

	after(() => {
		umbExtensionsRegistry.unregister(searchProviderManifest.alias);
	});

	beforeEach(() => {
		searchCalls = [];
		inheritedContextHost = new UmbTestControllerHostElement();
		pickerHost = new UmbTestControllerHostElement();
		inheritedContextHost.appendChild(pickerHost);
		document.body.appendChild(inheritedContextHost);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('search culture scoping', () => {
		it('scopes search to the inherited variant culture before any language is explicitly picked', async () => {
			const variantContext = new UmbVariantContext(inheritedContextHost);
			await variantContext.setCulture('da-DK');

			const context = new UmbDocumentLinkPickerContext(pickerHost);
			await aTimeout(0);

			expect(getConfigCulture(context)).to.equal('da-DK');
		});

		it('re-scopes search to the explicitly picked language', async () => {
			const variantContext = new UmbVariantContext(inheritedContextHost);
			await variantContext.setCulture('da-DK');

			const context = new UmbDocumentLinkPickerContext(pickerHost);
			await aTimeout(0);

			await context.setCulture('en-US');

			expect(getConfigCulture(context)).to.equal('en-US');
		});

		it('falls back to the inherited culture once the explicit pick is cleared', async () => {
			const variantContext = new UmbVariantContext(inheritedContextHost);
			await variantContext.setCulture('da-DK');

			const context = new UmbDocumentLinkPickerContext(pickerHost);
			await aTimeout(0);

			await context.setCulture('en-US');
			await context.setCulture(null);

			expect(getConfigCulture(context)).to.equal('da-DK');
		});

		it('re-runs an already active search when the culture changes', async () => {
			const variantContext = new UmbVariantContext(inheritedContextHost);
			await variantContext.setCulture('da-DK');

			const context = new UmbDocumentLinkPickerContext(pickerHost);
			await aTimeout(0);

			context.search.updateQuery({ query: 'home' });
			context.search.search();
			await aTimeout(400);

			expect(searchCalls).to.have.lengthOf(1);
			expect(searchCalls[0].culture).to.equal('da-DK');

			await context.setCulture('en-US');
			await aTimeout(400);

			expect(searchCalls).to.have.lengthOf(2);
			expect(searchCalls[1].culture).to.equal('en-US');
		});

		it('does not trigger a search when the culture changes with no active query', async () => {
			const context = new UmbDocumentLinkPickerContext(pickerHost);
			await aTimeout(0);

			await context.setCulture('en-US');
			await aTimeout(400);

			expect(searchCalls).to.have.lengthOf(0);
		});
	});
});
