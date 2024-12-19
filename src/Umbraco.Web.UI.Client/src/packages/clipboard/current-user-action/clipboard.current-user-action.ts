import { UMB_CLIPBOARD_ROOT_ENTITY_TYPE } from '../clipboard-root/index.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '@umbraco-cms/backoffice/current-user';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';

export class UmbClipboardCurrentUserAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	constructor(host: UmbControllerHost, args: UmbCurrentUserActionArgs<ArgsMetaType>) {
		super(host, args);
	}

	public async getHref() {
		return undefined;
	}

	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_WORKSPACE_MODAL, {
			data: {
				entityType: UMB_CLIPBOARD_ROOT_ENTITY_TYPE,
				preset: {},
			},
			modal: {
				size: 'small',
			},
		});

		await modal.onSubmit();
	}
}

export { UmbClipboardCurrentUserAction as api };
