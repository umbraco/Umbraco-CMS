import type { UmbModalToken } from './token/modal-token.js';
import { UmbModalContext, UmbModalContextClass } from './index.js';
import type { IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';
import { appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export type UmbModalType = 'dialog' | 'sidebar';

export interface UmbModalConfig {
	key?: string;
	type?: UmbModalType;
	size?: UUIModalSidebarSize;
}

export class UmbModalManagerContext {
	host: UmbControllerHostElement;
	// TODO: Investigate if we can get rid of HTML elements in our store, so we can use one of our states.
	#modals = new BehaviorSubject(<Array<UmbModalContext>>[]);
	public readonly modals = this.#modals.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.host = host;
	}

	/**
	 * Opens a modal or sidebar modal
	 * @public
	 * @param {(string | UmbModalToken)} modalAlias
	 * @param {ModalData} data
	 * @param {UmbModalConfig} config
	 * @param {IRouterSlot | null} router
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalManagerContext
	 */
	public open<ModalData extends object = object, ModalValue = unknown>(
		modalAlias: string | UmbModalToken<ModalData, ModalValue>,
		data?: ModalData,
		config?: UmbModalConfig,
		router: IRouterSlot | null = null,
	) {
		const modalContext = new UmbModalContextClass(
			this.host,
			router,
			modalAlias,
			data,
			config,
		) as unknown as UmbModalContext<ModalData, ModalValue>;

		this.#modals.next(
			appendToFrozenArray(this.#modals.getValue(), modalContext, (entry) => entry.key === modalContext.key),
		);
		return modalContext;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalManagerContext
	 */
	public close(key: string) {
		const modal = this.#modals.getValue().find((modal) => modal.key === key);
		if (modal) {
			modal.reject();
		}
	}

	public remove(key: string) {
		this.#modals.next(this.#modals.getValue().filter((modal) => modal.key !== key));
	}
}

export const UMB_MODAL_MANAGER_CONTEXT_TOKEN = new UmbContextToken<UmbModalManagerContext>('UmbModalManagerContext');
