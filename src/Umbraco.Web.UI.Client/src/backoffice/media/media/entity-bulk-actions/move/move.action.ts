import type { UmbMediaRepository } from '../../repository/media.repository';
import { UMB_MEDIA_PICKER_MODAL_TOKEN } from '../../modals/media-picker';
import { UmbEntityBulkActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: the picker should be single picker by default
		const modalHandler = this.#modalContext?.open(UMB_MEDIA_PICKER_MODAL_TOKEN, {
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
