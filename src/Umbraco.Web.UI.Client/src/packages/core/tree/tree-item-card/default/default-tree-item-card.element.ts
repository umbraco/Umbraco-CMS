import type { UmbTreeItemModel } from '../../types.js';
import type { UmbDefaultTreeItemCardApi } from './default-tree-item-card.api.js';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-tree-item-card')
export class UmbDefaultTreeItemCardElement extends UmbLitElement {
	#api?: UmbDefaultTreeItemCardApi;

	@property({ type: Object, attribute: false })
	public set api(value: UmbDefaultTreeItemCardApi | undefined) {
		this.#api = value;
		if (value) {
			this.observe(value.isSelectable, (v) => (this._isSelectable = v), '_observeIsSelectable');
			this.observe(value.isSelectableContext, (v) => (this._isSelectableContext = v), '_observeIsSelectableContext');
			this.observe(value.selectOnly, (v) => (this._selectOnly = v), '_observeSelectOnly');
			this.observe(value.isSelected, (v) => (this._isSelected = v), '_observeIsSelected');
			this.observe(value.isActive, (v) => (this._isActive = v), '_observeIsActive');
			this.observe(value.path, (v) => (this._path = v), '_observePath');
			this.observe(value.hasActions, (v) => (this._hasActions = v), '_observeHasActions');
		}
	}
	public get api(): UmbDefaultTreeItemCardApi | undefined {
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

	#onDblClick(e: MouseEvent) {
		if (!this.item?.hasChildren) return;
		e.stopPropagation();
		this.#api?.open();
	}

	#onKeyDown(e: KeyboardEvent) {
		if (e.key === 'ArrowRight' && this.item?.hasChildren) {
			e.stopPropagation();
			this.#api?.open();
		}
	}

	override render() {
		if (!this.item) return nothing;
		// When in selection mode, clear href so clicking navigates to select rather than route
		const href = this._isSelectableContext ? undefined : this._path || undefined;
		return html`
			<umb-figure-card
				name=${this.item.name}
				href=${ifDefined(href)}
				?selectable=${this._isSelectable}
				?select-only=${this._selectOnly}
				?selected=${this._isSelected}
				?active=${this._isActive}
				?disabled=${this._isSelectableContext && !this._isSelectable}
				background-color="var(--uui-color-surface)"
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}
				@dblclick=${this.#onDblClick}
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
		return html`<umb-entity-actions-bundle slot="actions" .label=${this.item?.name ?? ''}></umb-entity-actions-bundle>`;
	}
}

export default UmbDefaultTreeItemCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree-item-card': UmbDefaultTreeItemCardElement;
	}
}
