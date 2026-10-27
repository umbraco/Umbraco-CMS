import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbGuardRule, UmbReadOnlyGuardManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityWorkspaceContext, UmbNameWriteGuardManager } from '@umbraco-cms/backoffice/workspace';

/**
 * The minimal contract a workspace context must satisfy for recycle-bin support to be plugged into it.
 */
export interface UmbTrashableEntityWorkspaceContext extends UmbEntityWorkspaceContext {
	readonly modalContext?: unknown;
	readonly isTrashed: Observable<boolean | undefined>;
	readonly isNew: Observable<boolean | undefined>;
	readonly navigationParentItemPath: Observable<string | undefined>;
	reload(): Promise<void>;

	/**
	 * The guard that locks editing while the entity is trashed. When absent, the recycle-bin controller skips it.
	 */
	readonly readOnlyGuard?: UmbReadOnlyGuardManager<UmbGuardRule>;

	/**
	 * The guard that blocks renaming while the entity is trashed. When absent, the recycle-bin controller skips it.
	 */
	readonly nameWriteGuard?: UmbNameWriteGuardManager;

	resetData(): void;
}
