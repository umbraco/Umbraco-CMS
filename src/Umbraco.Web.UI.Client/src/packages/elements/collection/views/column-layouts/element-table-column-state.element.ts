import { UmbElementItemDataResolver } from '../../../item/data-resolver/element-item-data-resolver.js';
import type { UmbElementItemModel } from '../../../types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-element-table-column-state')
export class UmbElementTableColumnStateElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbElementItemDataResolver(this);

	@state()
	private _state = '';

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbElementItemModel) {
		this.#value = value;
		if (value) {
			this.#resolver.setData(value);
		}
	}
	public get value(): UmbElementItemModel {
		return this.#value;
	}
	#value!: UmbElementItemModel;

	constructor() {
		super();
		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
	}

	#getStateTagConfig(): { color: UUIInterfaceColor; label: string } {
		switch (this._state) {
			case DocumentVariantStateModel.PUBLISHED:
				return { color: 'positive', label: this.localize.term('content_published') };
			case DocumentVariantStateModel.PUBLISHED_PENDING_CHANGES:
				return { color: 'warning', label: this.localize.term('content_publishedPendingChanges') };
			case DocumentVariantStateModel.DRAFT:
				return { color: 'default', label: this.localize.term('content_unpublished') };
			case DocumentVariantStateModel.NOT_CREATED:
				return { color: 'danger', label: this.localize.term('content_notCreated') };
			default:
				return { color: 'danger', label: fromCamelCase(this._state) };
		}
	}

	override render() {
		const tagConfig = this.#getStateTagConfig();
		return html`<uui-tag color=${tagConfig.color} look="secondary">${tagConfig.label}</uui-tag>`;
	}
}

export default UmbElementTableColumnStateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-table-column-state': UmbElementTableColumnStateElement;
	}
}
