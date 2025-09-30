import { UMB_DOCUMENT_NOTIFICATIONS_MODAL } from './modal/document-notifications-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentNotificationsEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		await umbOpenModal(this, UMB_DOCUMENT_NOTIFICATIONS_MODAL, {
			data: { unique: this.args.unique },
		});
	}
}
export default UmbDocumentNotificationsEntityAction;
