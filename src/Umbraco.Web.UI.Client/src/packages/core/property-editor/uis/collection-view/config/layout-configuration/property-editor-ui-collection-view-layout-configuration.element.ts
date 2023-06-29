import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-collection-view-layout-configuration
 */
@customElement('umb-property-editor-ui-collection-view-layout-configuration')
export class UmbPropertyEditorUICollectionViewLayoutConfigurationElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-collection-view-layout-configuration</div>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUICollectionViewLayoutConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-layout-configuration': UmbPropertyEditorUICollectionViewLayoutConfigurationElement;
	}
}
