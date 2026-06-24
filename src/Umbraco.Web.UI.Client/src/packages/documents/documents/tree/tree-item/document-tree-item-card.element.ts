import type { UmbDocumentTreeItemModel } from '../types.js';
import { UmbDocumentItemDataResolver } from '../../item/index.js';
import { UmbDocumentVariantState } from '../../variant-state.js';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import type { UmbTreeItemApi } from '@umbraco-cms/backoffice/tree';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

const elementName = 'umb-document-tree-item-card';

@customElement(elementName)
export class UmbDocumentTreeItemCardElement extends UmbLitElement {
	#api?: UmbTreeItemApi;
	#item = new UmbDocumentItemDataResolver(this);

	@property({ type: Object, attribute: false })
	public set api(value: UmbTreeItemApi | undefined) {
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
	public get api(): UmbTreeItemApi | undefined {
		return this.#api;
	}

	@property({ type: Object, attribute: false })
	public set item(value: UmbDocumentTreeItemModel | undefined) {
		this.#item.setData(value);
	}

	@state()
	private _name = '';

	@state()
	private _icon?: string;

	@state()
	private _state?: UmbDocumentVariantState | null;

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

	constructor() {
		super();
		this.observe(this.#item.name, (name) => (this._name = name ?? ''), '_observeName');
		this.observe(this.#item.icon, (icon) => (this._icon = icon), '_observeIcon');
		this.observe(this.#item.state, (state) => (this._state = state), '_observeState');
	}

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
		const href = this._isSelectableContext ? undefined : this._path || undefined;
		return html`
			<umb-figure-card
				name=${this.localize.string(this._name)}
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
				${this.#renderIcon()} ${this.#renderState()} ${this.#renderActions()}
			</umb-figure-card>
		`;
	}

	#renderIcon() {
		const icon = this._icon || getItemFallbackIcon();
		return html`<umb-icon name=${icon}></umb-icon>`;
	}

	#getStateTagConfig(): { color: UUIInterfaceColor; label: string } | undefined {
		if (!this._state) return undefined;
		switch (this._state) {
			case UmbDocumentVariantState.PUBLISHED:
				return { color: 'positive', label: this.localize.term('content_published') };
			case UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES:
				return { color: 'warning', label: this.localize.term('content_publishedPendingChanges') };
			case UmbDocumentVariantState.DRAFT:
				return { color: 'default', label: this.localize.term('content_unpublished') };
			case UmbDocumentVariantState.NOT_CREATED:
				return { color: 'danger', label: this.localize.term('content_notCreated') };
			default:
				return { color: 'danger', label: fromCamelCase(this._state) };
		}
	}

	#renderState() {
		const tagConfig = this.#getStateTagConfig();
		if (!tagConfig) return nothing;
		return html`<uui-tag slot="tag" color=${tagConfig.color} look="secondary">${tagConfig.label}</uui-tag>`;
	}

	#renderActions() {
		if (!this._hasActions) return nothing;
		return html`<umb-entity-actions-bundle
			slot="actions"
			.label=${this.localize.string(this._name)}></umb-entity-actions-bundle>`;
	}
}

export { UmbDocumentTreeItemCardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentTreeItemCardElement;
	}
}
