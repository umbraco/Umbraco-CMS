import { html, customElement, state, ifDefined, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { PropertyEditorConfigProperty } from '@umbraco-cms/backoffice/extension-registry';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_DATA_TYPE_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-config
 * @description - Element for displaying the configuration for a Property Editor based on a Property Editor UI Alias and a Property Editor Model alias.
 * This element requires a UMB_DATA_TYPE_WORKSPACE_CONTEXT to be present.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbLitElement {
	// TODO: Make this element generic, so its not bound to DATA-TYPEs. This will require moving some functionality of Data-Type-Context to this. and this might need to self provide a variant Context for its inner property editor UIs.
	#variantContext?: typeof UMB_DATA_TYPE_VARIANT_CONTEXT.TYPE;

	@state()
	private _properties: Array<PropertyEditorConfigProperty> = [];

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_VARIANT_CONTEXT, (instance) => {
			this.#variantContext = instance;
			this.#observeProperties();
		});
	}

	#observeProperties() {
		if (!this.#variantContext) return;
		this.observe(
			this.#variantContext.properties,
			(properties) => {
				this._properties = properties as Array<PropertyEditorConfigProperty>;
			},
			'observeProperties',
		);
	}

	render() {
		return this._properties?.length > 0
			? repeat(
					this._properties,
					(property) => property.alias,
					(property) =>
						html`<umb-workspace-property
							label="${property.label}"
							description="${ifDefined(property.description)}"
							alias="${property.alias}"
							property-editor-ui-alias="${property.propertyEditorUiAlias}"
							.config=${property.config}></umb-workspace-property>`,
			  )
			: html`<div>No configuration</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-config': UmbPropertyEditorConfigElement;
	}
}
