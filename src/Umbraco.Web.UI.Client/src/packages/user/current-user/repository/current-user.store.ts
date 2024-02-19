import type { UmbCurrentUserModel } from '../types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbCurrentUserStore extends UmbContextBase<UmbCurrentUserStore> {
	#data = new UmbObjectState<UmbCurrentUserModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_CURRENT_USER_STORE_CONTEXT.toString());
	}

	/**
	 * Get the current user
	 * @readonly
	 * @type {UmbCurrentUserModel}
	 * @memberof UmbCurrentUserStore
	 */
	get() {
		return this.#data.getValue();
	}

	/**
	 * Set the current user
	 * @param {UmbCurrentUserModel} data
	 * @memberof UmbCurrentUserStore
	 */
	set(data: UmbCurrentUserModel) {
		this.#data.setValue(data);
	}

	/**
	 * Update the current user
	 * @param {Partial<UmbCurrentUserModel>} data
	 * @memberof UmbCurrentUserStore
	 */
	update(data: Partial<UmbCurrentUserModel>) {
		this.#data.update(data);
	}

	/**
	 * Clear the current user
	 * @memberof UmbCurrentUserStore
	 */
	clear() {
		this.#data.setValue(undefined);
	}
}

export const UMB_CURRENT_USER_STORE_CONTEXT = new UmbContextToken<UmbCurrentUserStore>('UmbCurrentUserStore');
