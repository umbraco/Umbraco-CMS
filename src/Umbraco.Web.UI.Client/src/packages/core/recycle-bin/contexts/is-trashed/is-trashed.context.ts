import { UMB_IS_TRASHED_CONTEXT } from './is-trashed.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Represents the context for the isTrashed state
 * @export
 * @class UmbIsTrashedContext
 * @extends {UmbContextBase<UmbIsTrashedContext>}
 * @implements {UmbIsTrashedContext}
 */
export class UmbIsTrashedContext extends UmbContextBase<UmbIsTrashedContext> {
	#isTrashed = new UmbBooleanState(false);
	isTrashed = this.#isTrashed.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_IS_TRASHED_CONTEXT);
	}

	/**
	 * Gets the isTrashed state
	 * @return {*}
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
