import { UmbOpenModalController } from '../../controller/open-modal.controller.js';
import { UMB_CONFIRM_MODAL, type UmbConfirmModalData } from './confirm-modal.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 * @param {UmbControllerHost} host - The host controller
 * @param {UmbConfirmModalData} data - The data to pass to the modal
 * @returns {UmbOpenModalController} The modal controller instance
 */
export function umbConfirmModal(host: UmbControllerHost, data: UmbConfirmModalData) {
	return new UmbOpenModalController(host).open(UMB_CONFIRM_MODAL, { data });
}
