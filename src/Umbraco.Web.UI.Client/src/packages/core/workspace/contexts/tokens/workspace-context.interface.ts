import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceContext extends UmbApi {
	readonly workspaceAlias: string;
	getEntityType(): string;
}

/**
 * @deprecated Use UmbWorkspaceContext instead — Will be removed before RC.
 * Notice if you have complains on missing `getUnique()` then you need to use UmbEntityWorkspaceContext instead.
 * TODO: Delete before RC.
 */
export interface UmbWorkspaceContextInterface extends UmbWorkspaceContext {}
