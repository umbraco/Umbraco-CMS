import type { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaDetailRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		console.log(`execute move for: ${this.selection}`);

		// TODO: the picker should be single picker by default
		const modalContext = this.#modalContext?.open(UMB_MEDIA_TREE_PICKER_MODAL, {
			data: {
				multiple: false,
			},
			value: {
				selection: [],
			},
		});
		if (modalContext) {
			const { selection } = await modalContext.onSubmit();
			const destination = selection[0];
			//await this.repository?.move(this.selection, destination);
		}
	}
}
