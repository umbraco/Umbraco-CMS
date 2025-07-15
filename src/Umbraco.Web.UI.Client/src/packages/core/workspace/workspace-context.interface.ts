import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceContext extends UmbApi, UmbContextMinimal {
	readonly workspaceAlias: string;
	getEntityType(): string;
}
