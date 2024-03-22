import type { UmbPropertyEditorConfigCollection } from '../../config/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_WORKSPACE_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '@umbraco-cms/backoffice/collection';

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

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._config = this.#mapDataTypeConfigToCollectionConfig(config);
	}

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_COLLECTION_CONTEXT, (workspaceContext) => {
			this._collectionAlias = workspaceContext.getCollectionAlias();

			this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
				this.observe(propertyContext.alias, async (propertyAlias) => {
					if (propertyAlias) {
						// Gets the Data Type ID for the current property.
						const property = await workspaceContext.structure.getPropertyStructureByAlias(propertyAlias);
						const unique = workspaceContext.getUnique();
						if (unique && property && this._config) {
							this._config.unique = unique;
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
