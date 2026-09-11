import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/context/document-workspace.context-token.js';
import type { ManifestWorkspaceActionMenuItemPreviewOptionKind } from './preview-option.workspace-action-menu-item.extension.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbWorkspaceActionBase, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveAndPreviewOptionWorkspaceAction extends UmbWorkspaceActionBase {
	manifest?: ManifestWorkspaceActionMenuItemPreviewOptionKind;

	// Consumed up front so the document's unique is readable synchronously when the menu item is clicked.
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<never>) {
		super(host, args);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
		});
	}

	override async execute() {
		// Opened before the first await, while still inside the click's synchronous call stack — the only
		// place Safari permits window.open(). See save-and-preview.action.ts for the full rationale (#22626).
		const unique = this.#workspaceContext?.getUnique();
		const previewWindow = window.open('', unique ? `umbpreview-${unique}` : '_blank');

		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			previewWindow?.close();
			throw new Error('The workspace context is missing');
		}
		await workspaceContext.saveAndPreview(this.manifest?.meta.urlProviderAlias, previewWindow);
	}
}

export { UmbDocumentSaveAndPreviewOptionWorkspaceAction as api };
