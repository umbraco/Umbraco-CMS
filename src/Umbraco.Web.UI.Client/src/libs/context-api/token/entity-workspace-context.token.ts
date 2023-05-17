import { UmbEntityWorkspaceContextInterface } from '../../workspace/context/workspace-entity-context.interface';
import { UmbContextToken } from './context-token';
import type { UmbEntityBase } from 'src/libs/models';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface<UmbEntityBase>>(
	'UmbEntityWorkspaceContext'
);
