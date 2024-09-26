import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbDocumentTypeFolderRepository } from '../repository/index.js';
import { UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbFolderWorkspaceEditorElement } from './folder-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbScriptWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbFolderModel, UmbDocumentTypeFolderRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly data = this._data.current;
	public readonly unique = this._data.createObservablePartOfCurrent((data) => data?.unique);
	public readonly entityType = this._data.createObservablePartOfCurrent((data) => data?.entityType);
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			entityType: UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
			detailRepositoryAlias: 'Umb.Repository.DocumentType.Folder',
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbFolderWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbFolderWorkspaceEditorElement,
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

export { UmbScriptWorkspaceContext as api };
