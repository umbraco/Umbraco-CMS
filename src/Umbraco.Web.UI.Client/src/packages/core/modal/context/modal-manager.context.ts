import type { UmbModalToken } from '../token/modal-token.js';
import { UmbModalContext } from './modal.context.js';
import type { UmbModalContextClassArgs } from './modal.context.js';
import { UmbBasicState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbModalManagerContext extends UmbContextBase<UmbModalManagerContext> {
	// TODO: Investigate if we can get rid of HTML elements in our store, so we can use one of our states.
	#modals = new UmbBasicState(<Array<UmbModalContext>>[]);
	public readonly modals = this.#modals.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MODAL_MANAGER_CONTEXT);

		window.addEventListener('navigationsuccess', this.#onNavigationSuccess);
	}

	/**
	 * Opens a modal or sidebar modal
	 * @public
	 * @param {UmbControllerHost} host - The host that the modal should be attached to, this is usually the controller/element that is opening the modal. This additionally acts as the modal origin for the context api.
	 * @param {(string | UmbModalToken)} modalAlias - The alias or token of the modal to open
	 * @param {UmbModalContextClassArgs} args - The arguments for this setup.
	 * @returns {*}  {UmbModalHandler}
	 * @memberof UmbModalManagerContext
	 */
	public open<
		ModalData extends { [key: string]: any } = { [key: string]: any },
		ModalValue = unknown,
		ModalAliasTypeAsToken extends UmbModalToken = UmbModalToken<ModalData, ModalValue>,
	>(
		host: UmbControllerHost,
		modalAlias: UmbModalToken<ModalData, ModalValue> | string,
		args: UmbModalContextClassArgs<ModalAliasTypeAsToken> = {},
	) {
		const modalContext = new UmbModalContext(host, modalAlias, args);

		// Append to store:
		this.#modals.setValue(
			appendToFrozenArray(this.#modals.value, modalContext, (entry) => entry.key === modalContext.key),
		);

		// Return to implementor:
		return modalContext;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key - The key of the modal to close
	 * @memberof UmbModalManagerContext
	 */
	public close(key: string) {
		const modal = this.#modals.getValue().find((modal) => modal.key === key);
		if (modal) {
			modal.forceResolve();
		}
	}

	public remove(key: string) {
		this.#modals.setValue(this.#modals.getValue().filter((modal) => modal.key !== key));
	}

	/**
	 * Closes all modals that is not routable
	 * @private
	 * @memberof UmbModalManagerContext
	 */
	#closeNoneRoutableModals() {
		this.#modals
			.getValue()
			.filter((modal) => modal.router === null)
			.forEach((modal) => {
				modal.forceResolve();
			});
	}

	#onNavigationSuccess = () => {
		this.#closeNoneRoutableModals();
	};

	override destroy() {
		super.destroy();
		this.#modals.destroy();
		window.removeEventListener('navigationsuccess', this.#onNavigationSuccess);
	}
}

export const UMB_MODAL_MANAGER_CONTEXT = new UmbContextToken<UmbModalManagerContext, UmbModalManagerContext>(
	'UmbModalManagerContext',
);
