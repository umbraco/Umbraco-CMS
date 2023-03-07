import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbEntityBulkActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from 'libs/modal';

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
		const modalHandler = this.#modalContext?.mediaPicker({ selection: [], multiple: false });
		const selection = await modalHandler?.onClose();
		const destination = selection[0];
		await this.repository?.move(this.selection, destination);
	}
}
