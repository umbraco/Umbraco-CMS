import { UmbMediaTypeDetailRepository } from '../repository/detail/media-type-detail.repository.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTypeDetailModel } from '../types.js';
import { UmbMediaTypeWorkspaceEditorElement } from './media-type-workspace-editor.element.js';
import {
	UmbSubmittableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbContentTypeStructureManager,
	UmbContentTypeWorkspaceContextBase,
} from '@umbraco-cms/backoffice/content-type';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type {
	UmbContentTypeCompositionModel,
	UmbContentTypeSortModel,
	UmbContentTypeWorkspaceContext,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MEDIA_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';

type DetailModelType = UmbMediaTypeDetailModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbContentTypeWorkspaceContextBase<DetailModelType>
	implements UmbContentTypeWorkspaceContext<DetailModelType>, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEDIA_TYPE_WORKSPACE_ALIAS,
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:parentEntityType/:parentUnique',
				component: UmbMediaTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
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
				path: 'edit/:id',
				component: UmbMediaTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.load(id);
				},
			},
		]);
	}

	setAllowedAtRoot(allowedAtRoot: boolean) {
		this.structure.updateOwnerContentType({ allowedAtRoot });
	}

	setVariesByCulture(variesByCulture: boolean) {
		this.structure.updateOwnerContentType({ variesByCulture });
	}

	setVariesBySegment(variesBySegment: boolean) {
		this.structure.updateOwnerContentType({ variesBySegment });
	}

	setIsElement(isElement: boolean) {
		this.structure.updateOwnerContentType({ isElement });
	}

	setAllowedContentTypes(allowedContentTypes: Array<UmbContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}

	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	setCollection(collection: UmbReferenceByUnique) {
		this.structure.updateOwnerContentType({ collection });
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.structure.createScaffold();
		if (!data) return undefined;

		this.setIsNew(true);
		this.#persistedData.setValue(data);
		return data;
	}

	async load(unique: string) {
		this.resetState();
		const { data, asObservable } = await this.structure.loadType(unique);

		if (data) {
			this.setIsNew(false);
			this.#persistedData.update(data);
		}

		if (asObservable) {
			this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbMediaTypeStoreObserver');
		}
	}

	/**
	 * Save or creates the media type, based on wether its a new one or existing.
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

export { UmbMediaTypeWorkspaceContext as api };
