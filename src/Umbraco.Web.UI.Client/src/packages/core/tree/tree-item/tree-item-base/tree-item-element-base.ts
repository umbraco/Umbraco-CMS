import type { UmbTreeItemContext } from '../index.js';
import type { UmbTreeItemModel } from '../../types.js';
import {
	html,
	ifDefined,
	nothing,
	state,
	repeat,
	property,
	css,
	type TemplateResult,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';

export abstract class UmbTreeItemElementBase<
	TreeItemModelType extends UmbTreeItemModel,
	TreeItemContextType extends UmbTreeItemContext<TreeItemModelType> = UmbTreeItemContext<TreeItemModelType>,
> extends UmbLitElement {
	@property({ type: Object, attribute: false })
	set item(newVal: TreeItemModelType) {
		this._item = newVal;
		this._extractFlags(newVal);

		if (this._item) {
			this._label = this.localize.string(this._item?.name ?? '');
			this.#initTreeItem();
		}
	}
	get item(): TreeItemModelType | undefined {
		return this._item;
	}
	protected _item?: TreeItemModelType;

	/**
	 * @internal
	 * Indicates whether the user has no access to this tree item.
	 * This property is reflected as an attribute for styling purposes.
	 *
	 * **Usage Pattern (opt-in):**
	 * Child classes that support access restrictions should observe their context's `noAccess` observable
	 * and update this property. The base class provides the property, styling, and interaction prevention,
	 * but does not subscribe to the observable to avoid forcing all tree item types to implement it.
	 *
	 * **Example (in child class api setter):**
	 * ```typescript
	 * this.observe(this.#api.noAccess, (noAccess) => (this._noAccess = noAccess));
	 * ```
	 *
	 * **Why not in the base interface?**
	 * Adding `noAccess` to `UmbTreeItemContext` would be a breaking change, forcing all tree item
	 * implementations (users, members, data types, etc.) to provide this property even when access
	 * restrictions don't apply to them.
	 */
	@property({ type: Boolean, reflect: true, attribute: 'no-access' })
	protected _noAccess = false;

	/**
	 * @param item - The item from which to extract flags.
	 * @description This method is called whenever the `item` property is set. It extracts the flags from the item and assigns them to the `_flags` state property.
	 * This method is in some cases overridden in subclasses to customize how flags are extracted!
	 */
	protected _extractFlags(item: TreeItemModelType | undefined) {
		this._flags = item?.flags ?? [];
	}

	@state()
	protected _flags?: Array<UmbEntityFlag>;

	@state()
	private _label?: string;

	@state()
	private _isLoadingPrevChildren = false;

	@state()
	private _isLoadingNextChildren = false;

	@property({ type: Object, attribute: false })
	public set api(value: TreeItemContextType | undefined) {
		this.#api = value;

		if (this.#api) {
			this.#api?.setIsMenu(this._isMenu);
			this.observe(this.#api.childItems, (value) => (this._childItems = value));
			this.observe(this.#api.hasChildren, (value) => (this._hasChildren = value));
			this.observe(this.#api.isActive, (value) => (this._isActive = value));
			this.observe(this.#api.isOpen, (value) => (this._isOpen = value));
			this.observe(this.#api.isLoading, (value) => (this._isLoading = value));
			this.observe(this.#api.isSelectableContext, (value) => (this._isSelectableContext = value));
			this.observe(this.#api.isSelectable, (value) => (this._isSelectable = value));
			this.observe(this.#api.isSelected, (value) => (this._isSelected = value));
			this.observe(this.#api.path, (value) => (this._href = value));
			this.observe(this.#api.pagination.currentPage, (value) => (this._currentPage = value));
			this.observe(this.#api.pagination.totalPages, (value) => (this._totalPages = value));
			this.observe(this.#api.isLoadingPrevChildren, (value) => (this._isLoadingPrevChildren = value ?? false));
			this.observe(this.#api.isLoadingNextChildren, (value) => (this._isLoadingNextChildren = value ?? false));

			this.observe(
				this.#api.targetPagination?.totalPrevItems,
				(value) => (this._hasPreviousItems = value !== undefined ? value > 0 : false),
			);
			this.observe(
				this.#api.targetPagination?.totalNextItems,
				(value) => (this._hasNextItems = value !== undefined ? value > 0 : false),
			);

			this.#initTreeItem();
		}
	}
	public get api(): TreeItemContextType | undefined {
		return this.#api;
	}
	#api: TreeItemContextType | undefined;

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
	protected _isSelected = false;

	@state()
	private _hasChildren = false;

	@state()
	protected _forceShowExpand = false;

	@state()
	private _isOpen = false;

	@state()
	private _iconSlotHasChildren = false;

	@state()
	private _totalPages = 1;

	@state()
	private _currentPage = 1;

	@state()
	private _hasPreviousItems = false;

	@state()
	private _hasNextItems = false;

	@state()
	protected _isMenu = false;

	set isMenu(value: boolean) {
		this._isMenu = value;
		this.#api?.setIsMenu(value);
	}
	get isMenu(): boolean {
		return this._isMenu;
	}

	#initTreeItem() {
		if (!this.#api) return;
		if (!this._item) return;
		this.#api.setTreeItem(this._item);
	}

	private _handleSelectedItem(event: Event) {
		event.stopPropagation();
		this.#api?.select();
	}

	private _handleDeselectedItem(event: Event) {
		event.stopPropagation();
		this.#api?.deselect();
	}

	private _onShowChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		// Prevent default cause we will now control the show-children state ourself.
		event.preventDefault();
		this.#api?.showChildren();
	}

	private _onHideChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		// Prevent default cause we will now control the show-children state ourself.
		event.preventDefault();
		this.#api?.hideChildren();
	}

	#onLoadPrev(event: any) {
		event.stopPropagation();
		this.#api?.loadPrevItems?.();
	}

	#onLoadNext(event: any) {
		event.stopPropagation();
		const next = (this._currentPage = this._currentPage + 1);
		this.#api?.pagination.setCurrentPageNumber(next);
	}

	// Note: Currently we want to prevent opening when the item is in a selectable context, but this might change in the future.
	// If we like to be able to open items in selectable context, then we might want to make it as a menu item action, so you have to click ... and chose an action called 'Edit'
	override render() {
		const caretLabelKey = this._isOpen
			? 'visuallyHiddenTexts_collapseChildItems'
			: 'visuallyHiddenTexts_expandChildItems';
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				@hide-children=${this._onHideChildren}
				@selected=${this._handleSelectedItem}
				@deselected=${this._handleDeselectedItem}
				?active=${this._isActive}
				?disabled=${(this._isSelectableContext && !this._isSelectable) || this._noAccess}
				?selectable=${this._isSelectable}
				?selected=${this._isSelected}
				.loading=${this._isLoading}
				.hasChildren=${this._forceShowExpand || this._hasChildren}
				.showChildren=${this._isOpen}
				.caretLabel=${this.localize.term(caretLabelKey) + ' ' + this._label}
				label=${ifDefined(this._label)}
				href=${ifDefined(this._isSelectableContext || this._noAccess ? undefined : this._href)}
				.renderExpandSymbol=${this._renderExpandSymbol}>
				${this.#renderLoadPrevButton()} ${this.renderIconContainer()} ${this.renderLabel()} ${this.#renderActions()}
				${this.#renderChildItems()}
				<slot></slot>
				${this.#renderLoadNextButton()}
			</uui-menu-item>
		`;
	}

	#hasNodes = (e: Event) => {
		return (e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
	};

	renderIconContainer() {
		return html`
			<div id="icon-container" slot="icon">
				<slot
					name="icon"
					@slotchange=${(e: Event) => {
						this._iconSlotHasChildren = this.#hasNodes(e);
					}}></slot>
				${this.#renderSigns()}
			</div>
		`;
	}

	// eslint-disable-next-line @typescript-eslint/naming-convention
	_renderExpandSymbol?: () => HTMLElement | TemplateResult<1> | undefined;

	#renderSigns() {
		return this._item
			? html`<umb-entity-sign-bundle .entityType=${this._item!.entityType} .entityFlags=${this._flags}
					>${!this._iconSlotHasChildren ? this.#renderIcon() : nothing}</umb-entity-sign-bundle
				>`
			: nothing;
	}

	#renderIcon() {
		const iconName = this._getIconName();
		const isFolder = this._item?.isFolder;

		if (iconName) {
			return html`<umb-icon name="${this._getIconToRender(iconName)}"></umb-icon>`;
		}

		if (isFolder) {
			return html`<umb-icon name="icon-folder"></umb-icon>`;
		}

		return html`<umb-icon name="icon-circle-dotted"></umb-icon>`;
	}

	protected _getIconToRender(icon: string) {
		const iconWithoutColor = icon.split(' ')[0];
		return this._isActive || this._isSelected ? iconWithoutColor : icon;
	}

	protected _getIconName(): string | null | undefined {
		return this._item?.icon;
	}

	renderLabel() {
		return html`<slot name="label" slot="label"></slot>`;
	}

	#renderActions() {
		if (this.hideActions) return nothing;
		if (!this.#api || !this._item) return nothing;
		return html`
			<umb-entity-actions-bundle
				slot="actions"
				.entityType=${this.#api.entityType}
				.unique=${this.#api.unique}
				.label=${this._label}>
			</umb-entity-actions-bundle>
		`;
	}

	#renderChildItems() {
		return html`
			${this._childItems
				? repeat(
						this._childItems,
						(item, index) => item.name + '___' + index,
						(item) => html`
							<umb-tree-item
								.entityType=${item.entityType}
								.props=${{ hideActions: this.hideActions, item, isMenu: this.isMenu }}></umb-tree-item>
						`,
					)
				: ''}
		`;
	}

	#renderLoadPrevButton() {
		if (!this._hasPreviousItems) return nothing;
		return html` <umb-tree-load-prev-button
			@click=${this.#onLoadPrev}
			.loading=${this._isLoadingPrevChildren}></umb-tree-load-prev-button>`;
	}

	#renderLoadNextButton() {
		if (!this._hasNextItems) return nothing;
		return html`
			<umb-tree-load-more-button
				@click=${this.#onLoadNext}
				.loading=${this._isLoadingNextChildren}></umb-tree-load-more-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#icon-container {
				position: relative;
				font-size: 15px;
			}

			uui-menu-item {
				--umb-sign-bundle-bg: var(--uui-color-surface);
			}

			uui-menu-item:hover {
				--umb-sign-bundle-bg: var(--uui-color-surface-emphasis);
			}

			uui-menu-item[active] {
				--umb-sign-bundle-bg: var(--uui-color-current);
			}

			uui-menu-item[active]:hover {
				--umb-sign-bundle-bg: var(--uui-color-current-emphasis);
			}

			uui-menu-item[selected] {
				--umb-sign-bundle-bg: var(--uui-color-selected);
			}

			uui-menu-item[selected]:hover {
				--umb-sign-bundle-bg: var(--uui-color-selected-emphasis);
			}

			#label {
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
			}
		`,
	];
}
