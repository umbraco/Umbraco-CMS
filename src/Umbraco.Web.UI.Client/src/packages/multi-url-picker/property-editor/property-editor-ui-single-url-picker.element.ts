import { UmbUrlPickerPropertyEditorUIElementBase } from './property-editor-ui-url-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-url-picker
 */
@customElement('umb-property-editor-ui-single-url-picker')
export class UmbSingleUrlPickerPropertyEditorUIElement extends UmbUrlPickerPropertyEditorUIElementBase {
	protected override readonly multiple = false;
}

export { UmbSingleUrlPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-url-picker': UmbSingleUrlPickerPropertyEditorUIElement;
	}
}
