import { UmbDefaultTreeElement } from './default-tree.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbTreeItemModel } from '../types.js';

class UmbFakeTreeContext {
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

	setStartNode = () => {};
	setHideTreeRoot = () => {};
	setExpandTreeRoot = () => {};
	setFoldersOnly = () => {};
	setExpansion = () => {};

	setRootItems(items: Array<UmbTreeItemModel>) {
		this.#rootItems.setValue(items);
	}

	setIsLoading(value: boolean) {
		this.#isLoading.setValue(value);
	}
}

describe('UmbDefaultTreeElement', () => {
	let element: UmbDefaultTreeElement;
	let api: UmbFakeTreeContext;

	beforeEach(async () => {
		api = new UmbFakeTreeContext();
		element = await fixture(html`<umb-default-tree .hideTreeRoot=${true}></umb-default-tree>`);
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		element.api = api as any;
		await element.updateComplete;
	});

	const queryEmptyState = () => element.shadowRoot?.querySelector('#empty-state');

	it('does not show the empty state before the initial load has completed', async () => {
		expect(queryEmptyState()).to.be.null;
	});

	it('shows the empty state when there are no items after loading', async () => {
		api.setIsLoading(true);
		await element.updateComplete;
		api.setIsLoading(false);
		await element.updateComplete;

		expect(queryEmptyState()).to.not.be.null;
	});

	it('does not show the empty state when items are present after loading', async () => {
		api.setRootItems([{ unique: '1', entityType: 'test', name: 'Item', hasChildren: false } as UmbTreeItemModel]);
		api.setIsLoading(true);
		await element.updateComplete;
		api.setIsLoading(false);
		await element.updateComplete;

		expect(queryEmptyState()).to.be.null;
	});

	it('does not show the empty state in a menu even when empty after loading', async () => {
		element.isMenu = true;
		api.setIsLoading(true);
		await element.updateComplete;
		api.setIsLoading(false);
		await element.updateComplete;

		expect(queryEmptyState()).to.be.null;
	});
});
