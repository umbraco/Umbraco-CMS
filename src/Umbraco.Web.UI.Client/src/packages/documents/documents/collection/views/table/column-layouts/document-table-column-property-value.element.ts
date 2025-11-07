import type { UmbDocumentCollectionItemModel, UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import { customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertyValuePresentationDisplayOption, type ManifestPropertyValuePresentation } from '../../../../../../core/property-value-presentation/property-value-presentation.extension.js';
import type { UmbDocumentCollectionContext } from '../../../document-collection.context.js';
import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../../../document-collection.context-token.js';

@customElement('umb-document-table-column-property-value')
export class UmbDocumentTableColumnPropertyValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {

	#collectionContext?: UmbDocumentCollectionContext;

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

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});

		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
		this.#resolver.observe(this.#resolver.createDate, (createDate) => (this._createDate = createDate));
		this.#resolver.observe(this.#resolver.updateDate, (updateDate) => (this._updateDate = updateDate));
	}

	#getPropertyValueByAlias() {
		const args = {
			alias: this.column.alias,
			documentTypeAlias: this.value.item.documentType.alias,
			createDate: this._createDate,
			updateDate: this._updateDate,
			state: this._state,
			culture: this.#resolver.getCulture(),
			creator: this.value.item.creator,
			updater: this.value.item.updater,
			sortOrder: this.value.item.sortOrder,
			values: this.value.item.values,
		};
		return this.#collectionContext?.getPropertyValueByAlias(args);
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
