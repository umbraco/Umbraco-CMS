import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';

export interface UmbEntityDetailWorkspaceContextArgs {
	entityType: string;
	workspaceAlias: string;
	detailRepositoryAlias: string;
}

/**
 * @deprecated Use UmbEntityDetailWorkspaceContextArgs instead
 */
export type UmbEntityWorkspaceContextArgs = UmbEntityDetailWorkspaceContextArgs;

export interface UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> {
	parent: UmbEntityModel;
	preset?: UmbDeepPartialObject<DetailModelType>;
}
