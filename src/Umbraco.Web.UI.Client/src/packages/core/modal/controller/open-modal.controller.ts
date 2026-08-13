import { UMB_MODAL_MANAGER_CONTEXT } from '../context/modal-manager.context.js';
import type { UmbModalToken } from '../token/modal-token.js';
import type { UmbModalContextClassArgs } from '../context/modal.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbOpenModalController extends UmbControllerBase {
	async open<
		ModalData extends { [key: string]: any } = { [key: string]: any },
		ModalValue = unknown,
		ModalAliasTypeAsToken extends UmbModalToken = UmbModalToken<ModalData, ModalValue>,
	>(
		modalAlias: UmbModalToken<ModalData, ModalValue> | string,
		args: UmbModalContextClassArgs<ModalAliasTypeAsToken> = {},
	): Promise<ModalValue> {
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManagerContext) {
			this.destroy();
			throw new Error('Modal manager not found.');
		}

		const modalContext = modalManagerContext.open(this, modalAlias, args);

		return await modalContext.onSubmit();
	}
}

/**
 *
 * @param {UmbControllerHost} host - The host controller
 * @param {UmbModalToken | string} modalAlias - The alias or token of the modal to open
 * @param {UmbModalContextClassArgs} args - The data to pass to the modal
 * @template ModalValue
 * @returns {Promise<ModalValue>} The value resolved from the modal.
 */
export function umbOpenModal<
	ModalData extends { [key: string]: any } = { [key: string]: any },
	ModalValue = unknown,
	ModalAliasTypeAsToken extends UmbModalToken = UmbModalToken<ModalData, ModalValue>,
>(
	host: UmbControllerHost,
	modalAlias: UmbModalToken<ModalData, ModalValue> | string,
	args: UmbModalContextClassArgs<ModalAliasTypeAsToken> = {},
): Promise<ModalValue> {
	return new UmbOpenModalController(host).open(modalAlias, args);
}
