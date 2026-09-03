import type { UmbStructureItemModel } from './types.js';
import { UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT } from './menu-structure-workspace-context.context-token.js';
import type { UmbMenuStructureWorkspaceContext } from './menu-structure-workspace-context.interface.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';

/**
 * Base class for a menu structure workspace context for an entity that is organized as a flat
 * list of items directly under a single, fixed root (such as Users or Webhooks), rather than a tree.
 * @abstract
 * @class UmbMenuListStructureWorkspaceContextBase
 * @augments {UmbContextBase}
 */
export abstract class UmbMenuListStructureWorkspaceContextBase
	extends UmbContextBase
	implements UmbMenuStructureWorkspaceContext
{
	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parentContext = new UmbParentEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT);
		// 'UmbMenuStructureWorkspaceContext' is Obsolete, will be removed in v.18
		this.provideContext('UmbMenuStructureWorkspaceContext', this);
	}

	/**
	 * Sets the structure, ordered root first and the current item last, and updates the parent
	 * entity context to the item preceding the current one.
	 * @param {Array<UmbStructureItemModel>} items - The structure items, root first, current item last.
	 * @protected
	 * @memberof UmbMenuListStructureWorkspaceContextBase
	 */
	protected _setStructure(items: Array<UmbStructureItemModel>): void {
		this.#structure.setValue(items);

		const current = items[items.length - 1];
		const parent = items.filter((item) => item.unique !== current?.unique).pop();
		this.#parentContext.setParent(parent ? { unique: parent.unique, entityType: parent.entityType } : undefined);
	}
}
