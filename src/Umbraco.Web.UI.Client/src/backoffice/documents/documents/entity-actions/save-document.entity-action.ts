import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class SaveDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;
	#documentRepository: UmbDocumentRepository;
	#workspaceContext: any;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
		this.#documentRepository = new UmbDocumentRepository(this.#host); // TODO: make repository injectable

		new UmbContextConsumerController(this.#host, 'umbWorkspaceContext', (instance) => {
			this.#workspaceContext = instance;
		});
	}

	async execute() {
		// TODO: it doesn't get the updated value
		const document = this.#workspaceContext.getData();
		this.#documentRepository.saveDetail(document);
	}
}
