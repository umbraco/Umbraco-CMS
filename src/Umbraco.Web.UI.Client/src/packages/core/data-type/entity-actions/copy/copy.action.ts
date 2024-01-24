import { type UmbCopyDataTypeRepository } from '../../repository/copy/data-type-copy.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_DATA_TYPE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';

// TODO: investigate what we need to make a generic copy action
export class UmbCopyDataTypeEntityAction extends UmbEntityActionBase<UmbCopyDataTypeRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		const modalContext = this.#modalManagerContext?.open(UMB_DATA_TYPE_PICKER_MODAL);
		const value = await modalContext.onSubmit();
		if (!value) return;
		await this.repository.copy(this.unique, value.selection[0]);
	}
}
