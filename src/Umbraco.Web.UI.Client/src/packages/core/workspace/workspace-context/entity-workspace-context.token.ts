import { UmbEntityWorkspaceContextInterface } from './workspace-entity-context.interface';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface<UmbEntityBase>>(
	'UmbEntityWorkspaceContext'
);
