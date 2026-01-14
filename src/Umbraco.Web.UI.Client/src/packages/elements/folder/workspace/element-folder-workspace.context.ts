import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS, type UmbElementFolderRepository } from '../repository/index.js';
import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbElementFolderModel } from '../types.js';
import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbElementFolderWorkspaceEditorElement } from './element-folder-editor.element.js';
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

	#onTrashStateChange(isTrashed?: boolean) {
		this.#isTrashedContext.setIsTrashed(isTrashed ?? false);
	}
}

export { UmbElementFolderWorkspaceContext as api };
