import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbNameWriteGuardManager } from './name-write-guard.manager.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbNamableWorkspaceContext extends UmbWorkspaceContext {
	name: Observable<string | undefined>;
	getName(): string | undefined;
	setName(name: string): void;
	nameWriteGuard: UmbNameWriteGuardManager;
}
