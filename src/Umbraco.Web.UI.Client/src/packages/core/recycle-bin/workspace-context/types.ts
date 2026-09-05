import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

/**
 * The minimal contract a workspace context must satisfy for recycle-bin support to be plugged into it.
 */
export interface UmbTrashableEntityWorkspaceContext extends UmbEntityWorkspaceContext {
	readonly modalContext?: unknown;
	readonly isTrashed: Observable<boolean | undefined>;
	readonly isNew: Observable<boolean | undefined>;
	reload(): Promise<void>;
	readonly readOnlyGuard: UmbReadOnlyVariantGuardManager;
	resetData(): void;
}
