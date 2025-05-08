import type { UmbWorkspaceContext } from '../../types.js';
import type { UmbSaveableWorkspaceContext } from './saveable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SAVEABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbSaveableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbSaveableWorkspaceContext => 'requestSave' in context,
);
