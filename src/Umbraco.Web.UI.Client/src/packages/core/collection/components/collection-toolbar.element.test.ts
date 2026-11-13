import { UmbCollectionToolbarElement } from './collection-toolbar.element.js';
import type { ManifestCollectionTextFilter } from '../filter/text/collection-text-filter.extension.js';
import type { UmbCollectionTextFilterApi } from '../filter/text/collection-text-filter-api.interface.js';
import './collection-filter-bundle.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { of } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-test-collection-toolbar-text-filter')
class UmbTestTextFilterElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestTextFilterApi extends UmbControllerBase implements UmbCollectionTextFilterApi {
	text = of(undefined);
	setText() {}
}

const TEXT_FILTER_ALIAS = 'Umb.Test.CollectionToolbar.TextFilter';

function textFilterManifest(conditions?: ManifestCollectionTextFilter['conditions']): ManifestCollectionTextFilter {
	return {
		type: 'collectionTextFilter',
		alias: TEXT_FILTER_ALIAS,
		name: TEXT_FILTER_ALIAS,
		elementName: 'umb-test-collection-toolbar-text-filter',
		api: UmbTestTextFilterApi,
		conditions,
	};
}

describe('UmbCollectionToolbarElement', () => {
	let element: UmbCollectionToolbarElement;

	const filterBundle = () =>
		element.shadowRoot?.querySelector('umb-collection-filter-bundle') ??
		element.shadowRoot?.querySelector('umb-test-collection-toolbar-text-filter umb-collection-filter-bundle');

	afterEach(() => {
		umbExtensionsRegistry.unregister(TEXT_FILTER_ALIAS);
	});

	it('renders the filter bundle on its own while no text filter is permitted', async () => {
		umbExtensionsRegistry.register(textFilterManifest([{ alias: 'Umb.Test.Condition.NeverPermitted' }]));
		element = await fixture(html`<umb-collection-toolbar></umb-collection-toolbar>`);

		await waitUntil(() => filterBundle());
		expect(filterBundle()?.closest('umb-test-collection-toolbar-text-filter')).to.be.null;
	});

	it('places the filter bundle in the filters slot of a permitted text filter', async () => {
		umbExtensionsRegistry.register(textFilterManifest());
		element = await fixture(html`<umb-collection-toolbar></umb-collection-toolbar>`);

		await waitUntil(() => filterBundle()?.parentElement instanceof UmbTestTextFilterElement);
		expect(filterBundle()?.getAttribute('slot')).to.equal('filters');
	});
});
