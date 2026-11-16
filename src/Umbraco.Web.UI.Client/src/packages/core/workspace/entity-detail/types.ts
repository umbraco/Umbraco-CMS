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

/**
 * The meta passed to the methods of an entity detail workspace's `initHook`.
 */
export interface UmbEntityDetailInitHookMeta {
	/**
	 * The entity type of the entity being loaded or scaffolded.
	 */
	entityType: string;
	/**
	 * The unique of the entity being loaded, or undefined when a new entity is being created and has no unique yet.
	 */
	unique: string | null | undefined;
	/**
	 * Whether the entity is being scaffolded, as opposed to loaded.
	 */
	isNew: boolean;
}

/**
 * The meta passed to the methods of an entity detail workspace's `incomingDataHook`.
 */
export interface UmbEntityDetailIncomingDataHookMeta {
	/**
	 * The entity type of the incoming entity.
	 */
	entityType: string;
	/**
	 * The unique of the incoming entity.
	 */
	unique: string | null;
}
