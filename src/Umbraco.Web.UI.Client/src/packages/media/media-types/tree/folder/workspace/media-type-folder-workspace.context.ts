import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbMediaTypeFolderRepository } from '../repository/index.js';
import { UMB_EDIT_MEDIA_TYPE_FOLDER_WORKSPACE_PATH_PATTERN } from './paths.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbMediaTypeFolderWorkspaceEditorElement } from './media-type-folder-editor.element.js';
import { UMB_MEDIA_TYPE_ROOT_WORKSPACE_PATH } from '../../../paths.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbMediaTypeFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbMediaTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
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

	protected override _getNavigationParentItemPath(entity: UmbEntityModel | undefined): string | undefined {
		if (!entity?.unique) return UMB_MEDIA_TYPE_ROOT_WORKSPACE_PATH;
		return UMB_EDIT_MEDIA_TYPE_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}
}

export { UmbMediaTypeFolderWorkspaceContext as api };
