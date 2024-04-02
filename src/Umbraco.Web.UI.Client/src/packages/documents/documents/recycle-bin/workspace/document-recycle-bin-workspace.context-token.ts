import type { UmbDocumentRecycleBinWorkspaceContext } from './document-recycle-bin-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbDocumentRecycleBinWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDocumentRecycleBinWorkspaceContext => context.getEntityType?.() === 'document-recycle-bin',
);
