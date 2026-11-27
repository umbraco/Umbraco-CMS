import { UMB_GLOBAL_PERMISSION_TYPE } from '../constants.js';
import type { UmbPermissionConfig, UmbPermissionDenyReason, UmbPermission, UmbPermissionResult } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { filter, firstValueFrom, type Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

interface UmbPermissionVerbResult {
	permitted: boolean | undefined;
	denyReasons: Array<UmbPermissionDenyReason>;
}

interface UmbPermissionSource {
	key: string;
	permissions: Array<UmbPermission>;
}

/**
 * Collects permissions from sources and decides whether permission verbs are permitted. Denies by default.
 */
export class UmbPermissionRegistry {
	#sources = new UmbArrayState<UmbPermissionSource>([], (source) => source.key);

	/**
	 * Sets the permissions of a source, replacing everything it set before.
	 * @param {string} key - Identifies the source, named like an alias.
	 * @param {Array<UmbPermission>} permissions - The permissions of the source.
	 */
	setSource(key: string, permissions: Array<UmbPermission>): void {
		const validPermissions = permissions.filter((permission) => {
			if (typeof permission?.type === 'string' && permission.type !== '') return true;
			console.warn(`Permission from source "${key}" is ignored because it has no type.`, permission);
			return false;
		});
		this.#sources.appendOne({ key, permissions: validPermissions });
	}

	/**
	 * Removes the permissions of a source.
	 * @param {string} key - Identifies the source.
	 */
	removeSource(key: string): void {
		this.#sources.removeOne(key);
	}

	/**
	 * The result for the verbs in the config. Updates whenever the permissions change.
	 * @param {UmbControllerHost} host - Where the verbs are checked.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @returns {Observable<UmbPermissionResult>} The result.
	 */
	permissionResult(host: UmbControllerHost, config: UmbPermissionConfig): Observable<UmbPermissionResult> {
		return this.#sources.asObservablePart((sources) => {
			const globalPermissions = sources
				.flatMap((source) => source.permissions)
				.filter((permission) => permission.type === UMB_GLOBAL_PERMISSION_TYPE);
			return this.#combineResults(config, (verb) => this.#globalVerbResult(globalPermissions, verb));
		});
	}

	/**
	 * The first determined result for the verbs in the config.
	 * @param {UmbControllerHost} host - Where the verbs are checked.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @returns {Promise<UmbPermissionResult>} The result.
	 */
	getPermissionResult(host: UmbControllerHost, config: UmbPermissionConfig): Promise<UmbPermissionResult> {
		return firstValueFrom(this.permissionResult(host, config).pipe(filter((result) => result.permitted !== undefined)));
	}

	/**
	 * Combines the results of the configured verbs. A denied verb decides the result before undetermined
	 * ones are known. An empty config is denied.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @param {(verb: string) => UmbPermissionVerbResult} getVerbResult - The result of a single verb.
	 * @returns {UmbPermissionResult} The combined result.
	 */
	#combineResults(
		config: UmbPermissionConfig,
		getVerbResult: (verb: string) => UmbPermissionVerbResult,
	): UmbPermissionResult {
		const allOf = (config.allOf ?? []).map(getVerbResult);
		const oneOf = (config.oneOf ?? []).map(getVerbResult);

		if (allOf.length === 0 && oneOf.length === 0) {
			return { permitted: false, denyReasons: [] };
		}

		const allOfPermitted = this.#allPermitted(allOf.map((result) => result.permitted));
		const oneOfPermitted = oneOf.length > 0 ? this.#anyPermitted(oneOf.map((result) => result.permitted)) : true;
		const permitted = this.#allPermitted([allOfPermitted, oneOfPermitted]);

		if (permitted !== false) {
			return { permitted, denyReasons: [] };
		}

		const denied = [...allOf, ...oneOf].filter((result) => result.permitted === false);
		return { permitted, denyReasons: this.#uniqueDenyReasons(denied.flatMap((result) => result.denyReasons)) };
	}

	/**
	 * Three-valued AND: false wins, then undefined.
	 * @param {Array<boolean | undefined>} values - The values to combine.
	 * @returns {boolean | undefined} The combined value.
	 */
	#allPermitted(values: Array<boolean | undefined>): boolean | undefined {
		if (values.includes(false)) return false;
		if (values.includes(undefined)) return undefined;
		return true;
	}

	/**
	 * Three-valued OR: true wins, then undefined.
	 * @param {Array<boolean | undefined>} values - The values to combine.
	 * @returns {boolean | undefined} The combined value.
	 */
	#anyPermitted(values: Array<boolean | undefined>): boolean | undefined {
		if (values.includes(true)) return true;
		if (values.includes(undefined)) return undefined;
		return false;
	}

	/**
	 * Removes duplicate deny reasons.
	 * @param {Array<UmbPermissionDenyReason>} reasons - The reasons.
	 * @returns {Array<UmbPermissionDenyReason>} The reasons, each once.
	 */
	#uniqueDenyReasons(reasons: Array<UmbPermissionDenyReason>): Array<UmbPermissionDenyReason> {
		const seen = new Set<string>();
		return reasons.filter((reason) => {
			const key = JSON.stringify(reason);
			if (seen.has(key)) return false;
			seen.add(key);
			return true;
		});
	}

	#globalVerbResult(globalPermissions: Array<UmbPermission>, verb: string): UmbPermissionVerbResult {
		const denies = globalPermissions.filter((permission) => permission.deny?.includes(verb));
		if (denies.length > 0) {
			return {
				permitted: false,
				denyReasons: denies.flatMap((permission) => (permission.denyReason ? [permission.denyReason] : [])),
			};
		}
		const allowed = globalPermissions.some((permission) => permission.allow?.includes(verb));
		return { permitted: allowed, denyReasons: [] };
	}
}

export const umbPermissionRegistry = new UmbPermissionRegistry();
