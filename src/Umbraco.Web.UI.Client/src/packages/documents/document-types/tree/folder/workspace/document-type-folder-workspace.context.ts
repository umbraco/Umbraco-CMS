import {
	UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
	type UmbDocumentTypeFolderRepository,
} from '../repository/index.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbDocumentTypeFolderWorkspaceEditorElement } from './document-type-folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTypeFolderWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbFolderModel, UmbDocumentTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbDocumentTypeFolderWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	/**
	 * @description Set the name of the document type folder
	 * @param {string} value
	 * @memberof UmbDocumentTypeFolderWorkspaceContext
	 */
	public setName(value: string) {
		this._data.updateCurrent({ name: value });
	}

	/**
	 * @description Get the name of the document type folder
	 * @returns {string}
	 * @memberof UmbDocumentTypeFolderWorkspaceContext
	 */
	public getName() {
		return this._data.getCurrent()?.name;
	}
}

export { UmbDocumentTypeFolderWorkspaceContext as api };
