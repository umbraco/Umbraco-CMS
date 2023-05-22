import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-configuration')
export class UmbPropertyEditorUITinyMceConfigurationElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-tiny-mce-configuration</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUITinyMceConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-configuration': UmbPropertyEditorUITinyMceConfigurationElement;
	}
}
