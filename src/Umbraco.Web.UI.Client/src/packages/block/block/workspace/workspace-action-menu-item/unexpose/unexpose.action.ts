import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { UmbWorkspaceActionMenuItemBase } from '@umbraco-cms/backoffice/workspace';

/**
 * Workspace action menu item that stores the edits and sets the block as not exposed for the current variant.
 */
export class UmbUnexposeBlockWorkspaceActionMenuItem extends UmbWorkspaceActionMenuItemBase {
	override async execute() {
		const workspaceContext = await this.getContext(UMB_BLOCK_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('Block workspace context not found');
		}
		await workspaceContext.submitAsDraft();
	}
}

export { UmbUnexposeBlockWorkspaceActionMenuItem as api };
