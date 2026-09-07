import { UmbOpenModalController } from '../../controller/open-modal.controller.js';
import { UMB_INFO_MODAL, type UmbInfoModalData } from './info-modal.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 * @param {UmbControllerHost} host - The host controller
 * @param {UmbInfoModalData} data - The data to pass to the modal
 * @returns {UmbOpenModalController} The modal controller instance
 */
export function umbInfoModal(host: UmbControllerHost, data: UmbInfoModalData) {
	return new UmbOpenModalController(host).open(UMB_INFO_MODAL, { data });
}
