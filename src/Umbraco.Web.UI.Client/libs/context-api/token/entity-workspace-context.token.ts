import { UmbEntityWorkspaceContextInterface } from '../../workspace/context/workspace-entity-context.interface';
import { UmbContextToken } from './context-token';
import type { BaseEntity } from '@umbraco-cms/backoffice/models';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface<BaseEntity>>(
	'UmbEntityWorkspaceContext'
);
