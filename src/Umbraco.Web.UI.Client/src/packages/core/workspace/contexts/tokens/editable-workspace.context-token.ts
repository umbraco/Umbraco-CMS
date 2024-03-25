import type { UmbEditableWorkspaceContext } from './editable-workspace-context.interface.js';
import type { UmbWorkspaceContext } from './workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_EDITABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbEditableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	// TODO: Make proper discriminator:
	(context): context is UmbEditableWorkspaceContext => 'routes' in context,
);
