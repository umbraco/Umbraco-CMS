import { UmbPropertyEditorUIMediaPickerElementBase } from './property-editor-ui-media-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-media-picker
 */
@customElement('umb-property-editor-ui-single-media-picker')
export class UmbPropertyEditorUISingleMediaPickerElement extends UmbPropertyEditorUIMediaPickerElementBase {
	protected override readonly multiple = false;
}

export { UmbPropertyEditorUISingleMediaPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-media-picker': UmbPropertyEditorUISingleMediaPickerElement;
	}
}
