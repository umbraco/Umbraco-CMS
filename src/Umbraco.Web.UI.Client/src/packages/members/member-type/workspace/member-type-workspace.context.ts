import type { UmbMemberTypeDetailModel } from '../types.js';
import { UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_ENTITY_TYPE } from '../index.js';
import { UmbMemberTypeWorkspaceEditorElement } from './member-type-workspace-editor.element.js';
import { UMB_MEMBER_TYPE_WORKSPACE_ALIAS } from './constants.js';
import {
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbContentTypeWorkspaceContext,
	UmbContentTypeWorkspaceContextBase,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

type EntityDetailModel = UmbMemberTypeDetailModel;
export class UmbMemberTypeWorkspaceContext
	extends UmbContentTypeWorkspaceContextBase<EntityDetailModel>
	implements UmbContentTypeWorkspaceContext<EntityDetailModel>, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:parentEntityType/:parentUnique',
				component: UmbMemberTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const parent: UmbEntityModel = { entityType: parentEntityType, unique: parentUnique };
					await this.createScaffold({ parent });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbMemberTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	/**
	 * @deprecated Use the individual set methods instead. Will be removed in 17.
	 * @template PropertyName
	 * @param {PropertyName} propertyName
	 * @param {EntityDetailModel[PropertyName]} value
	 * @memberof UmbMemberTypeWorkspaceContext
	 */
	set<PropertyName extends keyof EntityDetailModel>(
		propertyName: PropertyName,
		value: EntityDetailModel[PropertyName],
	) {
		this.structure.updateOwnerContentType({ [propertyName]: value });
	}

	/**
	 * @deprecated Use the createScaffold method instead. Will be removed in 17.
	 * @param {UmbEntityModel} parent
	 * @memberof UmbMemberTypeWorkspaceContext
	 */
	async create(parent: UmbEntityModel) {
		this.createScaffold({ parent });
	}
}

export { UmbMemberTypeWorkspaceContext as api };
