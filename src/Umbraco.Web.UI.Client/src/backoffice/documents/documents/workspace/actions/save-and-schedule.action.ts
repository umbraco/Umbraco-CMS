import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbSaveAndScheduleDocumentWorkspaceAction extends UmbWorkspaceActionBase<UmbDocumentWorkspaceContext> {
	constructor(host: UmbControllerHostElement) {
		super(host);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const document = this.workspaceContext.getData();
		// TODO: handle errors
		if (!document) return;
		this.workspaceContext.repository.saveAndSchedule();
	}
}
