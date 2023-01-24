import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-property-editor-ui-collection-view-column-configuration
 */
@customElement('umb-property-editor-ui-collection-view-column-configuration')
export class UmbPropertyEditorUICollectionViewColumnConfigurationElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-collection-view-column-configuration</div>`;
	}
}

export default UmbPropertyEditorUICollectionViewColumnConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-column-configuration': UmbPropertyEditorUICollectionViewColumnConfigurationElement;
	}
}
