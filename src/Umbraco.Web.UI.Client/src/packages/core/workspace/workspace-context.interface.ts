import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceContext extends UmbApi {
	readonly workspaceAlias: string;
	getEntityType(): string;
	// TODO: Consider if its more right to make a new interface for UmbEntityWorkspaceContext, cause this on might be intended for the extension type Workspace Context
	// TODO: add entityType observable
}
