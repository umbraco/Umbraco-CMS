import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbWorkspaceContext extends UmbApi {
	readonly workspaceAlias: string;
	getEntityType(): string;
	// TODO: add entityType observable
}
