import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../repository/constants.js';
import type { UmbElementFolderRepository } from '../repository/index.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE,
} from '../user-permissions/constants.js';
import type { UmbElementFolderModel } from '../types.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbElementFolderWorkspaceEditorElement } from './element-folder-editor.element.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityNamedDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export class UmbElementFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbElementFolderModel, UmbElementFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);

	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		});

		this.observe(this.isTrashed, (isTrashed) => this.#onTrashStateChange(isTrashed));

		this.#setupNameWritePermissions();

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

	#setupNameWritePermissions() {
		const guardUnique = 'UMB_ALLOW_FOLDER_RENAME_WITH_PERMISSION';

		// Default to not permitted until the condition confirms permission.
		this.nameWriteGuard.fallbackToNotPermitted();

		createExtensionApiByAlias(this, UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE],
				},
				onChange: (permitted: boolean) => {
					if (permitted) {
						this.nameWriteGuard.addRule({
							unique: guardUnique,
							permitted: true,
						});
					} else {
						this.nameWriteGuard.removeRule(guardUnique);
					}
				},
			},
		]);
	}

	#onTrashStateChange(isTrashed?: boolean) {
		this.#isTrashedContext.setIsTrashed(isTrashed ?? false);

		const guardUnique = 'UMB_PREVENT_TRASHED_FOLDER_RENAME';

		if (isTrashed) {
			this.nameWriteGuard.addRule({
				unique: guardUnique,
				permitted: false,
				message: 'Cannot rename folders in the recycle bin.',
			});
		} else {
			this.nameWriteGuard.removeRule(guardUnique);
		}
	}
}

export { UmbElementFolderWorkspaceContext as api };
