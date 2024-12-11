import { UmbMediaTypeDetailRepository } from '../../media-types/repository/detail/media-type-detail.repository.js';
import { UmbMediaPropertyDatasetContext } from '../property-dataset-context/media-property-dataset-context.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbMediaDetailModel, UmbMediaVariantModel } from '../types.js';
import { UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN, UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/constants.js';
import type { UmbMediaDetailRepository } from '../repository/index.js';
import { UMB_MEDIA_WORKSPACE_ALIAS, UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD } from './constants.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbMediaTypeDetailModel } from '@umbraco-cms/backoffice/media-type';
import {
	UmbContentDetailWorkspaceContextBase,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';

type ContentModel = UmbMediaDetailModel;
type ContentTypeModel = UmbMediaTypeDetailModel;

export class UmbMediaWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbMediaDetailRepository,
		ContentTypeModel,
		UmbMediaVariantModel
	>
	implements
		UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbMediaVariantModel>,
		UmbContentCollectionWorkspaceContext<ContentTypeModel>
{
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.mediaType.unique);
	readonly contentTypeHasCollection = this._data.createObservablePartOfCurrent((data) => !!data?.mediaType.collection);

	readonly urls = this._data.createObservablePartOfCurrent((data) => data?.urls || []);

	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_MEDIA_ENTITY_TYPE,
			workspaceAlias: UMB_MEDIA_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbMediaTypeDetailRepository,
			contentVariantScaffold: UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD,
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./media-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const mediaTypeUnique = info.match.params.mediaTypeUnique;
					await this.createScaffold({
						parent: { entityType: parentEntityType, unique: parentUnique },
						preset: { mediaType: { unique: mediaTypeUnique, collection: null } },
					});

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

	public override resetState() {
		super.resetState();
		this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
	}

	public override async load(unique: string) {
		const response = await super.load(unique);

		if (response.data) {
			this.#isTrashedContext.setIsTrashed(response.data.isTrashed);
		}

		return response;
	}

	/*
	 * @deprecated Use `createScaffold` instead.
	 */
	public async create(parent: { entityType: string; unique: string | null }, mediaTypeUnique: string) {
		const args = {
			parent,
			preset: { mediaType: { unique: mediaTypeUnique, collection: null } },
		};
		return this.createScaffold(args);
	}

	public getCollectionAlias() {
		return UMB_MEDIA_COLLECTION_ALIAS;
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @deprecated Use `getContentTypeUnique` instead.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbMediaWorkspaceContext
	 */
	getContentTypeId(): string | undefined {
		return this.getContentTypeUnique();
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbMediaWorkspaceContext
	 */
	getContentTypeUnique(): string | undefined {
		return this.getData()?.mediaType.unique;
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbMediaPropertyDatasetContext {
		return new UmbMediaPropertyDatasetContext(host, this, variantId);
	}
}

export { UmbMediaWorkspaceContext as api };
