import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../workspace/document-workspace.context-token.js';
import { UmbDocumentPublishingRepository } from './repository/index.js';
import { UmbDocumentPublishedPendingChangesManager } from './document-published-pending-changes.manager.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from './document-publishing.workspace-context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentPublishingWorkspaceContext extends UmbContextBase<UmbDocumentPublishingWorkspaceContext> {
	public readonly publishedPendingChanges = new UmbDocumentPublishedPendingChangesManager(this);

	#publishingRepository = new UmbDocumentPublishingRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, async (context) => {
			// No need to check pending changes for new documents
			if (context.getIsNew()) {
				return;
			}

			this.observe(context.unique, async (unique) => {
				if (unique) {
					const { data: publishedData } = await this.#publishingRepository.published(unique);
					const currentData = context.getData();

					if (!currentData || !publishedData) {
						return;
					}

					this.publishedPendingChanges.process({ currentData, publishedData });
				}
			});
		});
	}
}

export { UmbDocumentPublishingWorkspaceContext as api };
