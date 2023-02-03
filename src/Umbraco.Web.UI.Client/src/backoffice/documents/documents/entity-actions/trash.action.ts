import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/components/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class UmbTrashDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, key: string) {
		super(host, UmbDocumentRepository, key);

		new UmbContextConsumerController(this.host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		const { data } = await this.repository.requestTreeItems([this.unique]);

		if (data) {
			const item = data[0];

			const modalHandler = this.#modalService?.confirm({
				headline: `Delete ${item.name}`,
				content: 'Are you sure you want to delete this item?',
				color: 'danger',
				confirmLabel: 'Delete',
			});

			modalHandler?.onClose().then(({ confirmed }) => {
				if (confirmed) {
					this.repository.delete(this.unique);
				}
			});
		}
	}
}
