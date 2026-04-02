import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-member-type-folder-workspace-editor')
export class UmbMemberTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbMemberTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-member-type-folder-workspace-editor']: UmbMemberTypeFolderWorkspaceEditorElement;
	}
}
