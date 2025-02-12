import type { UmbTreeItemContext } from '../index.js';
import type { UmbTreeItemModel } from '../../types.js';
import { UMB_TREE_ITEM_CONTEXT } from './tree-item-context-base.js';
import { html, nothing, state, ifDefined, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbTreeItemElementBase<
	TreeItemModelType extends UmbTreeItemModel,
	TreeItemContextType extends UmbTreeItemContext<TreeItemModelType> = UmbTreeItemContext<TreeItemModelType>,
> extends UmbLitElement {
	_item?: TreeItemModelType;
	@property({ type: Object, attribute: false })
	get item(): TreeItemModelType | undefined {
		return this._item;
	}
	set item(newVal: TreeItemModelType) {
		this._item = newVal;
		this.#initTreeItem();
	}

	@property({ type: Boolean, attribute: false })
	hideActions: boolean = false;

	@state()
	protected _isActive = false;

	@state()
	private _childItems?: TreeItemModelType[];

	@state()
	private _href?: string;

	@state()
	private _isLoading = false;

	@state()
	private _isSelectableContext = false;

	@state()
	private _isSelectable = false;

	@state()
	private _isSelected = false;

	@state()
	private _hasChildren = false;

	@state()
	private _iconSlotHasChildren = false;

	@state()
	private _totalPages = 1;

	@state()
	private _currentPage = 1;

	#treeItemContext?: TreeItemContextType;

	constructor() {
		super();

		// TODO: Notice this can be retrieve via a api property. [NL]
		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (instance) => {
			this.#treeItemContext = instance as TreeItemContextType;
			if (!this.#treeItemContext) return;

			this.#initTreeItem();

			// TODO: investigate if we can make an observe decorator
			this.observe(this.#treeItemContext.treeItem, (value) => (this._item = value));
			this.observe(this.#treeItemContext.childItems, (value) => (this._childItems = value));
			this.observe(this.#treeItemContext.hasChildren, (value) => (this._hasChildren = value));
			this.observe(this.#treeItemContext.isActive, (value) => (this._isActive = value));
			this.observe(this.#treeItemContext.isLoading, (value) => (this._isLoading = value));
			this.observe(this.#treeItemContext.isSelectableContext, (value) => (this._isSelectableContext = value));
			this.observe(this.#treeItemContext.isSelectable, (value) => (this._isSelectable = value));
			this.observe(this.#treeItemContext.isSelected, (value) => (this._isSelected = value));
			this.observe(this.#treeItemContext.path, (value) => (this._href = value));
			this.observe(this.#treeItemContext.pagination.currentPage, (value) => (this._currentPage = value));
			this.observe(this.#treeItemContext.pagination.totalPages, (value) => (this._totalPages = value));
		});
	}

	#initTreeItem() {
		if (!this.#treeItemContext) return;
		if (!this._item) return;
		this.#treeItemContext.setTreeItem(this._item);
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
		this.#treeItemContext?.loadChildren();
	}

	#onLoadMoreClick = (event: any) => {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this.#treeItemContext?.pagination.setCurrentPageNumber(next);
	};

	// Note: Currently we want to prevent opening when the item is in a selectable context, but this might change in the future.
	// If we like to be able to open items in selectable context, then we might want to make it as a menu item action, so you have to click ... and chose an action called 'Edit'
	override render() {
		const label = this.localize.string(this._item?.name ?? '');
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				@selected=${this._handleSelectedItem}
				@deselected=${this._handleDeselectedItem}
				?active=${this._isActive}
				?disabled=${this._isSelectableContext && !this._isSelectable}
				?selectable=${this._isSelectable}
				?selected=${this._isSelected}
				.loading=${this._isLoading}
				.hasChildren=${this._hasChildren}
				.caretLabel=${this.localize.term('visuallyHiddenTexts_expandChildItems') + ' ' + label}
				label=${label}
				href="${ifDefined(this._isSelectableContext ? undefined : this._href)}">
				${this.renderIconContainer()} ${this.renderLabel()} ${this.#renderActions()} ${this.#renderChildItems()}
				<slot></slot>
				${this.#renderPaging()}
			</uui-menu-item>
		`;
	}

	#hasNodes = (e: Event) => {
		return (e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
	};

	renderIconContainer() {
		return html`
			<slot
				name="icon"
				slot="icon"
				@slotchange=${(e: Event) => {
					this._iconSlotHasChildren = this.#hasNodes(e);
				}}></slot>
			${!this._iconSlotHasChildren ? this.#renderIcon() : nothing}
		`;
	}

	#renderIcon() {
		const icon = this._item?.icon;
		const isFolder = this._item?.isFolder;
		const iconWithoutColor = icon?.split(' ')[0];

		if (icon && iconWithoutColor) {
			return html`<umb-icon slot="icon" name="${this._isActive ? iconWithoutColor : icon}"></umb-icon>`;
		}

		if (isFolder) {
			return html`<umb-icon slot="icon" name="icon-folder"></umb-icon>`;
		}

		return html`<umb-icon slot="icon" name="icon-circle-dotted"></umb-icon>`;
	}

	renderLabel() {
		return html`<slot name="label" slot="label"></slot>`;
	}

	#renderActions() {
		if (this.hideActions) return;
		return this.#treeItemContext && this._item
			? html`<umb-entity-actions-bundle
					slot="actions"
					.entityType=${this.#treeItemContext.entityType}
					.unique=${this.#treeItemContext.unique}
					.label=${this._item.name}>
				</umb-entity-actions-bundle>`
			: '';
	}

	#renderChildItems() {
		return html`
			${this._childItems
				? repeat(
						this._childItems,
						(item, index) => item.name + '___' + index,
						(item) =>
							html`<umb-tree-item
								.entityType=${item.entityType}
								.props=${{ hideActions: this.hideActions, item }}></umb-tree-item>`,
					)
				: ''}
		`;
	}

	#renderPaging() {
		if (this._totalPages <= 1 || this._currentPage === this._totalPages) {
			return nothing;
		}

		return html` <uui-button @click=${this.#onLoadMoreClick} label="Load more"></uui-button> `;
	}
}
