import { UmbPropertyEditorUiDateTimePickerElementBase } from '../property-editor-ui-date-time-picker-base.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-time-only-picker
 */
@customElement('umb-property-editor-ui-time-only-picker')
export class UmbPropertyEditorUITimeOnlyPickerElement extends UmbPropertyEditorUiDateTimePickerElementBase {
	constructor() {
		super('time', false);
	}
}

export default UmbPropertyEditorUITimeOnlyPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-time-only-picker': UmbPropertyEditorUITimeOnlyPickerElement;
	}
}
