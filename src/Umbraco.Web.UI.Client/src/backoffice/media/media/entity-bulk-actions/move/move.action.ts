import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_MEDIA_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: the picker should be single picker by default
		const modalHandler = this.#modalContext?.open(UMB_MEDIA_PICKER_MODAL, {
			selection: [],
			multiple: false,
		});
		if (modalHandler) {
			const { selection } = await modalHandler.onSubmit();
			const destination = selection[0];
			await this.repository?.move(this.selection, destination);
		}
	}
}
