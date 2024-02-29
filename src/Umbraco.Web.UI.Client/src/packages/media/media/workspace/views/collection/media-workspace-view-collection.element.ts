import { customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_MEDIA_COLLECTION_ALIAS, UMB_MEDIA_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/media';
import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '@umbraco-cms/backoffice/collection';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-media-workspace-view-collection')
export class UmbMediaWorkspaceViewCollectionElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _config?: UmbCollectionConfiguration;

	@state()
	private _mediaUnique?: string;

	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	constructor() {
		super();
		this.#observeConfig();
	}

	async #observeConfig() {
		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext.unique, (unique) => {
				this._mediaUnique = unique;
			});
			this.observe(
				workspaceContext.structure.ownerContentType,
				async (mediaType) => {
					if (!mediaType || !mediaType.collection) return;

					const dataTypeUnique = mediaType.collection.unique;

					if (dataTypeUnique) {
						await this.#dataTypeDetailRepository.requestByUnique(dataTypeUnique);
						this.observe(
							await this.#dataTypeDetailRepository.byUnique(dataTypeUnique),
							(dataType) => {
								if (!dataType) return;
								this._config = this.#mapDataTypeConfigToCollectionConfig(dataType);
							},
							'_observeConfigDataType',
						);
					}
				},
				'_observeConfigMediaType',
			);
		});
	}

	#mapDataTypeConfigToCollectionConfig(dataType: UmbDataTypeDetailModel): UmbCollectionConfiguration {
		const config = new UmbPropertyEditorConfigCollection(dataType.values);
		return {
			unique: this._mediaUnique,
			dataTypeId: dataType.unique,
			allowedEntityBulkActions: config?.getValueByAlias<UmbCollectionBulkActionPermissions>('bulkActionPermissions'),
			orderBy: config?.getValueByAlias('orderBy') ?? 'updateDate',
			orderDirection: config?.getValueByAlias('orderDirection') ?? 'asc',
			pageSize: Number(config?.getValueByAlias('pageSize')) ?? 50,
			useInfiniteEditor: config?.getValueByAlias('useInfiniteEditor') ?? false,
			userDefinedProperties: config?.getValueByAlias('includeProperties'),
		};
	}

	render() {
		if (!this._config?.unique || !this._config?.dataTypeId) return html`<uui-loader></uui-loader>`;
		return html`<umb-collection .alias=${UMB_MEDIA_COLLECTION_ALIAS} .config=${this._config}></umb-collection>`;
	}
}

export default UmbMediaWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-collection': UmbMediaWorkspaceViewCollectionElement;
	}
}
