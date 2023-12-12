import { UmbTreeContextBase } from './tree.context.js';
import { html, nothing, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

import './tree-item-default/tree-item.element.js';
import './tree-item-base/tree-item-base.element.js';

@customElement('umb-tree')
export class UmbTreeElement extends UmbLitElement {
	@property({ type: String, reflect: true })
	get alias() {
		return this.#treeContext.getTreeAlias();
	}
	set alias(newVal) {
		this.#treeContext.setTreeAlias(newVal);
	}

	@property({ type: Boolean, reflect: true })
	get selectable() {
		return this.#treeContext.getSelectable();
	}
	set selectable(newVal) {
		this.#treeContext.setSelectable(newVal);
	}

	@property({ type: Array })
	get selection() {
		return this.#treeContext.getSelection();
	}
	set selection(newVal) {
		if (!Array.isArray(newVal)) return;
		this.#treeContext?.setSelection(newVal);
	}

	@property({ type: Boolean, reflect: true })
	get multiple() {
		return this.#treeContext.getMultiple();
	}
	set multiple(newVal) {
		this.#treeContext.setMultiple(newVal);
	}

	// TODO: what is the best name for this functionality?
	private _hideTreeRoot = false;
	@property({ type: Boolean, attribute: 'hide-tree-root' })
	get hideTreeRoot() {
		return this._hideTreeRoot;
	}
	set hideTreeRoot(newVal: boolean) {
		const oldVal = this._hideTreeRoot;
		this._hideTreeRoot = newVal;
		if (newVal === true) {
			this.#observeRootItems();
		}

		this.requestUpdate('hideTreeRoot', oldVal);
	}

	@property()
	get selectableFilter() {
		return this.#treeContext.selectableFilter;
	}
	set selectableFilter(newVal) {
		this.#treeContext.selectableFilter = newVal;
	}

	@property()
	get filter() {
		return this.#treeContext.filter;
	}
	set filter(newVal) {
		console.log(typeof newVal);
		this.#treeContext.filter = newVal;
	}

	@state()
	private _items: TreeItemPresentationModel[] = [];

	@state()
	private _treeRoot?: TreeItemPresentationModel;

	#treeContext = new UmbTreeContextBase<TreeItemPresentationModel>(this);

	#rootItemsObserver?: UmbObserverController<Array<TreeItemPresentationModel>>;

	constructor() {
		super();
		this.#requestTreeRoot();
	}

	async #requestTreeRoot() {
		if (!this.#treeContext?.requestTreeRoot) throw new Error('Tree does not support root');

		const { data } = await this.#treeContext.requestTreeRoot();
		this._treeRoot = data;
	}

	async #observeRootItems() {
		if (!this.#treeContext?.requestRootItems) throw new Error('Tree does not support root items');
		this.#rootItemsObserver?.destroy();

		const { asObservable } = await this.#treeContext.requestRootItems();

		if (asObservable) {
			this.#rootItemsObserver = this.observe(asObservable(), (rootItems) => {
				const oldValue = this._items;
				if (this.filter) {
					const items: Array<TreeItemPresentationModel> = [];
					rootItems.forEach((rootItem) => {
						const filtered = this.filter!(rootItem);
						if (filtered) items.push(rootItem);
					});
					this._items = items;
				} else {
					this._items = rootItems;
				}
				this.requestUpdate('_items', oldValue);
			});
		}
	}

	render() {
		return html` ${this.#renderTreeRoot()} ${this.#renderRootItems()}`;
	}

	#renderTreeRoot() {
		if (this.hideTreeRoot || this._treeRoot === undefined) return nothing;
		return html` <umb-tree-item-default .item=${this._treeRoot}></umb-tree-item-default> `;
	}

	#renderRootItems() {
		if (this._items?.length === 0) return nothing;
		return html`
			${repeat(
				this._items,
				// TODO: use unique here:
				(item, index) => item.name + '___' + index,
				(item) => html`<umb-tree-item-default .item=${item}></umb-tree-item-default>`,
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree': UmbTreeElement;
	}
}
