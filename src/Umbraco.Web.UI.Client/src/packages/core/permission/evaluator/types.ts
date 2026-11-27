import type { UmbPermission, UmbPermissionDenyReason } from '../types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Interprets the permissions of one permission type where the verbs are checked. Created by the permission registry
 * on the host that checks.
 */
export interface UmbPermissionTypeEvaluatorApi<MetaType = unknown> extends UmbApi {
	/**
	 * Called by the registry with this type's permissions from every source, before `verdicts()`, and again whenever
	 * they change.
	 * @param {Array<UmbPermission<MetaType>>} permissions - The permissions of this type.
	 */
	setPermissions(permissions: Array<UmbPermission<MetaType>>): void;

	/**
	 * Verdicts for the requested verbs, all from `forPermissionVerbs`. `undefined` while the answer isn't known yet.
	 * Leave out verbs you have no opinion on. A permitting verdict grants the verb, so an evaluator that only
	 * restricts never returns `permitted: true`.
	 * @param {Array<string>} verbs - The verbs to give verdicts for.
	 * @returns {Observable<Record<string, UmbPermissionTypeEvaluatorVerdict> | undefined>} The verdicts, by verb.
	 */
	verdicts(verbs: Array<string>): Observable<Record<string, UmbPermissionTypeEvaluatorVerdict> | undefined>;
}

/**
 * An evaluator's opinion on one verb.
 */
export interface UmbPermissionTypeEvaluatorVerdict {
	permitted: boolean;
	/** Why the verb is denied, from the permissions that blocked it. */
	denyReasons?: Array<UmbPermissionDenyReason>;
}
