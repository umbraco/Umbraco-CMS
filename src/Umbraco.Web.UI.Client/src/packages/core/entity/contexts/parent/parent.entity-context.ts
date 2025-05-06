import type { UmbEntityModel } from '../../types.js';
import { UMB_PARENT_ENTITY_CONTEXT } from './parent.entity-context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A entity context for the parent
 * @class UmbParentEntityContext
 * @augments {UmbContextBase}
 * @implements {UmbParentEntityContext}
 */
export class UmbParentEntityContext extends UmbContextBase {
	#parent = new UmbObjectState<UmbEntityModel | undefined>(undefined);
	parent = this.#parent.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PARENT_ENTITY_CONTEXT);
	}

	/**
	 * Gets the parent state
	 * @returns {UmbEntityModel | undefined} - The parent state
	 * @memberof UmbParentEntityContext
	 */
	getParent(): UmbEntityModel | undefined {
		return this.#parent.getValue();
	}

	/**
	 * Sets the parent state
	 * @param {UmbEntityModel | undefined} parent - The parent state
	 * @memberof UmbParentEntityContext
	 */
	setParent(parent: UmbEntityModel | undefined): void {
		this.#parent.setValue(parent);
	}
}
