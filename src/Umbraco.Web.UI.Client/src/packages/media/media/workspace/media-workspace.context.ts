import { UmbMediaTypeDetailRepository } from '../../media-types/repository/detail/media-type-detail.repository.js';
import { UmbMediaPropertyDatasetContext } from '../property-dataset-context/media-property-dataset-context.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import type { UmbMediaDetailModel, UmbMediaVariantModel, UmbMediaVariantOptionModel } from '../types.js';
import { UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN, UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD } from './constants.js';
import { UMB_MEDIA_WORKSPACE_ALIAS } from './manifests.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_INVARIANT_CULTURE } from '@umbraco-cms/backoffice/variant';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbMediaTypeDetailModel } from '@umbraco-cms/backoffice/media-type';
import {
	UmbContentDetailWorkspaceBase,
	UmbContentWorkspaceDataManager,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';

type ContentModel = UmbMediaDetailModel;
type ContentTypeModel = UmbMediaTypeDetailModel;
export class UmbMediaWorkspaceContext
	extends UmbContentDetailWorkspaceBase<ContentModel>
	implements
		UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbMediaVariantModel>,
		UmbContentCollectionWorkspaceContext<ContentTypeModel>
{
	readonly #data = new UmbContentWorkspaceDataManager<ContentModel>(this, UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD);

	readonly contentTypeUnique = this.#data.createObservablePartOfCurrent((data) => data?.mediaType.unique);
	readonly contentTypeHasCollection = this.#data.createObservablePartOfCurrent((data) => !!data?.mediaType.collection);

	readonly urls = this.#data.createObservablePartOfCurrent((data) => data?.urls || []);

	readonly variantOptions = mergeObservables(
		[this.varies, this.variants, this.languages],
		([varies, variants, languages]) => {
			// TODO: When including segments, when be aware about the case of segment varying when not culture varying. [NL]
			if (varies === true) {
				return languages.map((language) => {
					return {
						variant: variants.find((x) => x.culture === language.unique),
						language,
						// TODO: When including segments, this object should be updated to include a object for the segment. [NL]
						// TODO: When including segments, the unique should be updated to include the segment as well. [NL]
						unique: language.unique, // This must be a variantId string!
						culture: language.unique,
						segment: null,
					} as UmbMediaVariantOptionModel;
				});
			} else if (varies === false) {
				return [
					{
						variant: variants.find((x) => x.culture === null),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
					} as UmbMediaVariantOptionModel,
				];
			}
			return [] as Array<UmbMediaVariantOptionModel>;
		},
	);

	// TODO: this might not be the correct place to spin this up
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_MEDIA_ENTITY_TYPE,
			workspaceAlias: UMB_MEDIA_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbMediaTypeDetailRepository,
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./media-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const mediaTypeUnique = info.match.params.mediaTypeUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique }, mediaTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./media-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
		super.resetState();
		this.#data.clear();
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		if (response.data) {
			this.#isTrashedContext.setIsTrashed(response.data.isTrashed);
		}

		return response;
	}

	async create(parent: { entityType: string; unique: string | null }, mediaTypeUnique: string) {
		this.resetState();
		this.#parent.setValue(parent);
		this.#getDataPromise = this.repository.createScaffold({ mediaType: { unique: mediaTypeUnique, collection: null } });
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.#entityContext.setEntityType(UMB_MEDIA_ENTITY_TYPE);
		this.#entityContext.setUnique(data.unique);
		this.setIsNew(true);
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(data);
		return data;
	}

	getCollectionAlias() {
		return UMB_MEDIA_COLLECTION_ALIAS;
	}

	getContentTypeId() {
		return this.getData()?.mediaType.unique;
	}

	async #handleSave() {
		const saveData = this.#data.getCurrent();
		if (!saveData?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			const { data, error } = await this.repository.create(saveData, parent.unique);
			if (!data || error) {
				throw new Error('Error creating document');
			}

			this.setIsNew(false);
			this.#data.setPersisted(data);
			this.#data.setCurrent(data);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			// Save:
			const { data, error } = await this.repository.save(saveData);
			if (!data || error) {
				throw new Error('Error saving document');
			}

			this.#data.setPersisted(data);
			this.#data.setCurrent(data);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			eventContext.dispatchEvent(event);
		}
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbMediaPropertyDatasetContext {
		return new UmbMediaPropertyDatasetContext(host, this, variantId);
	}
}

export { UmbMediaWorkspaceContext as api };
