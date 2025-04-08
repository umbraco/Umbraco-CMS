import type { UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import type { UmbPartialViewDetailRepository } from '../repository/index.js';
import { UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS } from '../constants.js';
import { UmbPartialViewWorkspaceEditorElement } from './partial-view-workspace-editor.element.js';
import { UMB_PARTIAL_VIEW_WORKSPACE_ALIAS } from './manifests.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbEntityDetailWorkspaceContextCreateArgs,
	UmbRoutableWorkspaceContext,
	UmbSubmittableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { PartialViewService } from '@umbraco-cms/backoffice/external/backend-api';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbServerFileRenameWorkspaceRedirectController } from '@umbraco-cms/backoffice/server-file-system';

export interface UmbPartialViewWorkspaceContextCreateArgs
	extends UmbEntityDetailWorkspaceContextCreateArgs<UmbPartialViewDetailModel> {
	snippet: { unique: string } | null;
}

export class UmbPartialViewWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<
		UmbPartialViewDetailModel,
		UmbPartialViewDetailRepository,
		UmbPartialViewWorkspaceContextCreateArgs
	>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_PARTIAL_VIEW_WORKSPACE_ALIAS,
			entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
			detailRepositoryAlias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique/snippet/:snippetId',
				component: UmbPartialViewWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const snippetId = info.match.params.snippetId;
					this.#onCreate({
						parent: { entityType: parentEntityType, unique: parentUnique },
						snippet: { unique: snippetId },
					});
				},
			},
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbPartialViewWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.#onCreate({
						parent: { entityType: parentEntityType, unique: parentUnique },
						snippet: null,
					});
				},
			},
			{
				path: 'edit/:unique',
				component: UmbPartialViewWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
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

	#onCreate = async (args: UmbPartialViewWorkspaceContextCreateArgs) => {
		await this.createScaffold(args);

		new UmbWorkspaceIsNewRedirectController(
			this,
			this,
			this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
		);
	};

	setContent(value: string) {
		this._data.updateCurrent({ content: value });
	}

	override async createScaffold(args: UmbPartialViewWorkspaceContextCreateArgs) {
		let snippetContent = '';

		if (args.snippet?.unique) {
			const { data: snippet } = await this.#getSnippet(args.snippet.unique);
			snippetContent = snippet?.content || '';
		}

		const argsWithPreset = { ...args, preset: { content: snippetContent } };

		return super.createScaffold(argsWithPreset);
	}

	#getSnippet(unique: string) {
		return tryExecute(
			this,
			PartialViewService.getPartialViewSnippetById({
				id: unique,
			}),
		);
	}
}

export { UmbPartialViewWorkspaceContext as api };
