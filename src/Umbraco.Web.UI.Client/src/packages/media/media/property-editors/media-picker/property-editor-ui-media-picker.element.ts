import { UmbPropertyEditorUIMediaPickerElementBase } from './property-editor-ui-media-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbPropertyEditorUIMediaPickerElementBase {
	protected override readonly multiple = true;
}

export { UmbPropertyEditorUIMediaPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
