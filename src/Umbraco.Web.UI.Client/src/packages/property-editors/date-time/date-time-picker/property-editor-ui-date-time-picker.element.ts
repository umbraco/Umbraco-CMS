import { UmbPropertyEditorUiDateTimePickerElementBase } from '../property-editor-ui-date-time-picker-base.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-date-time-picker
 */
@customElement('umb-property-editor-ui-date-time-picker')
export class UmbPropertyEditorUIDateTimePickerElement extends UmbPropertyEditorUiDateTimePickerElementBase {
	constructor() {
		super('datetime-local', false);
	}
}

export default UmbPropertyEditorUIDateTimePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-time-picker': UmbPropertyEditorUIDateTimePickerElement;
	}
}
