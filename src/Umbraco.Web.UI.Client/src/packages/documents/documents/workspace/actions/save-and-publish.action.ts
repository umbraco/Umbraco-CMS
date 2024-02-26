import type { UmbDocumentWorkspaceContext } from '../document-workspace.context.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentSaveAndPublishWorkspaceAction extends UmbWorkspaceActionBase<UmbDocumentWorkspaceContext> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: Revisit, its unclear to me why we check the data of the context, should not matter IMO.
		// TODO: it doesn't get the updated value
		//const document = this.workspaceContext.getData();
		// TODO: handle errors
		//if (!document) return;
		this.workspaceContext.saveAndPublish();
	}
}
