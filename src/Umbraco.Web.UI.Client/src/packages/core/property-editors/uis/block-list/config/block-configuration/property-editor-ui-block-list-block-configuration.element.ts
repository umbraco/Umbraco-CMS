import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-list-block-configuration
 */
@customElement('umb-property-editor-ui-block-list-block-configuration')
export class UmbPropertyEditorUIBlockListBlockConfigurationElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-block-list-block-configuration</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIBlockListBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-block-configuration': UmbPropertyEditorUIBlockListBlockConfigurationElement;
	}
}
