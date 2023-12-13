import { UmbEntityActionBase } from '../../entity-action.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { type UmbModalManagerContext, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { type UmbFolderRepository, UMB_FOLDER_CREATE_MODAL } from '@umbraco-cms/backoffice/tree';

export class UmbCreateFolderEntityAction<T extends UmbFolderRepository> extends UmbEntityActionBase<T> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this._host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalContext) return;

		const modalContext = this.#modalContext.open(UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: this.repositoryAlias,
				parentUnique: this.unique ?? null,
			},
		});

		await modalContext.onSubmit();
	}
}
