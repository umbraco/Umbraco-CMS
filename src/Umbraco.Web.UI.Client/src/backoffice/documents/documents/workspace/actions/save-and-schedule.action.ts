import { UmbWorkspaceAction } from '@umbraco-cms/workspace';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveAndScheduleDocumentWorkspaceAction extends UmbWorkspaceAction<UmbDocumentWorkspaceContext> {
	constructor(host: UmbControllerHostInterface) {
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
