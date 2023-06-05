import type { UmbMediaRepository } from '../../repository/media.repository.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_MEDIA_TREE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: the picker should be single picker by default
		const modalContext = this.#modalContext?.open(UMB_MEDIA_TREE_PICKER_MODAL, {
			selection: [],
			multiple: false,
		});
		if (modalContext) {
			const { selection } = await modalContext.onSubmit();
			const destination = selection[0];
			await this.repository?.move(this.selection, destination);
		}
	}
}
