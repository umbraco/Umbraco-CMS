import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../repository/constants.js';
import type { UmbElementFolderRepository } from '../repository/index.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE,
} from '../user-permissions/constants.js';
import type { UmbElementFolderModel } from '../types.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH } from '../../recycle-bin/root/workspace/constants.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from './paths.js';
import { UmbElementFolderWorkspaceEditorElement } from './element-folder-editor.element.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityNamedDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UMB_LIBRARY_SECTION_PATH } from '@umbraco-cms/backoffice/library';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export class UmbElementFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbElementFolderModel, UmbElementFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		});

		this.observe(
			this.isNew,
			(isNew) => {
				if (isNew === undefined) return;
				this.#enforceUserPermission(
					UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE,
					'You do not have permission to update element folders.',
				);
			},
			null,
		);

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbElementFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override _getNavigationParentItemPath(entity: UmbEntityModel | undefined): string | undefined {
		if (!entity?.unique) {
			return this._data.getCurrent()?.isTrashed
				? UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH
				: UMB_LIBRARY_SECTION_PATH;
		}
		return UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}

	#enforcedVerbs = new Map<string, Promise<UmbApi>>();

	#enforceUserPermission(verb: string, message: string) {
		if (this.#enforcedVerbs.has(verb)) return;

		// We set the initial permission state to false because the condition is false by default and only execute the callback if it changes.
		this.#handleUserPermissionChange(verb, false, message);

		const condition = createExtensionApiByAlias(this, UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [verb],
				},
				onChange: (permitted: boolean) => {
					this.#handleUserPermissionChange(verb, permitted, message);
				},
			},
		]);
		this.#enforcedVerbs.set(verb, condition);
	}

	override resetState() {
		this.#enforcedVerbs.forEach((condition) => condition.then((api) => api.destroy()).catch(() => undefined));
		this.#enforcedVerbs.clear();
		super.resetState();
	}

	#handleUserPermissionChange(identifier: string, permitted: boolean, message: string) {
		if (permitted) {
			this.readOnlyGuard.removeRule(identifier);
			return;
		}

		this.readOnlyGuard.addRule({
			unique: identifier,
			message,
			// A rule permitted to be read-only, so without the permission the folder is read-only.
			permitted: true,
		});
	}
}

export { UmbElementFolderWorkspaceContext as api };
