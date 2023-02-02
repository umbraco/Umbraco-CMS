import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class TrashDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;
	#modalService?: UmbModalService;
	#documentRepository: UmbDocumentRepository;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
		this.#documentRepository = new UmbDocumentRepository(this.#host); // TODO: make repository injectable

		new UmbContextConsumerController(this.#host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		const { data } = await this.#documentRepository.requestTreeItems([this.#key]);

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
					this.#documentRepository.delete(this.#key);
				}
			});
		}
	}
}
