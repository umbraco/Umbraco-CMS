import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '../../../../core/collection/types.js';
import type { UmbPropertyEditorConfigCollection } from '../../config/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_MEDIA_COLLECTION_ALIAS } from '@umbraco-cms/backoffice/media';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbDocumentWorkspaceContext } from '@umbraco-cms/backoffice/document';
import type { UmbMediaWorkspaceContext } from '@umbraco-cms/backoffice/media';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-collection-view
 */
@customElement('umb-property-editor-ui-collection-view')
export class UmbPropertyEditorUICollectionViewElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string;

	@state()
	private _collectionAlias: string = UMB_DOCUMENT_COLLECTION_ALIAS;

	@state()
	private _config?: UmbCollectionConfiguration;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._config = this.#mapDataTypeConfigToCollectionConfig(config);
	}

	constructor() {
		super();

		// Gets the Data Type ID for the current property.
		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
				// TODO: [LK:2024-02-22] We need a solution that will allow the Collection property-editor
				// to work in any workspace (that supports `unique` and `structure.getPropertyStructureByAlias`).
				const entityType = workspaceContext.getEntityType();
				const contentWorkspaceContext =
					entityType === 'media'
						? (workspaceContext as UmbMediaWorkspaceContext)
						: (workspaceContext as UmbDocumentWorkspaceContext);

				this._collectionAlias = entityType === 'media' ? UMB_MEDIA_COLLECTION_ALIAS : UMB_DOCUMENT_COLLECTION_ALIAS;

				this.observe(contentWorkspaceContext.unique, (unique) => {
					if (this._config) {
						this._config.unique = unique;
					}
				});
				this.observe(propertyContext.alias, async (propertyAlias) => {
					if (propertyAlias) {
						const property = await contentWorkspaceContext.structure.getPropertyStructureByAlias(propertyAlias);
						if (property && this._config) {
							this._config.dataTypeId = property.dataType.unique;
							this.requestUpdate('_config');
						}
					}
				});
			});
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
			userDefinedProperties: config?.getValueByAlias('includeProperties'),
		};
	}

	render() {
		if (!this._config?.unique || !this._config?.dataTypeId) return html`<uui-loader></uui-loader>`;
		return html`<umb-collection .alias=${this._collectionAlias} .config=${this._config}></umb-collection>`;
	}
}

export default UmbPropertyEditorUICollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view': UmbPropertyEditorUICollectionViewElement;
	}
}
