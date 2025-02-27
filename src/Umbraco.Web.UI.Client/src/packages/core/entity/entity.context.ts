import { UMB_ENTITY_CONTEXT } from './entity.context-token.js';
import type { UmbEntityUnique } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Provides the entity context
 * @class UmbEntityContext
 * @augments {UmbContextBase<UmbEntityContext>}
 */
export class UmbEntityContext extends UmbContextBase<UmbEntityContext> {
	#entityType = new UmbStringState<string | undefined>(undefined);
	public readonly entityType = this.#entityType.asObservable();

	#unique = new UmbStringState<UmbEntityUnique>(null);
	public readonly unique = this.#unique.asObservable();

	/**
	 * Creates an instance of UmbEntityContext.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbEntityContext
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_CONTEXT);
	}

	/**
	 * Set the entity type
	 * @param {string | undefined} entityType
	 * @memberof UmbEntityContext
	 */
	setEntityType(entityType: string | undefined) {
		this.#entityType.setValue(entityType);
	}

	/**
	 * Get the entity type
	 * @returns {string | undefined}
	 * @memberof UmbEntityContext
	 */
	getEntityType(): string | undefined {
		return this.#entityType.getValue();
	}

	/**
	 * Set the unique
	 * @param {string | null} unique
	 * @memberof UmbEntityContext
	 */
	setUnique(unique: string | null) {
		this.#unique.setValue(unique);
	}

	/**
	 * Get the unique
	 * @returns {string | null}
	 * @memberof UmbEntityContext
	 */
	getUnique() {
		return this.#unique.getValue();
	}
}
