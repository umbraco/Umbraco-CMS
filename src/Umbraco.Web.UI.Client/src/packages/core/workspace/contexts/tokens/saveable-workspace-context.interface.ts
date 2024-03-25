import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbSaveableWorkspaceContext extends UmbEntityWorkspaceContext, UmbRoutableWorkspaceContext {
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	requestSubmit(): Promise<void>;
	destroy(): void;
}
/**
 * @deprecated Use `UmbSaveableWorkspaceContext` instead. This token will be removed in the RC version.
 * TODO: Remove in RC
 */
export interface UmbSaveableWorkspaceContextInterface extends UmbSaveableWorkspaceContext {}
