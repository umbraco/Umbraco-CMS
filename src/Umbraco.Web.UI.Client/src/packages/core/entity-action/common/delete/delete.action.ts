import { UmbEntityActionBase } from '../../entity-action.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDeleteEntityAction<
	T extends UmbDetailRepository<any> & UmbItemRepository<any>,
> extends UmbEntityActionBase<T> {
	#modalManager?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, args: any) {
		console.log('ARGS', args);
		super(host, repositoryAlias, unique, entityType);

		new UmbContextConsumerController(this._host, UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManager = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalManager) return;

		// TOOD: add back when entity actions can support multiple repositories
		//const { data } = await this.repository.requestItems([this.unique]);

		const modalContext = this.#modalManager.open(UMB_CONFIRM_MODAL, {
			data: {
				headline: `Delete`,
				content: 'Are you sure you want to delete this item?',
				color: 'danger',
				confirmLabel: 'Delete',
			},
		});

		await modalContext.onSubmit();
		await this.repository?.delete(this.unique);
	}
}
