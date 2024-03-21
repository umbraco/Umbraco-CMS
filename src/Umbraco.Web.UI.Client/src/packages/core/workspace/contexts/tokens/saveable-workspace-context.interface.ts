import type { UmbEntityWorkspaceContextInterface } from './entity-workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbSaveableWorkspaceContextInterface
	extends UmbEntityWorkspaceContextInterface,
		UmbRoutableWorkspaceContext {
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	save(): Promise<void>;
	destroy(): void;
}
