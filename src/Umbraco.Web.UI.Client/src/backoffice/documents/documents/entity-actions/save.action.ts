import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbDocumentWorkspaceContext } from '../workspace/document-workspace.context';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbEntityActionBase } from 'src/backoffice/shared/components/entity-action';

export class UmbSaveDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#workspaceContext?: UmbDocumentWorkspaceContext;

	constructor(host: UmbControllerHostInterface, unique: string) {
		super(host, UmbDocumentRepository, unique);

		// TODO: add context token for workspace
		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: UmbDocumentWorkspaceContext) => {
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
		this.repository.saveDetail(document);
	}
}
