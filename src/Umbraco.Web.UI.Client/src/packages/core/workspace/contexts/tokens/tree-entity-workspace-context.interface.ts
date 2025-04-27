import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbTreeEntityWorkspaceContext extends UmbEntityWorkspaceContext {
	parentEntityType: Observable<string | undefined>;
	parentUnique: Observable<string | null | undefined>;
	entityType: Observable<string | undefined>;
}
