import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceContext extends UmbApi {
	readonly workspaceAlias: string;
	getEntityType(): string;
}
