import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbWorkspaceActionBase } from '@umbraco-cms/workspace';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbDocumentSaveAndPublishWorkspaceAction extends UmbWorkspaceActionBase<UmbDocumentWorkspaceContext> {
	constructor(host: UmbControllerHostInterface) {
		super(host);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const document = this.workspaceContext.getData();
		// TODO: handle errors
		if (!document) return;
		this.workspaceContext.repository.saveAndPublish();
	}
}
