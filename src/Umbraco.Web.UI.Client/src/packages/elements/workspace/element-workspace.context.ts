import { UmbElementPropertyDatasetContext } from '../property-dataset-context/element-property-dataset-context.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN, UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/constants.js';
import type { UmbElementDetailRepository } from '../repository/index.js';
import type { UmbElementDetailModel, UmbElementVariantModel } from '../types.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from './constants.js';
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
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD } from '@umbraco-cms/backoffice/document';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

type ContentModel = UmbElementDetailModel;
type ContentTypeModel = UmbDocumentTypeDetailModel;

export class UmbElementWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbElementDetailRepository,
		ContentTypeModel,
		UmbElementVariantModel
	>
	implements UmbContentWorkspaceContext<ContentModel, UmbDocumentTypeDetailModel, UmbElementVariantModel>
{
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			workspaceAlias: UMB_ELEMENT_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			contentTypePropertyName: 'documentType',
			ignoreValidationResultOnSubmit: true,
		});

		this.observe(
			this.contentTypeUnique,
			(unique) => {
				if (unique) {
					this.structure.loadType(unique);
				}
			},
			null,
		);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./element-workspace-editor.element.js'),
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
				path: UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./element-workspace-editor.element.js'),
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

	/**
	 * Gets the unique identifier of the content type.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbElementWorkspaceContext
	 */
	getContentTypeUnique(): string | undefined {
		return this.getData()?.documentType.unique;
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbElementPropertyDatasetContext {
		return new UmbElementPropertyDatasetContext(host, this, variantId);
	}
}

export { UmbElementWorkspaceContext as api };
