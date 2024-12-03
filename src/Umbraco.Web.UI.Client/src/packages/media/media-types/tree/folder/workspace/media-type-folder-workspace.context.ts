import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbMediaTypeFolderRepository } from '../repository/index.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbMediaTypeFolderWorkspaceEditorElement } from './media-type-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTypeFolderWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbFolderModel, UmbMediaTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbMediaTypeFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	/**
	 * @description Set the name of the media type folder
	 * @param {string} value
	 * @memberof UmbMediaTypeFolderWorkspaceContext
	 */
	public setName(value: string) {
		this._data.updateCurrent({ name: value });
	}

	/**
	 * @description Get the name of the media type folder
	 * @returns {string}
	 * @memberof UmbMediaTypeFolderWorkspaceContext
	 */
	public getName() {
		return this._data.getCurrent()?.name;
	}
}

export { UmbMediaTypeFolderWorkspaceContext as api };
