import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '../../workspace/data-type-workspace.context-token.js';
import { html, customElement, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyEditorSettingsProperty } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-config
 * @description - Element for displaying the configuration for a Property Editor based on a Property Editor UI Alias and a Property Editor Model alias.
 * This element requires a UMB_DATA_TYPE_WORKSPACE_CONTEXT to be present.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbLitElement {
	// TODO: Make this element generic, so its not bound to DATA-TYPEs. This will require moving some functionality of Data-Type-Context to this. and this might need to self provide a variant Context for its inner property editor UIs. [NL]
	#workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _properties: Array<PropertyEditorSettingsProperty> = [];

	constructor() {
		super();

		// This now connects to the workspace, as the variant does not know about the layout details.
		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeProperties();
		});
	}

	#observeProperties() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.properties,
			(properties) => {
				this._properties = properties as Array<PropertyEditorSettingsProperty>;
			},
			'observeProperties',
		);
	}

	override render() {
		return this._properties?.length > 0
			? repeat(
					this._properties,
					(property) => property.alias,
					(property) =>
						html`<umb-property
							data-path="$.values[${UmbDataPathPropertyValueQuery(property)}].value"
							label=${property.label}
							description=${ifDefined(property.description)}
							alias=${property.alias}
							property-editor-ui-alias=${property.propertyEditorUiAlias}
							.config=${property.config}></umb-property>`,
				)
			: html`<umb-localize key="editdatatype_noConfiguration"
					>There is no configuration for this property editor.</umb-localize
				>`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbPropertyEditorConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-config': UmbPropertyEditorConfigElement;
	}
}
