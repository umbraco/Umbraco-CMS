import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS, type UmbElementFolderRepository } from '../repository/index.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbElementFolderWorkspaceEditorElement } from './element-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbElementFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbElementFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		});

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
}

export { UmbElementFolderWorkspaceContext as api };
