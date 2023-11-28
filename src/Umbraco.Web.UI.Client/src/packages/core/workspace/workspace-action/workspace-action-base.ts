import { UmbWorkspaceContextInterface, UMB_WORKSPACE_CONTEXT } from '../workspace-context/index.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceAction extends UmbApi {
	execute(): Promise<void>;
}

export abstract class UmbWorkspaceActionBase<WorkspaceContextType extends UmbWorkspaceContextInterface>
	extends UmbBaseController
	implements UmbWorkspaceAction
{
	workspaceContext?: WorkspaceContextType;
	constructor(host: UmbControllerHost) {
		super(host);

		// TODO, we most likely should require a context token here in this type, and mane it specifically for workspace actions with context workspace request.
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			// TODO: Be aware we are casting here. We should consider a better solution for typing the contexts. (But notice we still want to capture the first workspace...)
			this.workspaceContext = instance as unknown as WorkspaceContextType;
		});
	}
	abstract execute(): Promise<void>;
}
