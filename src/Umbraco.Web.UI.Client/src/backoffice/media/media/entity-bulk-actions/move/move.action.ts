import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbActionBase } from '../../../../shared/action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class UmbMediaMoveEntityBulkAction extends UmbActionBase<UmbMediaRepository> {
	#selection: Array<string>;
	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.#selection = selection;

		new UmbContextConsumerController(host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	setSelection(selection: Array<string>) {
		this.#selection = selection;
	}

	async execute() {
		// TODO: the picker should be single picker by default
		const modalHandler = this.#modalService?.mediaPicker({ selection: [], multiple: false });
		const selection = await modalHandler?.onClose();
		const destination = selection[0];
		await this.repository?.move(this.#selection, destination);
	}
}
