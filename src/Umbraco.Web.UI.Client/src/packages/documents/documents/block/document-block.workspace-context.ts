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
			const consumer = await this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, () => {}).passContextAliasMatches();
			const documentWorkspaceContext = consumer.asPromise().catch(() => {
				throw new Error('Could not find document workspace context');
			});

			// TODO: Revist this code, is there a need to hook in here to set fallbackToDisallowed? [NL]
		});
	}
}

export { UmbDocumentBlockWorkspaceContext as api };
