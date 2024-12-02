import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbSubmittableWorkspaceContext extends UmbEntityWorkspaceContext, UmbRoutableWorkspaceContext {
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	requestSubmit(): Promise<void>;
	destroy(): void;
}
