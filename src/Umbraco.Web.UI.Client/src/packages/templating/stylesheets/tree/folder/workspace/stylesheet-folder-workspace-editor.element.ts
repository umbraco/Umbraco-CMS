import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-stylesheet-folder-workspace-editor')
export class UmbStylesheetFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbStylesheetFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-stylesheet-folder-workspace-editor']: UmbStylesheetFolderWorkspaceEditorElement;
	}
}
