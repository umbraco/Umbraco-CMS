import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS, type UmbScriptFolderRepository } from '../repository/index.js';
import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbScriptFolderWorkspaceEditorElement } from './script-type-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbScriptFolderWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbFolderModel, UmbScriptFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_SCRIPT_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbScriptFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	/**
	 * @description Set the name of the script
	 * @param {string} value
	 * @memberof UmbScriptWorkspaceContext
	 */
	public setName(value: string) {
		this._data.updateCurrent({ name: value });
	}
}

export { UmbScriptFolderWorkspaceContext as api };
