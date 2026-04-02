import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';

/**
 * @element umb-property-editor-ui-collection
 */
@customElement('umb-property-editor-ui-collection')
export class UmbPropertyEditorUICollectionElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#workspaceContext?: typeof UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;

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

		this.consumeContext(UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this._collectionAlias = workspaceContext?.collection.getCollectionAlias() ?? UMB_DOCUMENT_COLLECTION_ALIAS;
			this.#gotContexts();
		});

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.#propertyContext = propertyContext;
			this.#gotContexts();
		});
	}

	#gotContexts() {
		if (!this.#workspaceContext || !this.#propertyContext) return;

		this.observe(this.#propertyContext?.alias, async (propertyAlias) => {
			if (this.#workspaceContext && propertyAlias) {
				// Gets the Data Type ID for the current property.
				const property = await this.#workspaceContext.structure.getPropertyStructureByAlias(propertyAlias);
				if (!this.#workspaceContext) {
					// We got destroyed in the meantime.
					return;
				}

				const unique = this.#workspaceContext.getUnique();
				if (unique && property && this._config) {
					// TODO: Handle case where config might not be set when this executes during initialization, its not likely but it is fragile to assume this. [NL]
					this._config.unique = unique;
					this._config.dataTypeId = property.dataType.unique;
					this.requestUpdate('_config');
				}
			}
		});
	}

	#mapDataTypeConfigToCollectionConfig(
		config: UmbPropertyEditorConfigCollection | undefined,
	): UmbCollectionConfiguration {
		const pageSize = Number(config?.getValueByAlias('pageSize'));
		return {
			layouts: config?.getValueByAlias('layouts'),
			orderBy: config?.getValueByAlias('orderBy') ?? 'updateDate',
			orderDirection: config?.getValueByAlias('orderDirection') ?? 'asc',
			pageSize: isNaN(pageSize) ? 50 : pageSize,
			userDefinedProperties: config?.getValueByAlias('includeProperties'),
		};
	}

	override render() {
		if (!this._config?.unique || !this._config?.dataTypeId) return html`<uui-loader></uui-loader>`;
		return html`<umb-collection .alias=${this._collectionAlias} .config=${this._config}></umb-collection>`;
	}

	override destroy(): void {
		super.destroy();
		this.#workspaceContext = undefined;
		this.#propertyContext = undefined;
	}
}

export default UmbPropertyEditorUICollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection': UmbPropertyEditorUICollectionElement;
	}
}
