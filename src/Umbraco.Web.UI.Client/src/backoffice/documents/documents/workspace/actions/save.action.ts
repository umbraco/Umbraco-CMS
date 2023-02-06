import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbWorkspaceAction } from '../../../../shared/components/workspace/workspace-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveDocumentWorkspaceAction extends UmbWorkspaceAction<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const document = this.workspaceContext.getData();
		// TODO: handle errors
		if (!document) return;
		this.repository?.saveDetail(document);
	}
}
