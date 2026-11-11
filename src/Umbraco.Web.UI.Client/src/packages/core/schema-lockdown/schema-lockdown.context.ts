import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from './schema-lockdown.context-token.js';
import { toSchemaEntityType } from './entity-type-map.js';
import type { UmbSchemaOperation } from './types.js';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ServerSchemaLockdownResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { defer } from '@umbraco-cms/backoffice/external/rxjs';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

// Used while loading and if the request fails. It permits everything, as the server enforces lockdown anyway.
const UMB_SCHEMA_LOCKDOWN_UNKNOWN_STATE: ServerSchemaLockdownResponseModel = Object.freeze({
	restrictedEntityTypes: [],
});

/**
 * Global context that holds which schema operations the server denies for each entity type.
 * The backoffice uses it to hide what the server would deny; the Management API enforces it either way.
 * Until the restrictions have loaded, or when they can't be loaded, every operation is allowed.
 * Entity types are the backoffice ones, translated to the server's; folders follow the entity type they hold.
 */
export class UmbSchemaLockdownContext extends UmbContextBase {
	readonly #state = new UmbObjectState<ServerSchemaLockdownResponseModel>(UMB_SCHEMA_LOCKDOWN_UNKNOWN_STATE);
	#requested = false;

	/**
	 * The schema lockdown restrictions. They're requested from the server once the user is authorized, and again by the
	 * next observer if that request failed.
	 */
	public readonly state = defer(() => {
		this.#request();
		return this.#state.asObservable();
	});

	constructor(host: UmbControllerHost) {
		super(host, UMB_SCHEMA_LOCKDOWN_CONTEXT);

		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(authContext?.isAuthorized, (isAuthorized) => {
				if (!isAuthorized) return;
				this.#request();
			});
		});
	}

	#request() {
		if (this.#requested) return;
		this.#requested = true;
		void this.#load();
	}

	async #load() {
		const { data } = await tryExecute(this._host, ServerService.getServerSchemaLockdown(), {
			disableNotifications: true,
		});
		if (data) {
			this.#state.setValue(data);
		} else {
			// The next observer requests the restrictions again.
			this.#requested = false;
		}
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
		const entry = this.#state.getValue().restrictedEntityTypes.find((x) => x.entityType === schemaEntityType);
		if (!entry) return true;

		return entry[operation];
	}
}

export default UmbSchemaLockdownContext;
