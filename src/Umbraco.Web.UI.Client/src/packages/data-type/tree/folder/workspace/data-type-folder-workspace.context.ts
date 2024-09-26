import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS, type UmbDataTypeFolderRepository } from '../repository/index.js';
import { UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbDataTypeFolderWorkspaceEditorElement } from './data-type-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbFolderModel, UmbDataTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly data = this._data.current;
	public readonly unique = this._data.createObservablePartOfCurrent((data) => data?.unique);
	public readonly entityType = this._data.createObservablePartOfCurrent((data) => data?.entityType);
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DATA_TYPE_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbDataTypeFolderWorkspaceEditorElement,
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

export { UmbDataTypeFolderWorkspaceContext as api };
