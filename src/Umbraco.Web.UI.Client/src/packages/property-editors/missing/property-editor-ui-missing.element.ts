import { UmbPropertyEditorUIMissingBaseElement } from './property-editor-ui-missing-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-missing
 */
@customElement('umb-property-editor-ui-missing')
export class UmbPropertyEditorUIMissingElement extends UmbPropertyEditorUIMissingBaseElement {
	constructor() {
		super();
		this._titleKey = 'missingEditor_title';
		this._detailsDescriptionKey = 'missingEditor_detailsDescription';
		this._displayPropertyEditorUi = false;
	}
}

export default UmbPropertyEditorUIMissingElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-missing': UmbPropertyEditorUIMissingElement;
	}
}
