import { UmbMediaPickerPropertyEditorUIElementBase } from './property-editor-ui-media-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-media-picker
 */
@customElement('umb-property-editor-ui-single-media-picker')
export class UmbSingleMediaPickerPropertyEditorUIElement extends UmbMediaPickerPropertyEditorUIElementBase {
	protected override readonly multiple = false;
}

export { UmbSingleMediaPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-media-picker': UmbSingleMediaPickerPropertyEditorUIElement;
	}
}
