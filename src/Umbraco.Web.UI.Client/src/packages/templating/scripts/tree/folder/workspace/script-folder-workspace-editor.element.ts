import { UMB_SCRIPT_FOLDER_WORKSPACE_CONTEXT } from './script-folder-workspace.context-token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-script-folder-workspace-editor')
export class UmbScriptFolderWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();

		this.consumeContext(UMB_SCRIPT_FOLDER_WORKSPACE_CONTEXT, (workspaceContext) => {
			workspaceContext?.nameWriteGuard.addRule({
				unique: 'UMB_SERVER_PREVENT_FILE_SYSTEM_FOLDER_RENAME',
				permitted: false,
				message: 'It is not possible to change the name of a script folder.',
			});
		});
	}

	override render() {
		return html`<umb-folder-workspace-editor></umb-folder-workspace-editor>`;
	}
}

export { UmbScriptFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-script-folder-workspace-editor']: UmbScriptFolderWorkspaceEditorElement;
	}
}
