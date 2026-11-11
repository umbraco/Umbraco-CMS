import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbGuardRule, UmbReadOnlyGuardManager } from '@umbraco-cms/backoffice/utils';

export interface UmbSubmittableWorkspaceContext extends UmbEntityWorkspaceContext, UmbRoutableWorkspaceContext {
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	requestSubmit(): Promise<void>;
	destroy(): void;
	readonly readOnlyGuard?: UmbReadOnlyGuardManager<UmbGuardRule>;
}
