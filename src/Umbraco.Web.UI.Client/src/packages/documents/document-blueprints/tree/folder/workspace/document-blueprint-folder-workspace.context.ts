import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbDocumentBlueprintFolderRepository } from '../repository/index.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UMB_EDIT_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_PATH_PATTERN } from './paths.js';
import { UmbDocumentBlueprintFolderWorkspaceEditorElement } from './document-blueprint-folder-editor.element.js';
import { UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_PATH } from '../../../paths.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbDocumentBlueprintFolderWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbFolderModel, UmbDocumentBlueprintFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbDocumentBlueprintFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override _getNavigationParentItemPath(entity: UmbEntityModel | undefined): string | undefined {
		if (!entity?.unique) return UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_PATH;
		return UMB_EDIT_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}
}

export { UmbDocumentBlueprintFolderWorkspaceContext as api };
