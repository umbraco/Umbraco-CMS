import { UmbUserRepository } from '../../repository/user.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

export class UmbSetGroupUserEntityBulkAction extends UmbEntityBulkActionBase<UmbUserRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;

			//TODO: add user group picker modal
		});
	}

	async execute() {
		//TODO: Implement
		alert('Bulk set group is not implemented yet');
	}
}
