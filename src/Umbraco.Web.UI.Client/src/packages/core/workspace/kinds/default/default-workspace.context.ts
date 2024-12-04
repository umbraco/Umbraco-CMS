import { UMB_WORKSPACE_CONTEXT } from '../../workspace.context-token.js';
import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/workspace';
import { UmbEntityContext, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export class UmbDefaultWorkspaceContext
	extends UmbContextBase<UmbDefaultWorkspaceContext>
	implements UmbWorkspaceContext
{
	public workspaceAlias!: string;

	#entityContext = new UmbEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_CONTEXT.toString());
	}

	set manifest(manifest: ManifestWorkspace) {
		this.workspaceAlias = manifest.alias;
		this.setEntityType(manifest.meta.entityType);
	}

	setUnique(unique: UmbEntityUnique): void {
		this.#entityContext.setUnique(unique);
	}

	getUnique(): UmbEntityUnique {
		return this.#entityContext.getUnique();
	}

	setEntityType(entityType: string): void {
		this.#entityContext.setEntityType(entityType);
	}

	getEntityType(): string {
		return this.#entityContext.getEntityType()!;
	}
}

export { UmbDefaultWorkspaceContext as api };
