import { UmbTreePickerModalElement } from './tree-picker-modal.element.js';
import type { UmbTreePickerModalData } from './types.js';
import type { UmbTreeItemModelBase } from '../types.js';
import { expect, waitUntil } from '@open-wc/testing';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbTreeItemOpenEvent } from '../tree-item/events/tree-item-open.event.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSearchProvider, UmbSearchProvider } from '@umbraco-cms/backoffice/search';

const SEARCH_PROVIDER_ALIAS = 'Umb.Test.TreePickerModal.SearchProvider';
const TREE_ALIAS = 'Umb.Test.TreePickerModal.Tree';
const TREE_REPOSITORY_ALIAS = 'Umb.Test.TreePickerModal.TreeRepository';

const ROOT = { unique: null, entityType: 'test-root', name: 'Root' };

/** Knows one node, so anything else is a node the tree does not have. */
class UmbTestTreeRepository {
	async requestTreeRoot() {
		return { data: ROOT };
	}

	async requestTreeItemAncestors({ treeItem }: { treeItem: { unique: string } }) {
		return {
			data:
				treeItem.unique === 'known'
					? [{ unique: 'known', entityType: 'test-item', name: 'Known', hasChildren: true, isFolder: false }]
					: [],
		};
	}

	destroy() {}
}

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

		umbExtensionsRegistry.registerMany([
			{ type: 'repository', alias: TREE_REPOSITORY_ALIAS, name: 'Test Tree Repository', api: UmbTestTreeRepository },
			{ type: 'tree', alias: TREE_ALIAS, name: 'Test Tree', meta: { repositoryAlias: TREE_REPOSITORY_ALIAS } },
		] as Array<never>);

		element = new UmbTreePickerModalElement<UmbTreeItemModelBase>();
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(SEARCH_PROVIDER_ALIAS);
		umbExtensionsRegistry.unregister(TREE_ALIAS);
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		restoreErrorHandler();
	});

	function getTabs() {
		return element.shadowRoot?.querySelector('uui-tab-group');
	}

	function getCrumbs() {
		const breadcrumb = element.shadowRoot?.querySelector('umb-tree-item-picker-breadcrumb');
		return [...(breadcrumb?.shadowRoot?.querySelectorAll('uui-breadcrumb-item') ?? [])];
	}

	function getNotFound() {
		return element.shadowRoot?.querySelector('#not-found');
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

	// The location manager reports a node it cannot resolve as `null`, which must not be read as "still loading" — the
	// modal has to say so rather than leave the tree standing at a level the user did not browse to.
	describe('browsing to a node the tree does not have', () => {
		beforeEach(async () => {
			await setup({ treeAlias: TREE_ALIAS });
			// The initial trail has to be in place first, or the load that establishes it lands after the navigation.
			await waitUntil(() => getCrumbs().length === 1, 'the root breadcrumb was never loaded');

			element.dispatchEvent(new UmbTreeItemOpenEvent({ unique: 'gone', entityType: 'test-item' }));
			await waitUntil(() => !!getNotFound(), 'the not-found state was never rendered');
		});

		it('renders the not-found state instead of the tree', () => {
			expect(getPane('browse')?.querySelector('umb-tree')).to.not.exist;
		});

		it('keeps the breadcrumb, so the user can browse back out', () => {
			expect(getPane('browse')?.querySelector('umb-tree-item-picker-breadcrumb')).to.exist;
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
