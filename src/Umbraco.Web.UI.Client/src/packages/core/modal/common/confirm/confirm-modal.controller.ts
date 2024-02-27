import { UMB_CONFIRM_MODAL, type UmbConfirmModalData } from '../../token/confirm-modal.token.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '../../context/modal-manager.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbConfirmModalArgs extends UmbConfirmModalData {}

export class UmbConfirmModalController extends UmbBaseController {
	async open(args: UmbConfirmModalArgs): Promise<void> {
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		const modalContext = modalManagerContext.open(UMB_CONFIRM_MODAL, {
			data: args,
		});

		await modalContext.onSubmit().catch(() => {
			this.destroy();
		});

		// This is a one time off, so we can destroy our selfs.
		this.destroy();

		// Map back into UmbVariantId instances:
		return;
	}
}

export function umbConfirmModal(host: UmbControllerHost, args: UmbConfirmModalArgs) {
	return new UmbConfirmModalController(host).open(args);
}
