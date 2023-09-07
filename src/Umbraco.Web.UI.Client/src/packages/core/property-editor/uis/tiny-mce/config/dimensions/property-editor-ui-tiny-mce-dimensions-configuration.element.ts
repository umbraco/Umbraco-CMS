import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-dimensions-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-dimensions-configuration')
export class UmbPropertyEditorUITinyMceDimensionsConfigurationElement extends UmbLitElement {
	@property({ type: Object })
	value: { width?: number; height?: number } = {};

	render() {
		return html`<uui-input type="number" placeholder="Width" .value=${this.value.width}></uui-input> x
			<uui-input type="number" placeholder="Height" .value=${this.value.height}></uui-input> pixels`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITinyMceDimensionsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-dimensions-configuration': UmbPropertyEditorUITinyMceDimensionsConfigurationElement;
	}
}
