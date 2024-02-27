import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceContextInterface extends UmbApi {
	readonly workspaceAlias: string;
	getEntityType(): string;
	getUnique(): string | undefined;
}
