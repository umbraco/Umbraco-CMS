import type { UmbDocumentPublishingWorkspaceContext } from './document-publishing.workspace-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT = new UmbContextToken<UmbDocumentPublishingWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDocumentPublishingWorkspaceContext => context.publishedPendingChanges !== undefined,
);
