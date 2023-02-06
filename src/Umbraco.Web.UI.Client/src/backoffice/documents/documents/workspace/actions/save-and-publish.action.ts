import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbWorkspaceAction } from '../../../../shared/components/workspace/workspace-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveAndPublishDocumentWorkspaceAction extends UmbWorkspaceAction<UmbDocumentRepository> {
	#workspaceContext?: UmbDocumentWorkspaceContext;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const document = this.workspaceContext.getData();
		// TODO: handle errors
		if (!document) return;
		this.repository?.saveAndPublish();
	}
}
