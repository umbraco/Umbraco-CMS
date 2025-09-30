import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-folder-workspace-editor')
export class UmbDocumentBlueprintFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor> </umb-folder-workspace-editor>`;
	}
}

export { UmbDocumentBlueprintFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-document-blueprint-folder-workspace-editor']: UmbDocumentBlueprintFolderWorkspaceEditorElement;
	}
}
