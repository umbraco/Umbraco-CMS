import type { UmbWorkspaceActionArgs } from '../../types.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbSubmitWorkspaceActionArgs<MetaArgsType> extends UmbWorkspaceActionArgs<MetaArgsType> {
	workspaceContextToken?: string | UmbContextToken<UmbSubmittableWorkspaceContext, UmbSubmittableWorkspaceContext>;
}
