import { UmbFilterManagerContext } from './filter-manager.context.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-filter-manager-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbFilterManagerContext', () => {
	let host: TestHostElement;
	let manager: UmbFilterManagerContext;

	beforeEach(() => {
		host = document.createElement('test-filter-manager-host') as TestHostElement;
		document.body.appendChild(host);
		manager = new UmbFilterManagerContext(host);
	});

	afterEach(() => {
		manager.destroy();
		document.body.innerHTML = '';
	});

	it('counts every active value in totalActiveFilters, not the number of filters', () => {
		manager.setFilterValues('color', [
			{ unique: 'black', value: 'black' },
			{ unique: 'white', value: 'white' },
		]);
		manager.setFilterValues('size', [{ unique: 'large', value: 'large' }]);

		let total: number | undefined;
		manager.totalActiveFilters.subscribe((value) => (total = value)).unsubscribe();
		expect(total).to.equal(3);
	});
});
