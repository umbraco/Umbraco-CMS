import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

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
	preset?: Partial<DetailModelType>;
}
