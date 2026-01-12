import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../../workspace-context/constants.js';
import { UMB_ELEMENT_WORKSPACE_CONTEXT } from '../../../workspace/element-workspace.context-token.js';
import { UmbWorkspaceActionBase, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementSaveAndPublishWorkspaceAction extends UmbWorkspaceActionBase {
	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<never>) {
		super(host, args);
	}

	async hasAdditionalOptions() {
		const workspaceContext = await this.getContext(UMB_ELEMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		const variantOptions = await this.observe(workspaceContext.variantOptions).asPromise();
		const cultureVariantOptions = variantOptions?.filter((option) => option.segment === null);
		return cultureVariantOptions?.length > 1;
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		return workspaceContext.saveAndPublish();
	}
}

export { UmbElementSaveAndPublishWorkspaceAction as api };
