import { UMB_CURRENT_USER_MFA_MODAL } from '../modals/current-user-mfa/current-user-mfa-modal.token.js';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '../current-user-action.extension.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbConfigureMfaProvidersApi<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	async getHref() {
		return undefined;
	}

	async execute() {
		await umbOpenModal(this, UMB_CURRENT_USER_MFA_MODAL);
	}
}

export { UmbConfigureMfaProvidersApi as api };
