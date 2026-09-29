import { UmbDropdownPropertyEditorUIElementBase } from './property-editor-ui-dropdown-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-single-dropdown
 */
@customElement('umb-property-editor-ui-single-dropdown')
export class UmbPropertyEditorUISingleDropdownElement extends UmbDropdownPropertyEditorUIElementBase {
	protected override readonly multiple = false;
}

export { UmbPropertyEditorUISingleDropdownElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-single-dropdown': UmbPropertyEditorUISingleDropdownElement;
	}
}
