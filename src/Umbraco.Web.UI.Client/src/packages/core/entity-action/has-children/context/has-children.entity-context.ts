import { UMB_HAS_CHILDREN_ENTITY_CONTEXT } from './has-children.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbHasChildrenEntityContext extends UmbContextBase {
	#hasChildren = new UmbBooleanState(undefined);
	public readonly hasChildren = this.#hasChildren.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_HAS_CHILDREN_ENTITY_CONTEXT);
	}

	/**
	 * Gets the hasChildren state
	 * @returns {boolean} - The hasChildren state
	 * @memberof UmbHasChildrenEntityContext
	 */
	public getHasChildren(): boolean | undefined {
		return this.#hasChildren.getValue();
	}

	/**
	 * Sets the hasChildren state
	 * @param {boolean} hasChildren - The hasChildren state
	 * @memberof UmbHasChildrenEntityContext
	 */
	public setHasChildren(hasChildren: boolean) {
		this.#hasChildren.setValue(hasChildren);
	}
}
