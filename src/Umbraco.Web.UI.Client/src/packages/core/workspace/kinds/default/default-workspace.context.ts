import { UMB_WORKSPACE_CONTEXT } from '../../workspace.context-token.js';
import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

export abstract class UmbDefaultWorkspaceContext
	extends UmbContextBase<UmbDefaultWorkspaceContext>
	implements UmbWorkspaceContext
{
	public workspaceAlias!: string;

	#entityType!: string;
	#unique: string | null = null;

	#entityContext = new UmbEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_CONTEXT.toString());
	}

	set manifest(manifest: ManifestWorkspace) {
		this.workspaceAlias = manifest.alias;
		this.#entityType = manifest.meta.entityType;

		this.#entityContext.setEntityType(this.#entityType);
		this.#entityContext.setUnique(this.#unique);
	}

	getUnique(): string | null {
		return this.#unique;
	}

	getEntityType(): string {
		return this.#entityType;
	}
}

export { UmbDefaultWorkspaceContext as api };
