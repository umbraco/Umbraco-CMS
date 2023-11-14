import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tree-picker-start-node
 */
@customElement('umb-property-editor-ui-tree-picker-start-node')
export class UmbPropertyEditorUITreePickerStartNodeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`<div>umb-property-editor-ui-tree-picker-start-node</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITreePickerStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tree-picker-start-node': UmbPropertyEditorUITreePickerStartNodeElement;
	}
}
