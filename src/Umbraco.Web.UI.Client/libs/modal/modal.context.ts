// TODO: remove this import when the search hack is removed
import '../../src/backoffice/search/modals/search/search-modal.element';

import type { UUIModalDialogElement, UUIModalSidebarSize } from '@umbraco-ui/uui';
import { BehaviorSubject } from 'rxjs';
import { UmbModalHandler, UmbModalHandlerClass } from './modal-handler';
import type { UmbModalToken } from './token/modal-token';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export type UmbModalType = 'dialog' | 'sidebar';

export interface UmbModalConfig {
	key?: string;
	type?: UmbModalType;
	size?: UUIModalSidebarSize;
}

// TODO: we should find a way to easily open a modal without adding custom methods to this context. It would result in a better separation of concerns.
// TODO: move all layouts into their correct "silo" folders. User picker should live with users etc.
export class UmbModalContext {
	host: UmbControllerHostElement;
	// TODO: Investigate if we can get rid of HTML elements in our store, so we can use one of our states.
	#modals = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals = this.#modals.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.host = host;
	}

	// TODO: Remove this when the modal system is more flexible
	public search() {
		const modalHandler = new UmbModalHandlerClass(this.host, 'Umb.Modal.Search') as unknown as UmbModalHandler<
			any,
			any
		>;

		//TODO START: This is a hack to get the search modal layout to look like i want it to.
		//TODO: Remove from here to END when the modal system is more flexible
		const topDistance = '50%';
		const margin = '16px';
		const maxHeight = '600px';
		const maxWidth = '500px';
		const dialog = document.createElement('dialog') as HTMLDialogElement;
		dialog.style.top = `max(${margin}, calc(${topDistance} - ${maxHeight} / 2))`;
		dialog.style.margin = '0 auto';
		dialog.style.transform = `translateY(${-maxHeight})`;
		dialog.style.maxHeight = `min(${maxHeight}, calc(100% - ${margin}px * 2))`;
		dialog.style.width = `min(${maxWidth}, calc(100vw - ${margin}))`;
		dialog.style.boxSizing = 'border-box';
		dialog.style.background = 'none';
		dialog.style.border = 'none';
		dialog.style.padding = '0';
		dialog.style.boxShadow = 'var(--uui-shadow-depth-5)';
		dialog.style.borderRadius = '9px';
		const search = document.createElement('umb-search-modal');
		dialog.appendChild(search);
		requestAnimationFrame(() => {
			dialog.showModal();
		});
		modalHandler.modalElement = dialog as unknown as UUIModalDialogElement;
		//TODO END

		modalHandler.modalElement.addEventListener('close-end', () => this.#onCloseEnd(modalHandler));

		this.#modals.next([...this.#modals.getValue(), modalHandler]);
		return modalHandler;
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
		config?: UmbModalConfig
	) {
		const modalHandler = new UmbModalHandlerClass(this.host, modalAlias, data, config) as unknown as UmbModalHandler<
			ModalData,
			ModalResult
		>;

		modalHandler.modalElement.addEventListener('close-end', () => this.#onCloseEnd(modalHandler));

		this.#modals.next([...this.#modals.getValue(), modalHandler]);
		return modalHandler;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalContext
	 */
	public close(key: string) {
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

export const UMB_MODAL_CONTEXT_TOKEN = new UmbContextToken<UmbModalContext>('UmbModalContext');
