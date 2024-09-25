import { UmbDocumentTypeTreeRepository } from '../../document-type-tree.repository.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'umb-folder-collection-workspace-view';
@customElement(elementName)
export class UmbFolderCollectionWorkspaceViewEditorElement extends UmbLitElement {
	#treeRepository = new UmbDocumentTypeTreeRepository(this);

	constructor() {
		super();
	}

	override render() {
		return html` <div>Some Collection</div>`;
	}

	static override styles = [css``];
}

export { UmbFolderCollectionWorkspaceViewEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbFolderCollectionWorkspaceViewEditorElement;
	}
}
