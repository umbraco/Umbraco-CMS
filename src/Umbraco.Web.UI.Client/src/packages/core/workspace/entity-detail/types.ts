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

export type UmbEntityDetailLoadingHookMeta = {
	entityType: string;
	/**
	 * The unique of the entity being loaded, or undefined when a new entity is being created and has no unique yet.
	 */
	unique: string | null | undefined;
	isNew: boolean;
};

export type UmbEntityDetailIncomingDataHookMeta = {
	entityType: string;
	unique: string | null;
};
