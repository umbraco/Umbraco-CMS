import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDatasetContext } from '../property-dataset-context/document-property-dataset.context.js';
import type { UmbDocumentDetailRepository } from '../repository/index.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import type { UmbDocumentDetailModel, UmbDocumentVariantModel } from '../types.js';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_DOCUMENT_COLLECTION_ALIAS,
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_SAVE_MODAL,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '../constants.js';
import { UmbDocumentPreviewRepository } from '../repository/preview/index.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT, UmbDocumentPublishingRepository } from '../publishing/index.js';
import { UmbDocumentValidationRepository } from '../repository/validation/index.js';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD, UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	type UmbPublishableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';
import {
	UmbContentDetailWorkspaceContextBase,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

type ContentModel = UmbDocumentDetailModel;
type ContentTypeModel = UmbDocumentTypeDetailModel;

export class UmbDocumentWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbDocumentDetailRepository,
		ContentTypeModel,
		UmbDocumentVariantModel
	>
	implements
		UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbDocumentVariantModel>,
		UmbPublishableWorkspaceContext,
		UmbContentCollectionWorkspaceContext<UmbDocumentTypeDetailModel>
{
	/**
	 * The publishing repository for the document workspace.
	 * @deprecated Will be removed in v17. Use the methods on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.
	 * @memberof UmbDocumentWorkspaceContext
	 */
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);
	readonly contentTypeHasCollection = this._data.createObservablePartOfCurrent(
		(data) => !!data?.documentType.collection,
	);
	readonly urls = this._data.createObservablePartOfCurrent((data) => data?.urls || []);
	readonly templateId = this._data.createObservablePartOfCurrent((data) => data?.template?.unique || null);

	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#publishingContext?: typeof UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			workspaceAlias: UMB_DOCUMENT_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentValidationRepository: UmbDocumentValidationRepository,
			skipValidationOnSubmit: true,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			saveModalToken: UMB_DOCUMENT_SAVE_MODAL,
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

		// TODO: Remove this in v17 as we have moved the publishing methods to the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT.
		this.consumeContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT, (context) => {
			this.#publishingContext = context;
		});

		this.routes.setRoutes([
			{
				path: UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique: string | null =
						info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					const blueprintUnique = info.match.params.blueprintUnique;

					await this.create(
						{ entityType: parentEntityType, unique: parentUnique },
						documentTypeUnique,
						blueprintUnique,
					);
					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-workspace-editor.element.js'),
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
				component: () => import('./document-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		if (response.data) {
			this.#isTrashedContext.setIsTrashed(response.data.isTrashed);
		}

		return response;
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string, blueprintUnique?: string) {
		if (blueprintUnique) {
			const blueprintRepository = new UmbDocumentBlueprintDetailRepository(this);
			const { data } = await blueprintRepository.requestByUnique(blueprintUnique);

			return this.createScaffold({
				parent,
				preset: {
					documentType: data?.documentType,
					values: data?.values,
					variants: data?.variants as Array<UmbDocumentVariantModel>,
				},
			});
		}

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

	/**
	 * Set the template
	 * @param {string} templateUnique The unique identifier of the template.
	 * @memberof UmbDocumentWorkspaceContext
	 */
	setTemplate(templateUnique: string) {
		this._data.updateCurrent({ template: { unique: templateUnique } });
	}

	/**
	 * Request a submit of the workspace, in the case of Document Workspaces the validation does not need to be valid for this to be submitted.
	 * @returns {Promise<void>} a promise which resolves once it has been completed.
	 */
	public override requestSubmit() {
		return this._handleSubmit();
	}

	// Because we do not make validation prevent submission this also submits the workspace. [NL]
	public override invalidSubmit() {
		return this._handleSubmit();
	}

	public async saveAndPreview(): Promise<void> {
		return this.#handleSaveAndPreview();
	}

	async #handleSaveAndPreview() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let culture = UMB_INVARIANT_CULTURE;

		// Save document (the active variant) before previewing.
		const { selected } = await this._determineVariantOptions();
		if (selected.length > 0) {
			culture = selected[0];
			const variantIds = [UmbVariantId.FromString(culture)];
			const saveData = await this._data.constructData(variantIds);
			await this.runMandatoryValidationForSaveData(saveData);
			await this.performCreateOrUpdate(variantIds, saveData);
		}

		// Tell the server that we're entering preview mode.
		await new UmbDocumentPreviewRepository(this).enter();

		const appContext = await this.getContext(UMB_APP_CONTEXT);

		const previewUrl = new URL(appContext.getBackofficePath() + '/preview', appContext.getServerUrl());
		previewUrl.searchParams.set('id', unique);

		if (culture && culture !== UMB_INVARIANT_CULTURE) {
			previewUrl.searchParams.set('culture', culture);
		}

		const previewWindow = window.open(previewUrl.toString(), `umbpreview-${unique}`);
		previewWindow?.focus();
	}

	public async publish() {
		new UmbDeprecation({
			deprecated: 'The Publish method on the UMB_DOCUMENT_WORKSPACE_CONTEXT is deprecated.',
			removeInVersion: '17',
			solution: 'Use the Publish method on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.',
		}).warn();
		if (!this.#publishingContext) throw new Error('Publishing context is missing');
		this.#publishingContext.publish();
	}

	/**
	 * Save the document and publish it.
	 * @deprecated Will be removed in v17. Use the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.
	 */
	public async saveAndPublish(): Promise<void> {
		new UmbDeprecation({
			deprecated: 'The SaveAndPublish method on the UMB_DOCUMENT_WORKSPACE_CONTEXT is deprecated.',
			removeInVersion: '17',
			solution: 'Use the SaveAndPublish method on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.',
		}).warn();
		if (!this.#publishingContext) throw new Error('Publishing context is missing');
		this.#publishingContext.saveAndPublish();
	}

	/**
	 * Schedule the document to be published at a later date.
	 * @deprecated Will be removed in v17. Use the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.
	 */
	public async schedule() {
		new UmbDeprecation({
			deprecated: 'The Schedule method on the UMB_DOCUMENT_WORKSPACE_CONTEXT is deprecated.',
			removeInVersion: '17',
			solution: 'Use the Schedule method on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.',
		}).warn();
		if (!this.#publishingContext) throw new Error('Publishing context is missing');
		this.#publishingContext.schedule();
	}

	/**
	 * Unpublish the document.
	 * @deprecated Will be removed in v17. Use the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.
	 */
	public async unpublish() {
		new UmbDeprecation({
			deprecated: 'The Unpublish method on the UMB_DOCUMENT_WORKSPACE_CONTEXT is deprecated.',
			removeInVersion: '17',
			solution: 'Use the Unpublish method on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.',
		}).warn();
		if (!this.#publishingContext) throw new Error('Publishing context is missing');
		this.#publishingContext.unpublish();
	}

	/**
	 * Publish the document and all its descendants.
	 * @deprecated Will be removed in v17. Use the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.
	 */
	public async publishWithDescendants() {
		new UmbDeprecation({
			deprecated: 'The PublishWithDescendants method on the UMB_DOCUMENT_WORKSPACE_CONTEXT is deprecated.',
			removeInVersion: '17',
			solution: 'Use the PublishWithDescendants method on the UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT instead.',
		}).warn();
		if (!this.#publishingContext) throw new Error('Publishing context is missing');
		this.#publishingContext.publishWithDescendants();
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentPropertyDatasetContext {
		return new UmbDocumentPropertyDatasetContext(host, this, variantId);
	}
}

export default UmbDocumentWorkspaceContext;
