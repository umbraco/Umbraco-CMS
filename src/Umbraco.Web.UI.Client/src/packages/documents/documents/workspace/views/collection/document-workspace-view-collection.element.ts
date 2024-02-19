import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '../../../../../core/collection/types.js';
import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-workspace-view-collection')
export class UmbDocumentWorkspaceViewCollectionElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _config?: UmbCollectionConfiguration;

	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	constructor() {
		super();
		this.#observeConfig();
	}

	async #observeConfig() {
		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(
				workspaceContext.structure.ownerContentType(),
				async (documentType) => {
					if (!documentType) return;

					// TODO: [LK] Temp hard-coded. Once the API is ready, wire up the data-type ID from the content-type.
					const dataTypeUnique = 'dt-collectionView';

					if (dataTypeUnique) {
						await this.#dataTypeDetailRepository.requestByUnique(dataTypeUnique);
						this.observe(
							await this.#dataTypeDetailRepository.byUnique(dataTypeUnique),
							(dataType) => {
								if (!dataType) return;
								this._config = this.#mapDataTypeConfigToCollectionConfig(
									new UmbPropertyEditorConfigCollection(dataType.values),
								);
							},
							'#observeConfig.dataType',
						);
					}
				},
				'#observeConfig.documentType',
			);
		});
	}

	#mapDataTypeConfigToCollectionConfig(
		config: UmbPropertyEditorConfigCollection | undefined,
	): UmbCollectionConfiguration {
		return {
			allowedEntityBulkActions: config?.getValueByAlias<UmbCollectionBulkActionPermissions>('bulkActionPermissions'),
			orderBy: config?.getValueByAlias('orderBy') ?? 'updateDate',
			orderDirection: config?.getValueByAlias('orderDirection') ?? 'asc',
			pageSize: Number(config?.getValueByAlias('pageSize')) ?? 50,
			useInfiniteEditor: config?.getValueByAlias('useInfiniteEditor') ?? false,
		};
	}

	render() {
		return html`<umb-collection alias="Umb.Collection.Document" .config=${this._config}></umb-collection>`;
	}
}

export default UmbDocumentWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-collection': UmbDocumentWorkspaceViewCollectionElement;
	}
}
