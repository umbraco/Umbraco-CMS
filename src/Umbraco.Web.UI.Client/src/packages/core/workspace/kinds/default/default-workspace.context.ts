import { UMB_WORKSPACE_CONTEXT } from '../../workspace.context-token.js';
import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { ManifestWorkspaceDefaultKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbEntityContext, type UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';

export class UmbDefaultWorkspaceContext extends UmbContextBase implements UmbWorkspaceContext {
	public workspaceAlias!: string;

	#entityContext = new UmbEntityContext(this);

	public readonly view = new UmbViewContext(this, null);

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_CONTEXT.toString());
	}

	set manifest(manifest: ManifestWorkspaceDefaultKind) {
		this.workspaceAlias = manifest.alias;
		this.setEntityType(manifest.meta.entityType);
		this.view.setTitle(manifest.meta.headline);
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
