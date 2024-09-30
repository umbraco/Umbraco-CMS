import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '../current-user-action.extension.js';
import { UMB_USER_WORKSPACE_PATH } from '@umbraco-cms/backoffice/user';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEditCurrentUserAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	#init;
	#unique?: string;

	constructor(host: UmbControllerHost, args: UmbCurrentUserActionArgs<ArgsMetaType>) {
		super(host, args);

		this.#init = new Promise<void>((res) => {
			this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
				this.observe(
					context.unique,
					(unique) => {
						this.#unique = unique;
						res();
					},
					'umbEditCurrentUserActionObserver',
				);
			});
		});
	}

	async getHref() {
		await this.#init;
		return UMB_USER_WORKSPACE_PATH + '/edit/' + this.#unique;
	}

	async execute() {
		return;
	}
}

export { UmbEditCurrentUserAction as api };
