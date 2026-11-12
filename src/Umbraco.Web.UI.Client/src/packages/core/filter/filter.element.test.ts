import { UmbFilterElement } from './filter.element.js';
import { UmbFilterManagerContext } from './filter-manager.context.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-filter-element-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const FILTER_ALIAS = 'Umb.Test.Filter';

describe('UmbFilterElement', () => {
	let host: TestHostElement;
	let manager: UmbFilterManagerContext;
	let element: UmbFilterElement;

	const clearButton = () => element.shadowRoot?.querySelector('uui-button');

	beforeEach(async () => {
		host = document.createElement('test-filter-element-host') as TestHostElement;
		document.body.appendChild(host);
		manager = new UmbFilterManagerContext(host);
		element = new UmbFilterElement();
		element.alias = FILTER_ALIAS;
		element.label = 'Colour';
		host.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		host.destroy();
		host.remove();
	});

	it('shows its label', () => {
		expect(element.shadowRoot?.textContent).to.contain('Colour');
	});

	it('disables the clear button while the filter has no values', async () => {
		expect(clearButton()?.disabled).to.be.true;

		manager.setFilterValues(FILTER_ALIAS, [{ unique: 'red', value: 'red' }]);
		await waitUntil(() => clearButton()?.disabled === false);

		manager.clearFilter(FILTER_ALIAS);
		await waitUntil(() => clearButton()?.disabled === true);
	});

	it('clears only its own filter', async () => {
		manager.setFilterValues(FILTER_ALIAS, [{ unique: 'red', value: 'red' }]);
		manager.setFilterValues('Umb.Test.OtherFilter', [{ unique: 'large', value: 'large' }]);
		await waitUntil(() => clearButton()?.disabled === false);

		clearButton()!.click();
		await waitUntil(() => clearButton()?.disabled === true);

		expect(await firstValueFrom(manager.activeFiltersByAlias(FILTER_ALIAS))).to.be.empty;
		expect(await firstValueFrom(manager.activeFiltersByAlias('Umb.Test.OtherFilter'))).to.have.length(1);
	});
});
