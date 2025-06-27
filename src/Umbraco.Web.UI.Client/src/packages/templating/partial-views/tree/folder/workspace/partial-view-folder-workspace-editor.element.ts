import { UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT } from './partial-view-folder-workspace.context-token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-partial-view-folder-workspace-editor')
export class UmbPartialViewFolderWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();

		this.consumeContext(UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT, (workspaceContext) => {
			workspaceContext?.nameWriteGuard.addRule({
				unique: 'UMB_SERVER_PREVENT_FILE_SYSTEM_FOLDER_RENAME',
				permitted: false,
				message: 'It is not possible to change the name of a Partial View folder.',
			});
		});
	}

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
