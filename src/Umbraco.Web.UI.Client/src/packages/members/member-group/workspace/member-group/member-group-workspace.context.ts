import { UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberGroupWorkspaceEditorElement } from './member-group-workspace-editor.element.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '@umbraco-cms/backoffice/user-group';

export class UmbMemberGroupWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbMemberGroupDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly unique = this._data.createObservablePartOfCurrent((data) => data?.unique);
	readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbMemberGroupWorkspaceEditorElement,
				setup: () => {
					this.createScaffold({ parent: { entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbMemberGroupWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		this.observe(
			response.asObservable?.(),
			(memberGroup) => this.#onMemberGroupStoreChange(memberGroup),
			'umbMemberGroupStoreObserver',
		);

		return response;
	}

	#onMemberGroupStoreChange(memberGroup: UmbMemberGroupDetailModel | undefined) {
		if (!memberGroup) {
			history.pushState(null, '', 'section/member-management/view/member-groups');
		}
	}

	getName() {
		return this._data.getCurrent()?.name;
	}

	setName(name: string | undefined) {
		this._data.updateCurrent({ name });
	}
}

export { UmbMemberGroupWorkspaceContext as api };
