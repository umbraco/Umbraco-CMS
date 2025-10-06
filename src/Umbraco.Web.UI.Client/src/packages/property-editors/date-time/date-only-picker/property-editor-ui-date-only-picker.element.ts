import { UmbPropertyEditorUiDateTimePickerElementBase } from '../property-editor-ui-date-time-picker-base.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-date-only-picker
 */
@customElement('umb-property-editor-ui-date-only-picker')
export class UmbPropertyEditorUIDateOnlyPickerElement extends UmbPropertyEditorUiDateTimePickerElementBase {
	constructor() {
		super('date', false);
	}
}

export default UmbPropertyEditorUIDateOnlyPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-only-picker': UmbPropertyEditorUIDateOnlyPickerElement;
	}
}
