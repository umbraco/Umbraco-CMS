import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS, type UmbStylesheetFolderRepository } from '../repository/index.js';
import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbStylesheetFolderWorkspaceEditorElement } from './stylesheet-folder-workspace-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbStylesheetFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbStylesheetFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_STYLESHEET_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbStylesheetFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}
}

export { UmbStylesheetFolderWorkspaceContext as api };
