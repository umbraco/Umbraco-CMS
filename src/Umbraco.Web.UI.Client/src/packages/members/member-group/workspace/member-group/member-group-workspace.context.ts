import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_GROUP_DETAIL_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from './constants.js';
import { UmbMemberGroupWorkspaceEditorElement } from './member-group-workspace-editor.element.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityNamedDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '@umbraco-cms/backoffice/user-group';

export class UmbMemberGroupWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbMemberGroupDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
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
				setup: async () => {
					await this.createScaffold({ parent: { entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE, unique: null } });

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
}

export { UmbMemberGroupWorkspaceContext as api };
