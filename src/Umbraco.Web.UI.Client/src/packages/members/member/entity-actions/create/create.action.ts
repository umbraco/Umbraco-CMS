import { UMB_MEMBER_CREATE_OPTIONS_MODAL } from './member-create-options-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateMemberEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		await umbOpenModal(this, UMB_MEMBER_CREATE_OPTIONS_MODAL);
	}
}

export { UmbCreateMemberEntityAction as api };
