import { UmbFilterElement } from './filter.element.js';
import { UmbFilterManagerContext } from './filter-manager.context.js';
import { UMB_FILTER_CONTEXT } from './filter.context-token.js';
import type { ManifestFilterBase } from './filter.extension.js';
import type { UmbFilterExtensionElement } from './filter-extension-element.interface.js';
import type { UmbFilterExtensionApi } from './filter-extension-api.interface.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('test-filter-element-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-filter-element-extension')
class TestFilterExtensionElement
	extends UmbControllerHostElementMixin(HTMLElement)
	implements UmbFilterExtensionElement
{
	api?: UmbFilterExtensionApi;
	manifest?: ManifestFilterBase;
}

class TestFilterExtensionApi extends UmbControllerBase implements UmbFilterExtensionApi {
	manifest?: ManifestFilterBase;
	filterAlias?: string;

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_FILTER_CONTEXT, (context) => (this.filterAlias = context?.getAlias()));
	}
}

const FILTER_ALIAS = 'Umb.Test.Filter';

const manifest = {
	type: 'umbTestFilter',
	alias: FILTER_ALIAS,
	name: FILTER_ALIAS,
	element: TestFilterExtensionElement,
	api: TestFilterExtensionApi,
	meta: { label: 'Colour' },
} as unknown as ManifestFilterBase;

describe('UmbFilterElement', () => {
	let host: TestHostElement;
	let manager: UmbFilterManagerContext;
	let element: UmbFilterElement;

	const clearButton = () => element.shadowRoot?.querySelector('uui-button');
	const extension = () =>
		element.shadowRoot?.querySelector<TestFilterExtensionElement>('test-filter-element-extension') ?? undefined;

	beforeEach(async () => {
		host = document.createElement('test-filter-element-host') as TestHostElement;
		document.body.appendChild(host);
		manager = new UmbFilterManagerContext(host);
		element = new UmbFilterElement();
		element.manifest = manifest;
		host.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		host.destroy();
		host.remove();
	});

	it('shows the label of its filter', () => {
		expect(element.shadowRoot?.textContent).to.contain('Colour');
	});

	it('renders the element of its filter with the manifest and api', async () => {
		await waitUntil(() => extension());

		expect(extension()?.manifest).to.equal(manifest);
		expect(extension()?.api).to.be.instanceOf(TestFilterExtensionApi);
		expect(extension()?.api?.manifest).to.equal(manifest);
	});

	it('provides the filter context of its filter to the extension', async () => {
		await waitUntil(() => (extension()?.api as TestFilterExtensionApi | undefined)?.filterAlias);

		expect((extension()?.api as TestFilterExtensionApi).filterAlias).to.equal(FILTER_ALIAS);
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
