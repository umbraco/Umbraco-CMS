import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDatasetContext } from '../property-dataset-context/document-property-dataset.context.js';
import type { UmbDocumentDetailRepository } from '../repository/index.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import type { UmbDocumentVariantPublishModel, UmbDocumentDetailModel, UmbDocumentVariantModel } from '../types.js';
import {
	UMB_DOCUMENT_COLLECTION_ALIAS,
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_PUBLISH_MODAL,
	UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL,
	UMB_DOCUMENT_SAVE_MODAL,
	UMB_DOCUMENT_SCHEDULE_MODAL,
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '../constants.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UmbDocumentValidationRepository } from '../repository/validation/document-validation.repository.js';
import { UmbDocumentPreviewRepository } from '../repository/preview/index.js';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD, UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	type UmbPublishableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbServerModelValidatorContext } from '@umbraco-cms/backoffice/validation';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import {
	UmbContentDetailWorkspaceContextBase,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

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
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	#serverValidation = new UmbServerModelValidatorContext(this);
	#validationRepository?: UmbDocumentValidationRepository;

	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);
	readonly contentTypeHasCollection = this._data.createObservablePartOfCurrent(
		(data) => !!data?.documentType.collection,
	);
	readonly urls = this._data.createObservablePartOfCurrent((data) => data?.urls || []);
	readonly templateId = this._data.createObservablePartOfCurrent((data) => data?.template?.unique || null);

	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			workspaceAlias: UMB_DOCUMENT_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			saveModalToken: UMB_DOCUMENT_SAVE_MODAL,
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

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
			await this._runMandatoryValidationForSaveData(saveData);
			await this._performCreateOrUpdate(variantIds, saveData);
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

	async #handleSaveAndPublish() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let variantIds: Array<UmbVariantId> = [];

		const { options, selected } = await this._determineVariantOptions();

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to publish.
			const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const result = await modalManagerContext
				.open(this, UMB_DOCUMENT_PUBLISH_MODAL, {
					data: {
						options,
						pickableFilter: this._readOnlyLanguageVariantsFilter,
					},
					value: { selection: selected },
				})
				.onSubmit()
				.catch(() => undefined);

			if (!result?.selection.length || !unique) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = await this._data.constructData(variantIds);
		await this._runMandatoryValidationForSaveData(saveData);

		// Create the validation repository if it does not exist. (we first create this here when we need it) [NL]
		this.#validationRepository ??= new UmbDocumentValidationRepository(this);

		// We ask the server first to get a concatenated set of validation messages. So we see both front-end and back-end validation messages [NL]
		if (this.getIsNew()) {
			const parent = this.getParent();
			if (!parent) throw new Error('Parent is not set');
			this.#serverValidation.askServerForValidation(
				saveData,
				this.#validationRepository.validateCreate(saveData, parent.unique),
			);
		} else {
			this.#serverValidation.askServerForValidation(
				saveData,
				this.#validationRepository.validateSave(saveData, variantIds),
			);
		}

		// TODO: Only validate the specified selection.. [NL]
		return this.validateAndSubmit(
			async () => {
				return this.#performSaveAndPublish(variantIds, saveData);
			},
			async () => {
				// If data of the selection is not valid Then just save:
				await this._performCreateOrUpdate(variantIds, saveData);
				// Notifying that the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				// TODO: Get rid of the save notification.
				// TODO: Translate this message [NL]
				notificationContext.peek('danger', {
					data: { message: 'Document was not published, but we saved it for you.' },
				});
				// Reject even thought the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				return await Promise.reject();
			},
		);
	}

	async #performSaveAndPublish(variantIds: Array<UmbVariantId>, saveData: UmbDocumentDetailModel): Promise<void> {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		await this._performCreateOrUpdate(variantIds, saveData);

		const { error } = await this.publishingRepository.publish(
			unique,
			variantIds.map((variantId) => ({ variantId })),
		);

		if (!error) {
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			eventContext.dispatchEvent(event);

			this._closeModal();
		}
	}

	public async publish() {
		throw new Error('Method not implemented.');
	}

	public async saveAndPreview(): Promise<void> {
		return this.#handleSaveAndPreview();
	}

	public async saveAndPublish(): Promise<void> {
		return this.#handleSaveAndPublish();
	}

	public async schedule() {
		const { options, selected } = await this._determineVariantOptions();

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_SCHEDULE_MODAL, {
				data: {
					options,
					pickableFilter: this._readOnlyLanguageVariantsFilter,
				},
				value: { selection: selected.map((unique) => ({ unique, schedule: {} })) },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to the correct format for the API (UmbDocumentVariantPublishModel)
		const variants =
			result?.selection.map<UmbDocumentVariantPublishModel>((x) => ({
				variantId: UmbVariantId.FromString(x.unique),
				schedule: x.schedule,
			})) ?? [];

		if (!variants.length) return;

		// TODO: Validate content & Save changes for the selected variants — This was how it worked in v.13 [NL]

		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		await this.publishingRepository.publish(unique, variants);
	}

	public async unpublish() {
		const unique = this.getUnique();
		const entityType = this.getEntityType();
		if (!unique) throw new Error('Unique is missing');
		if (!entityType) throw new Error('Entity type is missing');

		// TODO: remove meta
		new UmbUnpublishDocumentEntityAction(this, { unique, entityType, meta: {} as never }).execute();
	}

	public async publishWithDescendants() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		const entityType = this.getEntityType();
		if (!entityType) throw new Error('Entity type is missing');

		const { options, selected } = await this._determineVariantOptions();

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, {
				data: {
					options,
					pickableFilter: this._readOnlyLanguageVariantsFilter,
				},
				value: { selection: selected },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to variantIds
		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (!variantIds.length) return;

		const { error } = await this.publishingRepository.publishWithDescendants(
			unique,
			variantIds,
			result.includeUnpublishedDescendants ?? false,
		);

		if (!error) {
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

			// request reload of this entity
			const structureEvent = new UmbRequestReloadStructureForEntityEvent({
				entityType,
				unique,
			});

			// request reload of the children
			const childrenEvent = new UmbRequestReloadChildrenOfEntityEvent({
				entityType,
				unique,
			});

			eventContext.dispatchEvent(structureEvent);
			eventContext.dispatchEvent(childrenEvent);
		}
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentPropertyDatasetContext {
		return new UmbDocumentPropertyDatasetContext(host, this, variantId);
	}
}

export default UmbDocumentWorkspaceContext;
