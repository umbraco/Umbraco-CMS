import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbTreeItemContextBase, UMB_TREE_ITEM_CONTEXT_TOKEN } from './tree-item.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _item?: EntityTreeItemResponseModel;
	@property({ type: Object, attribute: false })
	get item() {
		return this._item;
	}
	set item(newVal) {
		const oldVal = this._item;
		this._item = newVal;
		this.requestUpdate('item', oldVal);
	}

	@property({ type: Boolean, attribute: 'has-children' })
	hasChildren = false;

	@state()
	private _childItems?: EntityTreeItemResponseModel[];

	@state()
	private _href?: string;

	@state()
	private _isLoading = false;

	@state()
	private _isSelectable = false;

	@state()
	private _isSelected = false;

	@state()
	private _hasActions = false;

	#treeItemContext?: UmbTreeItemContextBase;

	constructor() {
		super();

		this.consumeContext(UMB_TREE_ITEM_CONTEXT_TOKEN, (instance) => {
			this.#treeItemContext = instance;
			if (!this.#treeItemContext) return;
			// TODO: investigate if we can make an observe decorator
			this.observe(this.#treeItemContext.isLoading, (value) => (this._isLoading = value));
			this.observe(this.#treeItemContext.isSelectable, (value) => (this._isSelectable = value));
			this.observe(this.#treeItemContext.isSelected, (value) => (this._isSelected = value));
			this.observe(this.#treeItemContext.hasActions, (value) => (this._hasActions = value));
			this.observe(this.#treeItemContext.path, (value) => (this._href = value));
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.addEventListener('selected', this._handleSelectedItem);
		this.addEventListener('unselected', this._handleDeselectedItem);
	}

	private _handleSelectedItem(event: Event) {
		event.stopPropagation();
		this.#treeItemContext?.select();
	}

	private _handleDeselectedItem(event: Event) {
		event.stopPropagation();
		this.#treeItemContext?.deselect();
	}

	// TODO: do we want to catch and emit a backoffice event here?
	private _onShowChildren() {
		if (this._childItems && this._childItems.length > 0) return;
		this.#observeChildren();
	}

	async #observeChildren() {
		if (!this.#treeItemContext?.requestChildren) return;

		const { asObservable } = await this.#treeItemContext.requestChildren();
		if (!asObservable) return;

		this.observe(asObservable(), (childItems) => {
			this._childItems = childItems;
		});
	}

	private _openActions() {
		this.#treeItemContext?.toggleContextMenu();
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				?selectable=${this._isSelectable}
				?selected=${this._isSelected}
				.loading=${this._isLoading}
				.hasChildren=${this.hasChildren}
				label="${ifDefined(this.item?.name)}"
				href="${ifDefined(this._href)}">
				${this.#renderIcon()} ${this.#renderActions()} ${this.#renderChildItems()}
				<slot></slot>
			</uui-menu-item>
		`;
	}

	#renderIcon() {
		return html` ${this.item?.icon ? html` <uui-icon slot="icon" name="${this.item.icon}"></uui-icon> ` : nothing} `;
	}

	#renderActions() {
		return html`
			${this._hasActions
				? html`
						<uui-action-bar slot="actions">
							<uui-button @click=${this._openActions} label="Open actions menu">
								<uui-symbol-more></uui-symbol-more>
							</uui-button>
						</uui-action-bar>
				  `
				: nothing}
		`;
	}

	#renderChildItems() {
		return html`
			${this._childItems
				? repeat(
						this._childItems,
						(item) => item.key,
						(item) => html`<umb-tree-item .item=${item}></umb-tree-item>`
				  )
				: ''}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItem;
	}
}
