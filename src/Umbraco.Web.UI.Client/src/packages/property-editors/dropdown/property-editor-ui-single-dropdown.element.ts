import { UmbPropertyEditorUIDropdownElementBase } from './property-editor-ui-dropdown-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-dropdown
 */
@customElement('umb-property-editor-ui-single-dropdown')
export class UmbPropertyEditorUISingleDropdownElement extends UmbPropertyEditorUIDropdownElementBase {
	protected override readonly multiple = false;
}

export default UmbPropertyEditorUISingleDropdownElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-dropdown': UmbPropertyEditorUISingleDropdownElement;
	}
}
