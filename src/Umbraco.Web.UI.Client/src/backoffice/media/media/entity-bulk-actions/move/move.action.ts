import { UmbEntityBulkActionBase } from '@umbraco-cms/entity-action';
import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<UmbMediaRepository> {
	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias, selection);

		new UmbContextConsumerController(host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		// TODO: the picker should be single picker by default
		const modalHandler = this.#modalService?.mediaPicker({ selection: [], multiple: false });
		const selection = await modalHandler?.onClose();
		const destination = selection[0];
		await this.repository?.move(this.selection, destination);
	}
}
