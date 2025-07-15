import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-type-folder-workspace-editor')
export class UmbMediaTypeFolderWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbMediaTypeFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-media-type-folder-workspace-editor']: UmbMediaTypeFolderWorkspaceEditorElement;
	}
}
