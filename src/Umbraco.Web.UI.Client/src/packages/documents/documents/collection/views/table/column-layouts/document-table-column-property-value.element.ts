import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import { customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { PropertyValuePresentationDisplayOption, type ManifestPropertyValuePresentation } from '../../../../../../core/property-value-presentation/property-value-presentation.extension.js';

@customElement('umb-document-table-column-property-value')
export class UmbDocumentTableColumnPropertyValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentItemDataResolver(this);

	@state()
	private _createDate?: Date;

	@state()
	private _state?: string;

	@state()
	private _updateDate?: Date;

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

	constructor() {
		super();

		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
		this.#resolver.observe(this.#resolver.createDate, (createDate) => (this._createDate = createDate));
		this.#resolver.observe(this.#resolver.updateDate, (updateDate) => (this._updateDate = updateDate));
	}

	#getPropertyValueByAlias() {
		const alias = this.column.alias;
		const item = this.value.item;
		switch (alias) {
			case 'contentTypeAlias':
				return item.documentType.alias;
			case 'createDate':
				return this._createDate?.toLocaleString();
			case 'creator':
			case 'owner':
				return item.creator;
			case 'published':
				return this._state !== DocumentVariantStateModel.DRAFT ? 'True' : 'False';
			case 'sortOrder':
				return item.sortOrder;
			case 'updateDate':
				return this._updateDate?.toLocaleString();
			case 'updater':
				return item.updater;
			default: {
				const culture = this.#resolver.getCulture();
				const prop = item.values.find((x) => x.alias === alias && (!x.culture || x.culture === culture));

				if (prop) {
					const value = prop.value ?? '';
					const propertyValuePresentationManifest = this.#getPropertyValuePresentationManifest(prop.editorAlias);
					if (propertyValuePresentationManifest.length > 0) {
						return html`<umb-extension-slot
							type="propertyValuePresentation"
							.filter=${(x: ManifestPropertyValuePresentation) => x.propertyEditorAlias === prop.editorAlias}
							.props=${{ alias: alias, value: value, display: PropertyValuePresentationDisplayOption.COLLECTION }}
						>
						</umb-extension-slot>`;
					}

					return value;

				}

				return '';
			}
		}
	}

	#getPropertyValuePresentationManifest(propertyEditorAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValuePresentation',
			(manifest) => manifest.propertyEditorAlias === propertyEditorAlias,
		);
	}

	override render() {
		if (!this.value) return nothing;
		const value = this.#getPropertyValueByAlias();
		return when(
			this.column.labelTemplate,
			() => html`<umb-ufm-render inline .markdown=${this.column.labelTemplate} .value=${{ value }}></umb-ufm-render>`,
			() => html`${value}`,
		);
	}
}

export default UmbDocumentTableColumnPropertyValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-property-value': UmbDocumentTableColumnPropertyValueElement;
	}
}
