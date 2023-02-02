import { UmbTemplateTreeRepository } from '../../../templating/templates/tree/data/template.tree.repository';
import { UmbTemplateDetailRepository } from '../../../templating/templates/workspace/data/template.detail.repository';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export class TrashDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;
	#modalService?: UmbModalService;
	#documentDetailRepo: UmbTemplateDetailRepository;
	#documentTreeRepo: UmbTemplateTreeRepository;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
		this.#documentTreeRepo = new UmbTemplateTreeRepository(this.#host); // TODO: change to document repo
		this.#documentDetailRepo = new UmbTemplateDetailRepository(this.#host); // TODO: change to document repo

		new UmbContextConsumerController(this.#host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		const { data } = await this.#documentTreeRepo.requestItems([this.#key]);

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
					this.#documentDetailRepo.delete(this.#key);
				}
			});
		}
	}
}
