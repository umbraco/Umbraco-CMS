import { UmbTreePickerModalElement } from './tree-picker-modal.element.js';
import type { UmbTreePickerModalData } from './types.js';
import type { UmbTreeItemModelBase } from '../types.js';
import { expect, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSearchProvider, UmbSearchProvider } from '@umbraco-cms/backoffice/search';

const SEARCH_PROVIDER_ALIAS = 'Umb.Test.TreePickerModal.SearchProvider';

class UmbTestSearchProvider implements UmbSearchProvider {
	async search() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

describe('UmbTreePickerModalElement', () => {
	let element: UmbTreePickerModalElement<UmbTreeItemModelBase>;
	let restoreErrorHandler: () => void;

	beforeEach(() => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();

		umbExtensionsRegistry.register({
			type: 'searchProvider',
			alias: SEARCH_PROVIDER_ALIAS,
			name: 'Test Search Provider',
			api: UmbTestSearchProvider,
		} as ManifestSearchProvider);

		element = new UmbTreePickerModalElement<UmbTreeItemModelBase>();
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

	async function setup(data?: Partial<UmbTreePickerModalData<UmbTreeItemModelBase>>) {
		element.data = data as UmbTreePickerModalData<UmbTreeItemModelBase>;
		document.body.appendChild(element);
		await element.updateComplete;
	}

	async function setupWithSearch(data?: Partial<UmbTreePickerModalData<UmbTreeItemModelBase>>) {
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

		it('shows the browse pane again when the browse tab is clicked', async () => {
			await clickTab('search');
			await clickTab('browse');

			expect(getPane('browse')?.hidden).to.be.false;
			expect(getPane('search')?.hidden).to.be.true;
		});

		it('keeps the tree mounted while the search tab is active', async () => {
			const tree = getPane('browse')?.querySelector('umb-tree');

			await clickTab('search');

			expect(getPane('browse')?.querySelector('umb-tree')).to.equal(tree);
		});
	});

	describe('selection', () => {
		beforeEach(async () => {
			await setupWithSearch({ multiple: true });
		});

		it('keeps the selection when switching tabs', async () => {
			expect(element.shadowRoot?.querySelector('#selection-info')).to.not.exist;

			getPane('browse')?.querySelector('umb-tree')?.dispatchEvent(new UmbSelectedEvent('item-1'));
			await element.updateComplete;

			await clickTab('search');
			await clickTab('browse');

			expect(element.shadowRoot?.querySelector('#selection-info')).to.exist;
		});
	});
});
