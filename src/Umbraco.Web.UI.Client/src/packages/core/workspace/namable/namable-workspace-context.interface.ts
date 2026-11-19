import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbNameWriteGuardManager } from './name-write-guard.manager.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbGuardRule, UmbReadOnlyGuardManager } from '@umbraco-cms/backoffice/utils';

export interface UmbNamableWorkspaceContext extends UmbWorkspaceContext {
	name: Observable<string | undefined>;
	getName(): string | undefined;
	setName(name: string): void;
	// TODO: implement across all namable workspaces and make it mandatory
	nameWriteGuard?: UmbNameWriteGuardManager;
	readonly readOnlyGuard?: UmbReadOnlyGuardManager<UmbGuardRule>;
}
