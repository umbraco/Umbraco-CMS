import type { UmbStylesheetDetailRepository } from '../repository/stylesheet-detail.repository.js';
import type { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_STYLESHEET_WORKSPACE_ALIAS } from './manifests.js';
import { UmbStylesheetWorkspaceEditorElement } from './stylesheet-workspace-editor.element.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityNamedDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbServerFileRenameWorkspaceRedirectController } from '@umbraco-cms/backoffice/server-file-system';

export class UmbStylesheetWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbStylesheetDetailModel, UmbStylesheetDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_STYLESHEET_WORKSPACE_ALIAS,
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			detailRepositoryAlias: UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbStylesheetWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbStylesheetWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					// TODO: Decode uniques [NL]
					const unique = info.match.params.unique;
					this.load(unique);

					new UmbServerFileRenameWorkspaceRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
		]);
	}

	/**
	 * @description Set the content of the stylesheet
	 * @param {string} value The content of the stylesheet
	 * @memberof UmbStylesheetWorkspaceContext
	 */
	public setContent(value: string) {
		this._data.updateCurrent({ content: value });
	}

	/**
	 * @description Create a new stylesheet
	 * @deprecated Use `createScaffold` instead. Will be removed in v17.
	 * @param { UmbEntityModel } parent The parent entity
	 * @param { string } parent.entityType The entity type of the parent
	 * @param { UmbEntityUnique } parent.unique The unique identifier of the parent
	 */
	async create(parent: UmbEntityModel) {
		await this.createScaffold({ parent });
	}
}

export { UmbStylesheetWorkspaceContext as api };
