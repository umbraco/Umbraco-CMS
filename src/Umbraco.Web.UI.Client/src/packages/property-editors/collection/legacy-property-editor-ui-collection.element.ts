import { UmbPropertyEditorUICollectionElement } from './property-editor-ui-collection.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-legacy-property-editor-ui-collection
 * @deprecated Use "umb-property-editor-ui-collection" instead.
 */
@customElement('umb-legacy-property-editor-ui-collection')
export class UmbLegacyPropertyEditorUICollectionElement extends UmbPropertyEditorUICollectionElement {
	constructor() {
		super();
		console.warn(
			'The element "umb-legacy-property-editor-ui-collection" has been deprecated. Use "umb-property-editor-ui-collection" instead.',
		);
	}
}

export default UmbPropertyEditorUICollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-legacy-property-editor-ui-collection': UmbPropertyEditorUICollectionElement;
	}
}
