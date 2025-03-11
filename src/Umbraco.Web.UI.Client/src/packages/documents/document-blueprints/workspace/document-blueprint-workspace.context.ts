import { UmbDocumentBlueprintPropertyDatasetContext } from '../property-dataset-context/document-blueprint-property-dataset-context.js';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentBlueprintDetailRepository } from '../repository/index.js';
import { UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS } from '../constants.js';
import type { UmbDocumentBlueprintDetailModel, UmbDocumentBlueprintVariantModel } from '../types.js';
import { UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from './constants.js';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import {
	type UmbDocumentTypeDetailModel,
	UmbDocumentTypeDetailRepository,
} from '@umbraco-cms/backoffice/document-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentDetailWorkspaceContextBase, type UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import {
	UMB_DOCUMENT_COLLECTION_ALIAS,
	UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '@umbraco-cms/backoffice/document';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

type ContentModel = UmbDocumentBlueprintDetailModel;
type ContentTypeModel = UmbDocumentTypeDetailModel;

export class UmbDocumentBlueprintWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbDocumentBlueprintDetailRepository,
		ContentTypeModel,
		UmbDocumentBlueprintVariantModel
	>
	implements UmbContentWorkspaceContext<ContentModel, UmbDocumentTypeDetailModel, UmbDocumentBlueprintVariantModel>
{
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			workspaceAlias: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			contentTypePropertyName: 'documentType',
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					await this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string) {
		return this.createScaffold({
			parent,
			preset: {
				documentType: {
					unique: documentTypeUnique,
					collection: null,
				},
			},
		});
	}

	getCollectionAlias() {
		return UMB_DOCUMENT_COLLECTION_ALIAS;
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @deprecated Use `getContentTypeUnique` instead.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbDocumentWorkspaceContext
	 */
	getContentTypeId(): string | undefined {
		return this.getContentTypeUnique();
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbDocumentWorkspaceContext
	 */
	getContentTypeUnique(): string | undefined {
		return this.getData()?.documentType.unique;
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentBlueprintPropertyDatasetContext {
		return new UmbDocumentBlueprintPropertyDatasetContext(host, this, variantId);
	}
}

export { UmbDocumentBlueprintWorkspaceContext as api };
