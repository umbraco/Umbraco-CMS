import { UmbModalHandler, UmbModalHandlerClass } from './modal-handler.js';
import type { UmbModalToken } from './token/modal-token.js';
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
	#modals = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals = this.#modals.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.host = host;
	}

	/**
	 * Opens a modal or sidebar modal
	 * @public
	 * @param {(string | HTMLElement)} element
	 * @param {UmbModalOptions<unknown>} [options]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public open<ModalData extends object = object, ModalResult = unknown>(
		modalAlias: string | UmbModalToken<ModalData, ModalResult>,
		data?: ModalData,
		config?: UmbModalConfig,
		router: IRouterSlot | null = null
	) {
		const modalHandler = new UmbModalHandlerClass(
			this.host,
			router,
			modalAlias,
			data,
			config
		) as unknown as UmbModalHandler<ModalData, ModalResult>;

		modalHandler.modalElement.addEventListener('close-end', () => this.#onCloseEnd(modalHandler));

		this.#modals.next(
			appendToFrozenArray(this.#modals.getValue(), modalHandler, (entry) => entry.key === modalHandler.key)
		);
		return modalHandler;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalContext
	 */
	public close(key: string) {
		console.log('close', key, this.#modals);
		const modal = this.#modals.getValue().find((modal) => modal.key === key);
		if (modal) {
			modal.reject();
		}
	}

	#remove(key: string) {
		this.#modals.next(this.#modals.getValue().filter((modal) => modal.key !== key));
	}

	/**
	 * Handles the close-end event
	 * @private
	 * @param {UmbModalHandler} modalHandler
	 * @memberof UmbModalContext
	 */
	#onCloseEnd(modalHandler: UmbModalHandler<any, any>) {
		modalHandler.modalElement.removeEventListener('close-end', () => this.#onCloseEnd(modalHandler));
		this.#remove(modalHandler.key);
	}
}

export const UMB_MODAL_MANAGER_CONTEXT_TOKEN = new UmbContextToken<UmbModalManagerContext>('UmbModalManagerContext');
