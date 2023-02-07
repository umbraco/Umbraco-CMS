import { UmbWorkspaceAction } from '../../../../shared/components/workspace/workspace-action';
import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveDocumentWorkspaceAction extends UmbWorkspaceAction<
	UmbDocumentRepository,
	UmbDocumentWorkspaceContext
> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string) {
		super(host, repositoryAlias);
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
