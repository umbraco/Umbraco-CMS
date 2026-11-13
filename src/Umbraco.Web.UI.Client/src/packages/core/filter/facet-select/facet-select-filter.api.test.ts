import { UmbFacetSelectFilterApi } from './facet-select-filter.api.js';
import type { MetaFilterFacetSelect } from './types.js';
import type { ManifestFilterBase } from '../filter.extension.js';
import { UmbFilterContext } from '../filter.context.js';
import { UmbFilterManagerContext } from '../filter-manager.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

@customElement('test-facet-select-filter-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const ENTITY_TYPE = 'test-facet-select-item';
const FILTER_ALIAS = 'Umb.Test.Filter.FacetSelect';
const ITEM_REPOSITORY_ALIAS = 'Umb.Test.Repository.FacetSelectItem';

const items: Array<UmbItemModel> = [
	{ entityType: ENTITY_TYPE, unique: 'a', name: 'Alpha' },
	{ entityType: ENTITY_TYPE, unique: 'b', name: 'Beta' },
	{ entityType: ENTITY_TYPE, unique: 'c', name: 'Gamma' },
];

class TestItemRepository extends UmbControllerBase {
	async requestItems(uniques: Array<string>) {
		return { data: items.filter((item) => uniques.includes(item.unique)) };
	}
}

const manifest = {
	type: 'umbTestFilter',
	kind: 'facetSelect',
	alias: FILTER_ALIAS,
	name: FILTER_ALIAS,
	meta: { label: 'Test', itemRepositoryAlias: ITEM_REPOSITORY_ALIAS },
} as unknown as ManifestFilterBase & { meta: MetaFilterFacetSelect };

function waitFor<T>(observable: Observable<T>, predicate: (value: T) => boolean): Promise<T> {
	return new Promise((resolve) => {
		const subscription = observable.subscribe((value) => {
			if (!predicate(value)) return;
			queueMicrotask(() => subscription.unsubscribe());
			resolve(value);
		});
	});
}

describe('UmbFacetSelectFilterApi', () => {
	let host: TestHostElement;
	let manager: UmbFilterManagerContext;
	let api: UmbFacetSelectFilterApi;

	before(() => {
		umbExtensionsRegistry.register({
			type: 'repository',
			alias: ITEM_REPOSITORY_ALIAS,
			name: ITEM_REPOSITORY_ALIAS,
			api: TestItemRepository,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(ITEM_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		host = new TestHostElement();
		document.body.appendChild(host);
		manager = new UmbFilterManagerContext(host);
		new UmbFilterContext(host).setAlias(FILTER_ALIAS);
		api = new UmbFacetSelectFilterApi(host);
		api.manifest = manifest;
	});

	afterEach(() => {
		host.destroy();
		host.remove();
	});

	it('names the options of the faceted result with the items of its item repository', async () => {
		manager.setFacetedResults([
			{
				alias: FILTER_ALIAS,
				result: [
					{ entityType: ENTITY_TYPE, unique: 'a', count: 2 },
					{ entityType: ENTITY_TYPE, unique: 'b', count: 1 },
				],
			},
		]);

		const options = await waitFor(api.options, (options) => options.length === 2 && !!options[1].name);
		expect(options.map(({ unique, name, count }) => ({ unique, name, count }))).to.deep.equal([
			{ unique: 'a', name: 'Alpha', count: 2 },
			{ unique: 'b', name: 'Beta', count: 1 },
		]);
	});

	it('resolves selected values into items, also values the faceted result does not have', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: [{ entityType: ENTITY_TYPE, unique: 'a', count: 2 }] }]);
		manager.setFilterValues(FILTER_ALIAS, [{ unique: 'c', value: 'c' }]);

		const valueItems = await waitFor(api.valueItems, (valueItems) => valueItems.length === 1);
		expect(valueItems[0].name).to.equal('Gamma');
	});

	it('has no options when the faceted result has no values for the filter', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: [{ entityType: ENTITY_TYPE, unique: 'a', count: 2 }] }]);
		await waitFor(api.options, (options) => options.length === 1);

		manager.setFacetedResults([]);

		const options = await waitFor(api.options, (options) => options.length === 0);
		expect(options).to.deep.equal([]);
	});
});
