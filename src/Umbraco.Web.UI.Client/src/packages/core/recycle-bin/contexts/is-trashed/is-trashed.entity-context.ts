import { UMB_IS_TRASHED_ENTITY_CONTEXT } from './is-trashed.entity-context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A entity context for the isTrashed state.
 * @class UmbIsTrashedEntityContext
 * @augments {UmbContextBase}
 * @implements {UmbIsTrashedEntityContext}
 */
export class UmbIsTrashedEntityContext extends UmbContextBase {
	#isTrashed = new UmbBooleanState(false);
	isTrashed = this.#isTrashed.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_IS_TRASHED_ENTITY_CONTEXT);
	}

	/**
	 * Gets the isTrashed state
	 * @returns {*}
	 * @memberof UmbIsTrashedContext
	 */
	getIsTrashed() {
		return this.#isTrashed.getValue();
	}

	/**
	 * Sets the isTrashed state
	 * @param {boolean} isTrashed
	 * @memberof UmbIsTrashedContext
	 */
	setIsTrashed(isTrashed: boolean) {
		this.#isTrashed.setValue(isTrashed);
	}
}
