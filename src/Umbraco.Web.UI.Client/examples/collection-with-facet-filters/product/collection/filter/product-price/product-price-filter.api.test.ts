import { ExampleProductPriceFilterApi } from './product-price-filter.api.js';
import type { ExampleProductPriceFilterValueModel } from './types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbFilterContext, UmbFilterManagerContext } from '@umbraco-cms/backoffice/filter';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { expect } from '@open-wc/testing';

@customElement('example-product-price-filter-api-test-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const FILTER_ALIAS = 'Example.Test.Filter.ProductPrice';

function limit(key: 'min' | 'max', amount: number) {
	return { unique: key, value: { key, amount } satisfies ExampleProductPriceFilterValueModel };
}

describe('ExampleProductPriceFilterApi', () => {
	let host: TestHostElement;
	let manager: UmbFilterManagerContext;
	let api: ExampleProductPriceFilterApi;

	function waitForSelection(min: number, max: number): Promise<void> {
		return new Promise((resolve) => {
			const subscription = observeMultiple([api.currentMin, api.currentMax]).subscribe(([currentMin, currentMax]) => {
				if (currentMin !== min || currentMax !== max) return;
				queueMicrotask(() => subscription.unsubscribe());
				resolve();
			});
		});
	}

	async function activeLimits(): Promise<Array<string>> {
		const filters = await firstValueFrom(manager.activeFiltersByAlias(FILTER_ALIAS));
		return filters.map((filter) => filter.unique);
	}

	beforeEach(() => {
		host = new TestHostElement();
		document.body.appendChild(host);
		manager = new UmbFilterManagerContext(host);
		new UmbFilterContext(host).setAlias(FILTER_ALIAS);
		api = new ExampleProductPriceFilterApi(host);
	});

	afterEach(() => {
		host.destroy();
		host.remove();
	});

	it('follows the price facet while no price limit is set', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 20, max: 120 } }]);
		await waitForSelection(20, 120);

		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 45, max: 60 } }]);
		await waitForSelection(45, 60);
	});

	it('opens the end of a removed limit up to the price facet', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 20, max: 120 } }]);
		manager.setFilterValues(FILTER_ALIAS, [limit('min', 30), limit('max', 80)]);
		await waitForSelection(30, 80);

		manager.clearActiveFilter(FILTER_ALIAS, 'min');
		await waitForSelection(20, 80);
	});

	it('keeps the selected range within the price facet when a limit lies outside it', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 20, max: 120 } }]);
		manager.setFilterValues(FILTER_ALIAS, [limit('min', 60), limit('max', 120)]);
		await waitForSelection(60, 120);

		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 75, max: 85 } }]);
		await waitForSelection(75, 85);
	});

	it('sets a limit only for an end that differs from the price facet', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 20, max: 120 } }]);
		await waitForSelection(20, 120);

		api.setValue(75, 120);
		await waitForSelection(75, 120);
		expect(await activeLimits()).to.deep.equal(['min']);

		api.setValue(20, 120);
		await waitForSelection(20, 120);
		expect(await activeLimits()).to.deep.equal([]);
	});

	it('returns to the price facet when the limits are cleared', async () => {
		manager.setFacetedResults([{ alias: FILTER_ALIAS, result: { min: 20, max: 120 } }]);
		manager.setFilterValues(FILTER_ALIAS, [limit('min', 30), limit('max', 80)]);
		await waitForSelection(30, 80);

		manager.clearFilter(FILTER_ALIAS);
		await waitForSelection(20, 120);
	});
});
