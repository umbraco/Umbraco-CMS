import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';

export interface UmbEntityDetailWorkspaceContextArgs {
	entityType: string;
	workspaceAlias: string;
	detailRepositoryAlias: string;
	/**
	 * Optional user-friendly label for this entity type (e.g. "User Group", "Member").
	 * When set, the workspace's title chain includes an additional breadcrumb segment
	 * above the entity name, used by the user history list to disambiguate entities
	 * of different types that share the same section. Localization keys are supported.
	 */
	typeLabel?: string;
	/**
	 * Optional icon to attach to this entity's leaf title segment (e.g. `icon-document-js`).
	 * Surfaced by consumers like the user history list. For workspaces whose icon varies
	 * per-entity (e.g. document type), set the icon directly via `view.setTitle(...)` instead.
	 */
	icon?: string;
}

/**
 * @deprecated Use UmbEntityDetailWorkspaceContextArgs instead
 */
export type UmbEntityWorkspaceContextArgs = UmbEntityDetailWorkspaceContextArgs;

export interface UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> {
	parent: UmbEntityModel;
	preset?: UmbDeepPartialObject<DetailModelType>;
}
