import { UmbCollectionItemPickerModalElement } from './collection-item-picker-modal.element.js';
import type { UmbCollectionItemPickerModalData } from './types.js';
import { expect, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSearchProvider, UmbSearchProvider } from '@umbraco-cms/backoffice/search';

const SEARCH_PROVIDER_ALIAS = 'Umb.Test.CollectionItemPickerModal.SearchProvider';

class UmbTestSearchProvider implements UmbSearchProvider {
	async search() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

describe('UmbCollectionItemPickerModalElement', () => {
	let element: UmbCollectionItemPickerModalElement;
	let restoreErrorHandler: () => void;

	beforeEach(() => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();

		umbExtensionsRegistry.register({
			type: 'searchProvider',
			alias: SEARCH_PROVIDER_ALIAS,
			name: 'Test Search Provider',
			api: UmbTestSearchProvider,
		} as ManifestSearchProvider);

		element = new UmbCollectionItemPickerModalElement();
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(SEARCH_PROVIDER_ALIAS);
		restoreErrorHandler();
	});

	function getTabs() {
		return element.shadowRoot?.querySelector('uui-tab-group');
	}

	function getPane(id: 'browse' | 'search') {
		return element.shadowRoot?.querySelector<HTMLElement>(`#${id}`);
	}

	async function clickTab(tab: 'browse' | 'search') {
		element.shadowRoot?.querySelector<HTMLElement>(`[data-mark="picker:tab:${tab}"]`)?.click();
		await element.updateComplete;
	}

	async function setup(data?: Partial<UmbCollectionItemPickerModalData>) {
		element.data = {
			collection: { menuAlias: 'Umb.Test.CollectionMenu' },
			...data,
		} as UmbCollectionItemPickerModalData;
		document.body.appendChild(element);
		await element.updateComplete;
	}

	async function setupWithSearch(data?: Partial<UmbCollectionItemPickerModalData>) {
		await setup({ search: { providerAlias: SEARCH_PROVIDER_ALIAS }, ...data });
		await waitUntil(() => !!getTabs(), 'tab group was never rendered');
	}

	describe('without search configured', () => {
		beforeEach(async () => {
			await setup();
		});

		it('renders no tabs', () => {
			expect(getTabs()).to.not.exist;
		});

		it('renders the browse pane', () => {
			expect(getPane('browse')?.hidden).to.be.false;
		});
	});

	describe('with search configured', () => {
		beforeEach(async () => {
			await setupWithSearch();
		});

		it('renders a browse and a search tab', () => {
			expect(getTabs()?.querySelectorAll('uui-tab')).to.have.lengthOf(2);
		});

		it('starts on the browse tab', () => {
			expect(getPane('browse')?.hidden).to.be.false;
			expect(getPane('search')?.hidden).to.be.true;
		});

		it('shows the search pane when the search tab is clicked', async () => {
			await clickTab('search');

			expect(getPane('browse')?.hidden).to.be.true;
			expect(getPane('search')?.hidden).to.be.false;
		});

		it('keeps the collection menu mounted while the search tab is active', async () => {
			const menu = getPane('browse')?.querySelector('umb-collection-menu');

			await clickTab('search');

			expect(getPane('browse')?.querySelector('umb-collection-menu')).to.equal(menu);
		});
	});

	describe('selection', () => {
		beforeEach(async () => {
			await setupWithSearch({ multiple: true });
		});

		it('keeps the selection when switching tabs', async () => {
			expect(element.shadowRoot?.querySelector('#selection-info')).to.not.exist;

			getPane('browse')?.querySelector('umb-collection-menu')?.dispatchEvent(new UmbSelectedEvent('item-1'));
			await element.updateComplete;

			await clickTab('search');
			await clickTab('browse');

			expect(element.shadowRoot?.querySelector('#selection-info')).to.exist;
		});
	});
});
