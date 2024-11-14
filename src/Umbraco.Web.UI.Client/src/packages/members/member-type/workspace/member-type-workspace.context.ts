import { UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS, UmbMemberTypeDetailRepository } from '../repository/detail/index.js';
import type { UmbMemberTypeDetailModel } from '../types.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../index.js';
import { UmbMemberTypeWorkspaceEditorElement } from './member-type-workspace-editor.element.js';
import {
	UmbSubmittableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbContentTypeCompositionModel,
	UmbContentTypeStructureManager,
	type UmbContentTypeWorkspaceContext,
	UmbContentTypeWorkspaceContextBase,
} from '@umbraco-cms/backoffice/content-type';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MEMBER_TYPE_WORKSPACE_ALIAS } from './manifests.js';
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
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbMemberTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.create({ entityType: parentEntityType, unique: parentUnique });

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
	 * @deprecated Use the individual set methods instead
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

	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	async create(parent: UmbEntityModel) {
		this.createScaffold({ parent });
	}

	/**
	 * Save or creates the member type, based on wether its a new one or existing.
	 */
	async submit() {
		const data = this.getData();
		if (!data) {
			throw new Error('Something went wrong, there is no data for media type you want to save...');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			await this.structure.create(parent.unique);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
			this.setIsNew(false);
		} else {
			await this.structure.save();

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}
}

export { UmbMemberTypeWorkspaceContext as api };
