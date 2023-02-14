import { UmbWorkspaceAction } from '../components/workspace/workspace-action';
import { UmbWorkspaceContextInterface } from '../components/workspace/workspace-context/workspace-context.interface';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

// TODO: add interface for repo/partial repo/save-repo
export class UmbSaveWorkspaceAction extends UmbWorkspaceAction<any, UmbWorkspaceContextInterface> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string) {
		super(host, repositoryAlias);
	}

	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const data = this.workspaceContext.getData();
		// TODO: handle errors
		if (!data) return;
		this.repository?.saveDetail(data);
	}
}
