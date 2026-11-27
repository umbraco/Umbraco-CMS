import type { UmbPermission, UmbPermissionConfig, UmbPermissionResult } from '../types.js';
import { UmbPermissionCheckController } from './permission-check.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

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
	 * The permissions of one type from every source. To find out whether a verb is permitted, use
	 * `permissionResult()`: it also applies the evaluators.
	 * @param {string} type - The permission type.
	 * @returns {Observable<Array<UmbPermission>>} The permissions.
	 */
	permissionsOfType(type: string): Observable<Array<UmbPermission>> {
		return this.#sources.asObservablePart((sources) =>
			sources.flatMap((source) => source.permissions).filter((permission) => permission.type === type),
		);
	}

	/**
	 * The result for the verbs in the config. Creates a check on the host, which runs the evaluators that list a
	 * requested verb and lives until the host is destroyed. Each call creates its own check.
	 * @param {UmbControllerHost} host - Where the verbs are checked.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @returns {Observable<UmbPermissionResult>} The result. Updates whenever the permissions or the verdicts change.
	 */
	permissionResult(host: UmbControllerHost, config: UmbPermissionConfig): Observable<UmbPermissionResult> {
		return new UmbPermissionCheckController(host, this, config).result;
	}

	/**
	 * The first determined result for the verbs in the config. The check it creates on the host is destroyed once it
	 * has answered.
	 * @param {UmbControllerHost} host - Where the verbs are checked.
	 * @param {UmbPermissionConfig} config - The verbs to check.
	 * @returns {Promise<UmbPermissionResult>} The result.
	 */
	async getPermissionResult(host: UmbControllerHost, config: UmbPermissionConfig): Promise<UmbPermissionResult> {
		const check = new UmbPermissionCheckController(host, this, config);
		const result = await check.getDeterminedResult();
		check.destroy();
		return result;
	}
}

export const umbPermissionRegistry = new UmbPermissionRegistry();
