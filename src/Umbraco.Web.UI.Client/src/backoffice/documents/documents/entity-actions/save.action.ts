import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbDocumentWorkspaceContext } from '../workspace/document-workspace.context';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;
	#documentRepository: UmbDocumentRepository;
	#workspaceContext?: UmbDocumentWorkspaceContext;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
		this.#documentRepository = new UmbDocumentRepository(this.#host); // TODO: make repository injectable

		// TODO: add context token for workspace
		new UmbContextConsumerController(this.#host, 'umbWorkspaceContext', (instance: UmbDocumentWorkspaceContext) => {
			this.#workspaceContext = instance;
		});
	}

	async execute() {
		if (!this.#workspaceContext) {
			alert('the actions doesnt work here');
			return;
		}
		// TODO: it doesn't get the updated value
		const document = this.#workspaceContext.getData();
		// TODO: handle errors
		if (!document) return;
		this.#documentRepository.saveDetail(document);
	}
}
