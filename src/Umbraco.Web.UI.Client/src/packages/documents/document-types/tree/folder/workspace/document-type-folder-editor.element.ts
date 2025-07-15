import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-type-folder-workspace-editor')
export class UmbDocumentTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbDocumentTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-document-type-folder-workspace-editor']: UmbDocumentTypeFolderWorkspaceEditorElement;
	}
}
