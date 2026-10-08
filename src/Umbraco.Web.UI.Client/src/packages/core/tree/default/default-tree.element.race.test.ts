import type { UmbDefaultTreeElement } from './default-tree.element.js';
import type { ManifestTreeView } from '../view/tree-view.extension.js';
import type { UmbTreeItemModel } from '../types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import './default-tree.element.js';

// Tree views are created from their manifest on demand, so two view changes in quick succession leave two element
// creations in flight at once. A view backed by a lazy import resolves slower than one backed by a statically imported
// constructor, so the creations can settle in the opposite order to the changes that started them.

@customElement('umb-test-race-slow-tree-view')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestRaceSlowTreeViewElement extends HTMLElement {}

@customElement('umb-test-race-fast-tree-view')
class UmbTestRaceFastTreeViewElement extends HTMLElement {}

class UmbFakeTreeViewManager {
	#currentView = new UmbObjectState<ManifestTreeView | undefined>(undefined);
	currentView = this.#currentView.asObservable();

	setCurrentView(view: ManifestTreeView | undefined) {
		this.#currentView.setValue(view);
	}

	getCurrentView() {
		return this.#currentView.getValue();
	}
}

class UmbFakeTreeContext {
	view = new UmbFakeTreeViewManager();

	#treeRoot = new UmbObjectState<unknown>(undefined);
	treeRoot = this.#treeRoot.asObservable();

	#rootItems = new UmbArrayState<UmbTreeItemModel>([], (x) => x.unique);
	rootItems = this.#rootItems.asObservable();

	#isLoading = new UmbBooleanState(undefined);
	isLoading = this.#isLoading.asObservable();

	#isLoadingPrevChildren = new UmbBooleanState(undefined);
	isLoadingPrevChildren = this.#isLoadingPrevChildren.asObservable();

	#isLoadingNextChildren = new UmbBooleanState(undefined);
	isLoadingNextChildren = this.#isLoadingNextChildren.asObservable();

	#currentPage = new UmbNumberState(1);
	pagination = { currentPage: this.#currentPage.asObservable() };

	#totalPrevItems = new UmbNumberState(0);
	#totalNextItems = new UmbNumberState(0);
	targetPagination = {
		totalPrevItems: this.#totalPrevItems.asObservable(),
		totalNextItems: this.#totalNextItems.asObservable(),
	};

	selection = {
		setMultiple: () => {},
		setSelectable: () => {},
		setSelection: () => {},
	};

	selectableFilter = () => true;
	filter = () => true;

	loadTree = () => {};
	setStartNode = () => {};
	setHideTreeRoot = () => {};
	setExpandTreeRoot = () => {};
	setFoldersOnly = () => {};
	setExpansion = () => {};
	setSelectOnly = () => {};
}

describe('UmbDefaultTreeElement view creation', () => {
	let element: UmbDefaultTreeElement;
	let api: UmbFakeTreeContext;
	let releaseSlowView: () => void;
	let slowView: ManifestTreeView;
	let fastView: ManifestTreeView;

	beforeEach(async () => {
		let release!: () => void;
		const gate = new Promise<void>((resolve) => (release = resolve));
		releaseSlowView = release;

		slowView = {
			type: 'treeView',
			alias: 'Umb.Test.Race.TreeView.Slow',
			name: 'Slow Tree View',
			element: async () => {
				await gate;
				return { element: UmbTestRaceSlowTreeViewElement };
			},
			meta: { label: 'Slow', icon: 'icon-list' },
		} as unknown as ManifestTreeView;

		fastView = {
			type: 'treeView',
			alias: 'Umb.Test.Race.TreeView.Fast',
			name: 'Fast Tree View',
			element: UmbTestRaceFastTreeViewElement,
			meta: { label: 'Fast', icon: 'icon-grid' },
		} as unknown as ManifestTreeView;

		api = new UmbFakeTreeContext();
		element = await fixture(html`<umb-default-tree></umb-default-tree>`);
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		element.api = api as any;
		await element.updateComplete;
	});

	const queryRenderedView = (tagName: string) => element.shadowRoot?.querySelector(tagName);

	it('renders the view it was asked for', async () => {
		api.view.setCurrentView(slowView);
		releaseSlowView();
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView('umb-test-race-slow-tree-view')).to.not.be.null;
	});

	it('renders the current view when an earlier view is created after it', async () => {
		api.view.setCurrentView(slowView);
		await aTimeout(0);

		api.view.setCurrentView(fastView);
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView('umb-test-race-fast-tree-view')).to.not.be.null;

		releaseSlowView();
		await aTimeout(0);
		await element.updateComplete;

		expect(queryRenderedView('umb-test-race-fast-tree-view')).to.not.be.null;
		expect(queryRenderedView('umb-test-race-slow-tree-view')).to.be.null;
	});
});
