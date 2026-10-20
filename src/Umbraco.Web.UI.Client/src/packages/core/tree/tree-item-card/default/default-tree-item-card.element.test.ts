import { UmbDefaultTreeItemCardElement } from './default-tree-item-card.element.js';
import type { UmbTreeItemCardApi } from '../types.js';
import type { UmbTreeItemModel } from '../../types.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

class UmbTestTreeItemCardApi {
	#isSelectable = new UmbBooleanState(false);
	readonly isSelectable = this.#isSelectable.asObservable();

	#isSelectableContext = new UmbBooleanState(false);
	readonly isSelectableContext = this.#isSelectableContext.asObservable();

	#selectOnly = new UmbBooleanState(false);
	readonly selectOnly = this.#selectOnly.asObservable();

	#isSelected = new UmbBooleanState(false);
	readonly isSelected = this.#isSelected.asObservable();

	#isActive = new UmbBooleanState(false);
	readonly isActive = this.#isActive.asObservable();

	#hasChildren = new UmbBooleanState(false);
	readonly hasChildren = this.#hasChildren.asObservable();

	#noAccess = new UmbBooleanState(false);
	readonly noAccess = this.#noAccess.asObservable();

	#path = new UmbStringState('');
	readonly path = this.#path.asObservable();

	#hasActions = new UmbBooleanState(false);
	readonly hasActions = this.#hasActions.asObservable();

	setSelectableContext(value: boolean) {
		this.#isSelectableContext.setValue(value);
		this.#isSelectable.setValue(value);
	}

	setSelectOnly(value: boolean) {
		this.#selectOnly.setValue(value);
	}

	setHasChildren(value: boolean) {
		this.#hasChildren.setValue(value);
	}

	open() {}
	select() {}
	deselect() {}
}

const item: UmbTreeItemModel = {
	unique: 'a-1',
	entityType: 'type-a',
	name: 'A1',
	hasChildren: false,
	isFolder: false,
	parent: { unique: null, entityType: 'type-a' },
};

describe('UmbDefaultTreeItemCardElement', () => {
	let element: UmbDefaultTreeItemCardElement;
	let api: UmbTestTreeItemCardApi;

	beforeEach(async () => {
		element = await fixture(html`<umb-default-tree-item-card></umb-default-tree-item-card>`);
		api = new UmbTestTreeItemCardApi();
		element.item = item;
		element.api = api as unknown as UmbTreeItemCardApi;
		await element.updateComplete;
	});

	function isSelectOnly(): boolean {
		return element.shadowRoot!.querySelector('umb-figure-card')!.hasAttribute('select-only');
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDefaultTreeItemCardElement);
	});

	it('is not select-only outside a selectable context', async () => {
		await element.updateComplete;
		expect(isSelectOnly()).to.be.false;
	});

	it('is select-only for an item without children in a selectable context', async () => {
		api.setSelectableContext(true);
		await element.updateComplete;

		expect(isSelectOnly()).to.be.true;
	});

	it('is not select-only for an item with children in a selectable context', async () => {
		api.setSelectableContext(true);
		api.setHasChildren(true);
		await element.updateComplete;

		expect(isSelectOnly()).to.be.false;
	});

	it('is not select-only for an item with children while a selection is in progress', async () => {
		api.setSelectableContext(true);
		api.setSelectOnly(true);
		api.setHasChildren(true);
		await element.updateComplete;

		expect(isSelectOnly()).to.be.false;
	});
});
