import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UmbElementItemDataResolver } from '../data-resolver/element-item-data-resolver.js';
import type { UmbElementItemModel } from '../types.js';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UUISelectableEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-element-item-ref')
export class UmbElementItemRefElement extends UmbLitElement {
	#item = new UmbElementItemDataResolver<UmbElementItemModel>(this);

	@property({ type: Object })
	public set item(value: UmbElementItemModel | undefined) {
		this.#item.setData(value);
	}
	public get item(): UmbElementItemModel | undefined {
		return this.#item.getData();
	}

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@property({ type: Boolean, attribute: 'select-only', reflect: true })
	selectOnly = false;

	@property({ type: Boolean, reflect: true })
	selectable = false;

	@property({ type: Boolean, reflect: true })
	selected = false;

	@property({ type: Boolean, reflect: true })
	disabled = false;

	@state()
	private _unique = '';

	@state()
	private _name = '';

	@state()
	private _icon = '';

	@state()
	private _isTrashed = false;

	@state()
	private _isDraft = false;

	@state()
	private _editPath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addUniquePaths(['unique'])
			.onSetup(() => {
				return { data: { entityType: UMB_ELEMENT_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPath = routeBuilder({ entityType: UMB_ELEMENT_ENTITY_TYPE });
			});

		this.#item.observe(this.#item.unique, (unique) => (this._unique = unique ?? ''));
		this.#item.observe(this.#item.name, (name) => (this._name = name ?? ''));
		this.#item.observe(this.#item.icon, (icon) => (this._icon = icon ?? ''));
		this.#item.observe(this.#item.isTrashed, (isTrashed) => (this._isTrashed = isTrashed ?? false));
		this.#item.observe(this.#item.isDraft, (isDraft) => (this._isDraft = isDraft ?? false));
	}

	#getHref() {
		if (!this._unique) return;
		const path = UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this._unique });
		return this._editPath + path;
	}

	#onSelected(event: UUISelectableEvent) {
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this._unique));
	}

	#onDeselected(event: UUISelectableEvent) {
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this._unique));
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this._name}
				href=${ifDefined(this.#getHref())}
				?readonly=${this.readonly}
				?standalone=${this.standalone}
				?select-only=${this.selectOnly}
				?selectable=${this.selectable}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon()}${this.#renderIsDraft()} ${this.#renderIsTrashed()}
			</uui-ref-node>
		`;
	}

	#renderIcon() {
		if (!this._icon) return nothing;
		return html`<umb-icon slot="icon" name=${this._icon}></umb-icon>`;
	}

	#renderIsTrashed() {
		if (!this._isTrashed) return nothing;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	#renderIsDraft() {
		if (!this._isDraft) return nothing;
		return html`<uui-tag size="s" slot="tag" look="secondary" color="default">Draft</uui-tag>`;
	}
}

export { UmbElementItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-item-ref': UmbElementItemRefElement;
	}
}
