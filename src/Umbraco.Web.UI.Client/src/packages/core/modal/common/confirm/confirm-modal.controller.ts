import { UMB_MODAL_MANAGER_CONTEXT } from '../../context/index.js';
import { UmbOpenModalController } from '../../index.js';
import { UMB_CONFIRM_MODAL, type UmbConfirmModalData } from './confirm-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/** @deprecated use `UmbConfirmModalData`, will be removed in v.17 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbConfirmModalArgs extends UmbConfirmModalData {}

/**
 *
 * @param host {UmbControllerHost} - The host controller
 * @param args {UmbConfirmModalData} - The data to pass to the modal
 * @returns {UmbOpenModalController} The modal controller instance
 */
export function umbConfirmModal(host: UmbControllerHost, data: UmbConfirmModalData) {
	return new UmbOpenModalController(host).open(UMB_CONFIRM_MODAL, { data });
}
