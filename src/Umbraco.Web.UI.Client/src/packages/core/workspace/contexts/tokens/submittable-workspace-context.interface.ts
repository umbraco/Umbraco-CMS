import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbSubmittableWorkspaceContext extends UmbEntityWorkspaceContext, UmbRoutableWorkspaceContext {
	isNew: Observable<boolean | undefined>;
	getIsNew(): boolean | undefined;
	requestSubmit(): Promise<void>;
	destroy(): void;
}
/**
 * @deprecated Use `UmbSubmittableWorkspaceContext` instead. This token will be removed in the RC version.
 * Rename your save method to `submit` and return a promise that resolves to true when save is complete. No need to call workspaceComplete() any longer.
 * TODO: Remove in RC
 */
export interface UmbSaveableWorkspaceContextInterface extends UmbSubmittableWorkspaceContext {}
