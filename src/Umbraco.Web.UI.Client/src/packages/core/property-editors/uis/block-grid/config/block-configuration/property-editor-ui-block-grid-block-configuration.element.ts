import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-grid-block-configuration
 */
@customElement('umb-property-editor-ui-block-grid-block-configuration')
export class UmbPropertyEditorUIBlockGridBlockConfigurationElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-block-grid-block-configuration</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIBlockGridBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-block-configuration': UmbPropertyEditorUIBlockGridBlockConfigurationElement;
	}
}
