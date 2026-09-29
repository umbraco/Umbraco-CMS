import { UmbUrlPickerPropertyEditorUIElementBase } from './property-editor-ui-url-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-url-picker
 */
@customElement('umb-property-editor-ui-single-url-picker')
export class UmbPropertyEditorUISingleUrlPickerElement extends UmbUrlPickerPropertyEditorUIElementBase {
	protected override readonly multiple = false;
}

export default UmbPropertyEditorUISingleUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-url-picker': UmbPropertyEditorUISingleUrlPickerElement;
	}
}
