import { UmbEntityActionBase } from '../../../../entity-action/entity-action.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UmbFolderRepository } from '@umbraco-cms/backoffice/tree';

export class UmbDeleteFolderEntityAction<T extends UmbFolderRepository> extends UmbEntityActionBase<T> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		new UmbContextConsumerController(this._host, UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository || !this.#modalContext) return;

		const { data: folder } = await this.repository.request(this.unique);

		if (folder) {
			// TODO: maybe we can show something about how many items are part of the folder?
			const modalContext = this.#modalContext.open(UMB_CONFIRM_MODAL, {
				data: {
					headline: `Delete folder ${folder.name}`,
					content: 'Are you sure you want to delete this folder?',
					color: 'danger',
					confirmLabel: 'Delete',
				},
			});

			await modalContext.onSubmit();
			await this.repository?.delete(this.unique);
		}
	}
}
