import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from './schema-lockdown.context-token.js';
import { toSchemaEntityType } from './entity-type-map.js';
import type { UmbSchemaOperation } from './types.js';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ServerSchemaLockdownResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

// Used while loading and if the request fails. It permits everything, as the server enforces lockdown anyway.
const UMB_SCHEMA_LOCKDOWN_UNKNOWN_STATE: ServerSchemaLockdownResponseModel = Object.freeze({
	restrictedEntityTypes: [],
});

export class UmbSchemaLockdownContext extends UmbContextBase {
	#state = new UmbObjectState<ServerSchemaLockdownResponseModel>(UMB_SCHEMA_LOCKDOWN_UNKNOWN_STATE);
	public readonly state = this.#state.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_SCHEMA_LOCKDOWN_CONTEXT);
		this.#load();
	}

	async #load() {
		const { data } = await tryExecute(this._host, ServerService.getServerSchemaLockdown(), {
			disableNotifications: true,
		});
		this.#state.setValue(data ?? UMB_SCHEMA_LOCKDOWN_UNKNOWN_STATE);
	}

	/**
	 * Whether the given operation is currently permitted for the given entity type. Entity types the server doesn't
	 * report as restricted are always permitted.
	 * @param {string} entityType The backoffice entity type to check.
	 * @param {UmbSchemaOperation} operation The operation to check.
	 * @returns {boolean} Whether the operation is permitted.
	 */
	public isAllowed(entityType: string, operation: UmbSchemaOperation): boolean {
		const schemaEntityType = toSchemaEntityType(entityType);
		if (!schemaEntityType) return true;

		const entry = this.#state.getValue().restrictedEntityTypes.find((x) => x.entityType === schemaEntityType);
		if (!entry) return true;

		return entry[operation];
	}
}

export default UmbSchemaLockdownContext;
