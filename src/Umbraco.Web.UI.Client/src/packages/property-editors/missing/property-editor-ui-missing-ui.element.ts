import { UmbPropertyEditorUIMissingBaseElement } from './property-editor-ui-missing-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-missing-ui
 */
@customElement('umb-property-editor-ui-missing-ui')
export class UmbPropertyEditorUIMissingUiElement extends UmbPropertyEditorUIMissingBaseElement {
	constructor() {
		super();
		this._titleKey = 'missingEditor_missingUiTitle';
		this._detailsDescriptionKey = 'missingEditor_missingUiDetailsDescription';
		this._displayPropertyEditorUi = true;
	}
}

export default UmbPropertyEditorUIMissingUiElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-missing-ui': UmbPropertyEditorUIMissingUiElement;
	}
}
