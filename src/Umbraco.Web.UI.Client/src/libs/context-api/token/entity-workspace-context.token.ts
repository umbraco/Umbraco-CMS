import { UmbEntityWorkspaceContextInterface } from '../../../packages/core/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbContextToken } from './context-token';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface<UmbEntityBase>>(
	'UmbEntityWorkspaceContext'
);
