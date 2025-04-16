import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbDefaultTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree')
export class UmbDocumentTreeElement extends UmbDefaultTreeElement {}

export { UmbDocumentTreeElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree': UmbDocumentTreeElement;
	}
}
