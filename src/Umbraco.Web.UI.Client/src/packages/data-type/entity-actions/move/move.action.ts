import type { UmbMoveDataTypeRepository } from '../../repository/move/data-type-move.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbModalManagerContext} from '@umbraco-cms/backoffice/modal';
import {
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_DATA_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';

// TODO: investigate what we need to make a generic move action
export class UmbMoveDataTypeEntityAction extends UmbEntityActionBase<UmbMoveDataTypeRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		const modalContext = this.#modalManagerContext?.open(UMB_DATA_TYPE_PICKER_MODAL);
		const { selection } = await modalContext.onSubmit();
		await this.repository.move(this.unique, selection[0]);
	}
}
