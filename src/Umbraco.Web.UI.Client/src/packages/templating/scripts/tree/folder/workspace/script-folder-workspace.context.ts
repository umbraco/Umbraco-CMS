import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS, type UmbScriptFolderRepository } from '../repository/index.js';
import { UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_SCRIPT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbScriptFolderWorkspaceEditorElement } from './script-folder-workspace-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbScriptFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbScriptFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
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
}

export { UmbScriptFolderWorkspaceContext as api };
