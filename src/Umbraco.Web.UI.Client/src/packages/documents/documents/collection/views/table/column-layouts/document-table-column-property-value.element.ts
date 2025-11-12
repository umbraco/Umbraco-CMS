import { UmbDocumentCollectionItemDataResolver } from '../../../document-collection-item-data-resolver.js';
import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import type { ManifestPropertyValuePresentation } from '@umbraco-cms/backoffice/property-value-presentation';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

@customElement('umb-document-table-column-property-value')
export class UmbDocumentTableColumnPropertyValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentCollectionItemDataResolver(this);

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbEditableDocumentCollectionItemModel) {
		this.#value = value;

		if (value.item) {
			this.#resolver.setData(value.item);
		}
	}
	public get value(): UmbEditableDocumentCollectionItemModel {
		return this.#value;
	}
	#value!: UmbEditableDocumentCollectionItemModel;

	override render() {
		if (!this.value?.item) return nothing;
		const prop = this.#resolver.getPropertyByAlias(this.column.alias);
		const props = { value: prop?.value ?? '' };

		if (this.column.labelTemplate) {
			return html`<umb-ufm-render inline .markdown=${this.column.labelTemplate} .value=${props}></umb-ufm-render>`;
		}

		return when(
			prop?.editorAlias,
			(schemaAlias) => html`
				<umb-extension-slot
					type="propertyValuePresentation"
					.filter=${(m: ManifestPropertyValuePresentation) =>
						stringOrStringArrayContains(m.forPropertyEditorSchemaAlias, schemaAlias)}
					.props=${props}>
					${prop?.value ?? nothing}
				</umb-extension-slot>
			`,
			() => (prop?.value ? html`${prop.value}` : nothing),
		);
	}
}

export default UmbDocumentTableColumnPropertyValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-property-value': UmbDocumentTableColumnPropertyValueElement;
	}
}
