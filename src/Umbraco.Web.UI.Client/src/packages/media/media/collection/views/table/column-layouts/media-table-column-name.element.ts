import type { UmbEditableMediaCollectionItemModel } from '../../../types.js';
import { resolveVariantName } from '../../../../item/index.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

@customElement('umb-media-table-column-name')
export class UmbMediaTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	#displayCulture?: string | null;
	#fallbackCulture?: string | null;

	@state()
	private _name = '';

	@property({ attribute: false })
	public set value(value: UmbEditableMediaCollectionItemModel) {
		this.#value = value;
		this.#updateName();
	}
	public get value(): UmbEditableMediaCollectionItemModel {
		return this.#value;
	}
	#value!: UmbEditableMediaCollectionItemModel;

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.observe(context?.displayCulture, (displayCulture) => {
				this.#displayCulture = displayCulture;
				this.#updateName();
			});
			this.observe(context?.fallbackCulture, (fallbackCulture) => {
				this.#fallbackCulture = fallbackCulture;
				this.#updateName();
			});
		});
	}

	#updateName() {
		this._name = resolveVariantName(this.#value?.item.variants, this.#displayCulture, this.#fallbackCulture);
	}

	#onClick(event: Event & { target: UUIButtonElement }) {
		event.preventDefault();
		event.stopPropagation();
		window.history.pushState(null, '', event.target.href);
	}

	override render() {
		if (!this.value) return nothing;
		if (!this._name) return nothing;
		return html`
			<uui-button compact href=${this.value.editPath} label=${this._name} @click=${this.#onClick}></uui-button>
		`;
	}

	static override styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbMediaTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-table-column-name': UmbMediaTableColumnNameElement;
	}
}
