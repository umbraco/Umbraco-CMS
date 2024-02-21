import type {
	UmbCollectionBulkActionPermissions,
	UmbCollectionConfiguration,
} from '../../../../core/collection/types.js';
import type { UmbPropertyEditorConfigCollection } from '../../config/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';

/**
 * @element umb-property-editor-ui-collection-view
 */
@customElement('umb-property-editor-ui-collection-view')
export class UmbPropertyEditorUICollectionViewElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string;

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
			// TODO: [LK:2024-02-01] Replace `UMB_DOCUMENT_WORKSPACE_CONTEXT`
			// with an abstracted context that supports both document and media workspaces.
			this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
				this.observe(workspaceContext.unique, (unique) => {
					if (this._config) {
						this._config.unique = unique;
					}
				});
				this.observe(propertyContext.alias, async (propertyAlias) => {
					if (propertyAlias) {
						const property = await workspaceContext.structure.getPropertyStructureByAlias(propertyAlias);
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
		return html`<umb-collection alias="Umb.Collection.Document" .config=${this._config}></umb-collection>`;
	}
}

export default UmbPropertyEditorUICollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view': UmbPropertyEditorUICollectionViewElement;
	}
}
