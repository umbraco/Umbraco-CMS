import type { UmbEditableMediaCollectionItemModel } from '../../../types.js';
import { customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestPropertyValuePresentation } from '@umbraco-cms/backoffice/property-value-presentation';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-media-table-column-property-value')
export class UmbMediaTableColumnPropertyValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbEditableMediaCollectionItemModel;

	#getPropertyByAlias() {
		const alias = this.column.alias;
		const item = this.value.item;
		const prop = item.values?.find((x) => x.alias === alias);

		return prop;
	}

	override render() {
		if (!this.value?.item) return nothing;
		const prop = this.#getPropertyByAlias();
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

export default UmbMediaTableColumnPropertyValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-table-column-property-value': UmbMediaTableColumnPropertyValueElement;
	}
}
