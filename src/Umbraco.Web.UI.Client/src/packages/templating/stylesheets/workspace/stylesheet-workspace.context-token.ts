import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import type { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';

export const UMB_STYLESHEET_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbStylesheetWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbStylesheetWorkspaceContext => context.getEntityType?.() === UMB_STYLESHEET_ENTITY_TYPE,
);
