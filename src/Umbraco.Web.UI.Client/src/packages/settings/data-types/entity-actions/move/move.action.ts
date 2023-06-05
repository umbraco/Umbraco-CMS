import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_DATA_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

// TODO: investigate what we need to make a generic move action
export class UmbMoveDataTypeEntityAction extends UmbEntityActionBase<UmbDataTypeRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		const modalHandler = this.#modalManagerContext?.open(UMB_DATA_TYPE_PICKER_MODAL);
		const { selection } = await modalHandler.onSubmit();
		await this.repository.move(this.unique, selection[0]);
	}
}
