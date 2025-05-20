import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-partial-view-folder-workspace-editor')
export class UmbPartialViewFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbPartialViewFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-partial-view-folder-workspace-editor']: UmbPartialViewFolderWorkspaceEditorElement;
	}
}
