import { UMB_WORKSPACE_CONTEXT } from './tokens/workspace.context-token.js';
import type { UmbWorkspaceContext } from './tokens/workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';

export abstract class UmbDefaultWorkspaceContext
	extends UmbContextBase<UmbDefaultWorkspaceContext>
	implements UmbWorkspaceContext
{
	public workspaceAlias!: string;
	#entityType!: string;

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_CONTEXT.toString());
	}

	set manifest(manifest: ManifestWorkspace) {
		this.workspaceAlias = manifest.alias;
		this.#entityType = manifest.meta.entityType;
	}

	getUnique(): string | undefined {
		return undefined;
	}
	getEntityType(): string {
		return this.#entityType;
	}
}

export { UmbDefaultWorkspaceContext as api };
