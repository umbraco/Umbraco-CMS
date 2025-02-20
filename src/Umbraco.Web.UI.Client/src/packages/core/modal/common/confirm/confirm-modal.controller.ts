import { UMB_MODAL_MANAGER_CONTEXT } from '../../context/index.js';
import { UMB_CONFIRM_MODAL, type UmbConfirmModalData } from './confirm-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbConfirmModalArgs extends UmbConfirmModalData {}

export class UmbConfirmModalController extends UmbControllerBase {
	async open(args: UmbConfirmModalArgs): Promise<void> {
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modalContext = modalManagerContext.open(this, UMB_CONFIRM_MODAL, {
			data: args,
		});

		const p = modalContext.onSubmit();
		p.catch(() => {
			this.destroy();
		});
		await p;

		// This is a one time off, so we can destroy our selfs.
		this.destroy();

		return;
	}
}

/**
 *
 * @param host {UmbControllerHost} - The host controller
 * @param args {UmbConfirmModalArgs} - The data to pass to the modal
 * @returns {UmbConfirmModalController} The modal controller instance
 */
export function umbConfirmModal(host: UmbControllerHost, args: UmbConfirmModalArgs) {
	return new UmbConfirmModalController(host).open(args);
}
