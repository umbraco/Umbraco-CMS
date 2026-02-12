import { UmbPropertyEditorUiDateTimePickerElementBase } from '../property-editor-ui-date-time-picker-base.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-date-time-with-time-zone-picker
 */
@customElement('umb-property-editor-ui-date-time-with-time-zone-picker')
export class UmbPropertyEditorUIDateTimeWithTimeZonePickerElement extends UmbPropertyEditorUiDateTimePickerElementBase {
	constructor() {
		super('datetime-local', true);
	}
}

export default UmbPropertyEditorUIDateTimeWithTimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-date-time-with-time-zone-picker': UmbPropertyEditorUIDateTimeWithTimeZonePickerElement;
	}
}
