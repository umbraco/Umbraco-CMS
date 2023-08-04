import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

export const UMB_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContextInterface<UmbEntityBase>>(
	'UmbWorkspaceContext'
);
