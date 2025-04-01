import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../workspace/constants.js';

/**
 * Document Block Workspace Context
 * Extension to configure the workspace context for a block in a document.
 * @export
 * @class UmbDocumentBlockWorkspaceContext
 * @extends {UmbControllerBase}
 */
export class UmbDocumentBlockWorkspaceContext extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, async (context) => {
			// TODO: revisit this when getContext supports passContextAliasMatches
			const documentWorkspaceContext = await this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, () => {})
				.passContextAliasMatches()
				.asPromise()
				.catch(() => {
					throw new Error('Could not find document workspace context');
				});

			if (context && documentWorkspaceContext) {
				documentWorkspaceContext.destroy();
				// Start the states for blocks inside documents to allow for property value permissions
				context.content.structure.propertyViewState.fallbackToOff();
				context.content.structure.propertyWriteState.fallbackToOff();
			}
		});
	}
}

export { UmbDocumentBlockWorkspaceContext as api };
