import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-element-folder-workspace-editor')
export class UmbElementFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbElementFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-folder-workspace-editor': UmbElementFolderWorkspaceEditorElement;
	}
}
