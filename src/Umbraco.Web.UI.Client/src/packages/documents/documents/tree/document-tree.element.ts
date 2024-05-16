import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDefaultTreeElement } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-document-tree';
@customElement(elementName)
export class UmbDocumentTreeElement extends UmbDefaultTreeElement {}

export { UmbDocumentTreeElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentTreeElement;
	}
}
