import type { UmbSubmittableWorkspaceContext } from '../../../../contexts/index.js';
import type { UmbWorkspaceActionArgs } from '../../types.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export interface UmbSubmitWorkspaceActionArgs<MetaArgsType> extends UmbWorkspaceActionArgs<MetaArgsType> {
	workspaceContextToken?: string | UmbContextToken<UmbSubmittableWorkspaceContext, UmbSubmittableWorkspaceContext>;
}
