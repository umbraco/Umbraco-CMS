import type { UmbEntityModel } from '../../types.js';
import { UMB_ANCESTORS_ENTITY_CONTEXT } from './ancestors.entity-context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A entity context for the ancestors
 * @class UmbAncestorsEntityContext
 * @augments {UmbContextBase}
 * @implements {UmbAncestorsEntityContext}
 */
export class UmbAncestorsEntityContext extends UmbContextBase {
	#ancestors = new UmbArrayState<UmbEntityModel>([], (x) => x.unique);
	ancestors = this.#ancestors.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ANCESTORS_ENTITY_CONTEXT);
	}

	/**
	 * Gets the ancestors state
	 * @returns {Array<UmbEntityModel>} - The ancestors state
	 * @memberof UmbAncestorsEntityContext
	 */
	getAncestors(): Array<UmbEntityModel> {
		return this.#ancestors.getValue();
	}

	/**
	 * Sets the ancestors state
	 * @param {Array<UmbEntityModel>} ancestors - The ancestors state
	 * @memberof UmbAncestorsEntityContext
	 */
	setAncestors(ancestors: Array<UmbEntityModel>) {
		this.#ancestors.setValue(ancestors);
	}
}
