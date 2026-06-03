import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import { UmbDocumentItemDataResolver } from '../../../item/document-item-data-resolver.js';
import type { UmbDocumentItemModel } from '../../../item/repository/types.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-document-picker-property-editor-value-summary')
export class UmbDocumentPickerPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbDocumentItemModel>
> {
	@state()
	private _name?: string;

	#resolver = new UmbDocumentItemDataResolver(this);

	constructor() {
		super();
		this.observe(this.#resolver.name, (name) => (this._name = name));
	}

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this)) {
			this.#resolver.setData(this._value?.[0]);
		}
	}

	override render() {
		if (!this._value?.length || !this._name) return nothing;
		return html`<span title="${this._name}">${this._name}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-picker-property-editor-value-summary': UmbDocumentPickerPropertyEditorValueSummaryElement;
	}
}
