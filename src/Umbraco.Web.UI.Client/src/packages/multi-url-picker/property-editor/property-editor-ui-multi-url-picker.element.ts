import { UmbPropertyEditorUIMultiUrlPickerElementBase } from './property-editor-ui-multi-url-picker-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement extends UmbPropertyEditorUIMultiUrlPickerElementBase {
	protected override readonly multiple = true;
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
