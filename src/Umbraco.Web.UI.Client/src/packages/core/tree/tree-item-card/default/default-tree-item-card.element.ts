import type { UmbTreeItemModel } from '../../types.js';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTreeItemCardApi } from '../types.js';

@customElement('umb-default-tree-item-card')
export class UmbDefaultTreeItemCardElement extends UmbLitElement {
	#api?: UmbTreeItemCardApi;

	@property({ type: Object, attribute: false })
	public set api(value: UmbTreeItemCardApi | undefined) {
		this.#api = value;
		if (value) {
			this.observe(value.isSelectable, (v) => (this._isSelectable = v), '_observeIsSelectable');
			this.observe(value.isSelectableContext, (v) => (this._isSelectableContext = v), '_observeIsSelectableContext');
			this.observe(value.selectOnly, (v) => (this._selectOnly = v), '_observeSelectOnly');
			this.observe(value.isSelected, (v) => (this._isSelected = v), '_observeIsSelected');
			this.observe(value.isActive, (v) => (this._isActive = v), '_observeIsActive');
			this.observe(value.hasChildren, (v) => (this._hasChildren = v), '_observeHasChildren');
			this.observe(value.noAccess, (v) => (this._noAccess = v), '_observeNoAccess');
			this.observe(value.path, (v) => (this._path = v), '_observePath');
			this.observe(value.hasActions, (v) => (this._hasActions = v), '_observeHasActions');
		}
	}
	public get api(): UmbTreeItemCardApi | undefined {
		return this.#api;
	}

	@property({ type: Object, attribute: false })
	item?: UmbTreeItemModel;

	@state()
	private _isSelectable = false;

	@state()
	private _isSelectableContext = false;

	@state()
	private _selectOnly = false;

	@state()
	private _isSelected = false;

	@state()
	private _isActive = false;

	@state()
	private _hasChildren = false;

	@state()
	private _noAccess = false;

	@state()
	private _path = '';

	@state()
	private _hasActions = false;

	#onSelected(e: CustomEvent) {
		e.stopPropagation();
		this.#api?.select();
	}

	#onDeselected(e: CustomEvent) {
		e.stopPropagation();
		this.#api?.deselect();
	}

	#onOpen(e: Event) {
		if (!this._hasChildren) return;
		e.stopPropagation();
		this.#api?.open();
	}

	#onKeyDown(e: KeyboardEvent) {
		if (e.key === 'ArrowRight' && this._hasChildren) {
			e.stopPropagation();
			this.#api?.open();
		}
	}

	override render() {
		if (!this.item) return nothing;
		const href = this._isSelectableContext ? undefined : this._path || undefined;
		return html`
			<umb-figure-card
				name=${this.localize.string(this.item?.name ?? '')}
				href=${ifDefined(href)}
				?selectable=${this._isSelectable}
				?select-only=${this._selectOnly || (!this._hasChildren && this._isSelectableContext)}
				?selected=${this._isSelected}
				?active=${this._isActive}
				?has-children=${this._hasChildren}
				?disabled=${this._noAccess}
				background-color="var(--uui-color-surface)"
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}
				@open=${this.#onOpen}
				@keydown=${this.#onKeyDown}>
				${this.#renderIcon(this.item)} ${this.#renderActions()}
			</umb-figure-card>
		`;
	}

	#renderIcon(item: UmbTreeItemModel) {
		const icon = item.isFolder ? 'icon-folder' : item.icon || getItemFallbackIcon();
		return html`<umb-icon name=${icon}></umb-icon>`;
	}

	#renderActions() {
		if (!this._hasActions) return nothing;
		return html`<umb-entity-actions-bundle slot="actions" .label=${this.localize.string(this.item?.name ?? '')}></umb-entity-actions-bundle>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree-item-card': UmbDefaultTreeItemCardElement;
	}
}
