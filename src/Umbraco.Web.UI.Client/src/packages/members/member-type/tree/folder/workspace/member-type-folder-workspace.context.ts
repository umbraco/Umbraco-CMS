import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbMemberTypeFolderRepository } from '../repository/index.js';
import { UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbMemberTypeFolderWorkspaceEditorElement } from './member-type-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbMemberTypeFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbMemberTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbMemberTypeFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}
}

export { UmbMemberTypeFolderWorkspaceContext as api };
