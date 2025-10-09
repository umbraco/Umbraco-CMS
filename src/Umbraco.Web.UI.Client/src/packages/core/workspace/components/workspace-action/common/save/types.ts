import type { UmbSaveableWorkspaceContext, UmbWorkspaceContext } from '../../../../contexts/index.js';
import type { UmbWorkspaceActionArgs } from '../../types.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export interface UmbSaveWorkspaceActionArgs<MetaArgsType, WorkspaceContextType extends UmbSaveableWorkspaceContext>
	extends UmbWorkspaceActionArgs<MetaArgsType> {
	workspaceContextToken?: string | UmbContextToken<UmbWorkspaceContext, WorkspaceContextType>;
}
