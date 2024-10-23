import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDatasetContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UmbDocumentDetailRepository } from '../repository/index.js';
import type {
	UmbDocumentVariantPublishModel,
	UmbDocumentDetailModel,
	UmbDocumentValueModel,
	UmbDocumentVariantModel,
	UmbDocumentVariantOptionModel,
} from '../types.js';
import {
	UMB_DOCUMENT_PUBLISH_MODAL,
	UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL,
	UMB_DOCUMENT_SCHEDULE_MODAL,
	UMB_DOCUMENT_SAVE_MODAL,
} from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UmbDocumentValidationRepository } from '../repository/validation/document-validation.repository.js';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '../paths.js';
import { UMB_DOCUMENTS_SECTION_PATH } from '../../section/paths.js';
import { UmbDocumentPreviewRepository } from '../repository/preview/index.js';
import { sortVariants } from '../utils.js';
import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD, UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import { UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	type UmbPublishableWorkspaceContext,
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceSplitViewManager,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	mergeObservables,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { type Observable, firstValueFrom, map } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import {
	UMB_VALIDATION_CONTEXT,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbDataPathVariantQuery,
	UmbServerModelValidatorContext,
	UmbValidationContext,
	UmbVariantValuesValidationPathTranslator,
	UmbVariantsValidationPathTranslator,
} from '@umbraco-cms/backoffice/validation';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import {
	UmbContentDetailWorkspaceBase,
	UmbContentWorkspaceDataManager,
	UmbMergeContentVariantDataController,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

type EntityModel = UmbDocumentDetailModel;
type EntityTypeModel = UmbDocumentTypeDetailModel;

export class UmbDocumentWorkspaceContext
	extends UmbContentDetailWorkspaceBase<EntityModel>
	implements
		UmbContentWorkspaceContext<EntityModel, EntityTypeModel, UmbDocumentVariantModel>,
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

					this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique, blueprintUnique);
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
					this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

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

	override resetState() {
		super.resetState();
		this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		if (response.data) {
			this.#isTrashedContext.setIsTrashed(response.data.isTrashed);
		}

		return response;
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string, blueprintUnique?: string) {
		this.resetState();
		this.#parent.setValue(parent);

		if (blueprintUnique) {
			const blueprintRepository = new UmbDocumentBlueprintDetailRepository(this);
			const { data } = await blueprintRepository.requestByUnique(blueprintUnique);

			this.#getDataPromise = this.repository.createScaffold({
				documentType: data?.documentType,
				values: data?.values,
				variants: data?.variants as Array<UmbDocumentVariantModel>,
			});
		} else {
			this.#getDataPromise = this.repository.createScaffold({
				documentType: {
					unique: documentTypeUnique,
					collection: null,
				},
			});
		}

		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.#entityContext.setEntityType(UMB_DOCUMENT_ENTITY_TYPE);
		this.#entityContext.setUnique(data.unique);
		this.#isTrashedContext.setIsTrashed(data.isTrashed);
		this.setIsNew(true);
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(data);
		return data;
	}

	getCollectionAlias() {
		return UMB_DOCUMENT_COLLECTION_ALIAS;
	}

	getContentTypeId() {
		return this.getData()?.documentType.unique;
	}

	setTemplate(templateUnique: string) {
		this.#data.updateCurrent({ template: { unique: templateUnique } });
	}

	async #performSaveOrCreate(variantIds: Array<UmbVariantId>, saveData: UmbDocumentDetailModel): Promise<void> {
		if (this.getIsNew()) {
			// Create:
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			const { data, error } = await this.repository.create(saveData, parent.unique);
			if (!data || error) {
				throw new Error('Error creating document');
			}

			this.setIsNew(false);
			this.#data.setPersisted(data);
			// TODO: Only update the variants that was chosen to be saved:
			const currentData = this.#data.getCurrent();

			const variantIdsIncludingInvariant = [...variantIds, UmbVariantId.CreateInvariant()];

			const newCurrentData = await new UmbMergeContentVariantDataController(this).process(
				currentData,
				data,
				variantIds,
				variantIdsIncludingInvariant,
			);
			this.#data.setCurrent(newCurrentData);

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
			// TODO: Only update the variants that was chosen to be saved:
			const currentData = this.#data.getCurrent();

			const variantIdsIncludingInvariant = [...variantIds, UmbVariantId.CreateInvariant()];

			const newCurrentData = await new UmbMergeContentVariantDataController(this).process(
				currentData,
				data,
				variantIds,
				variantIdsIncludingInvariant,
			);
			this.#data.setCurrent(newCurrentData);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				entityType: this.getEntityType(),
				unique: this.getUnique()!,
			});

			eventContext.dispatchEvent(event);
		}
	}

	#readOnlyLanguageVariantsFilter = (option: UmbDocumentVariantOptionModel) => {
		const readOnlyCultures = this.readOnlyState.getStates().map((s) => s.variantId.culture);
		return readOnlyCultures.includes(option.culture) === false;
	};

	async #handleSaveAndPreview() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let culture = UMB_INVARIANT_CULTURE;

		// Save document (the active variant) before previewing.
		const { selected } = await this.#determineVariantOptions();
		if (selected.length > 0) {
			culture = selected[0];
			const variantIds = [UmbVariantId.FromString(culture)];
			const saveData = await this.#data.constructData(variantIds);
			await this.#runMandatoryValidationForSaveData(saveData);
			await this.#performSaveOrCreate(variantIds, saveData);
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

		const { options, selected } = await this.#determineVariantOptions();

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
						pickableFilter: this.#readOnlyLanguageVariantsFilter,
					},
					value: { selection: selected },
				})
				.onSubmit()
				.catch(() => undefined);

			if (!result?.selection.length || !unique) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = await this.#data.constructData(variantIds);
		await this.#runMandatoryValidationForSaveData(saveData);

		// Create the validation repository if it does not exist. (we first create this here when we need it) [NL]
		this.#validationRepository ??= new UmbDocumentValidationRepository(this);

		// We ask the server first to get a concatenated set of validation messages. So we see both front-end and back-end validation messages [NL]
		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
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
				await this.#performSaveOrCreate(variantIds, saveData);
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

	async #runMandatoryValidationForSaveData(saveData: UmbDocumentDetailModel) {
		// Check that the data is valid before we save it.
		// Check variants have a name:
		const variantsWithoutAName = saveData.variants.filter((x) => !x.name);
		if (variantsWithoutAName.length > 0) {
			const validationContext = await this.getContext(UMB_VALIDATION_CONTEXT);
			variantsWithoutAName.forEach((variant) => {
				validationContext.messages.addMessage(
					'client',
					`$.variants[${UmbDataPathVariantQuery(variant)}].name`,
					UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
				);
			});
			throw new Error('All variants must have a name');
		}
	}

	async #performSaveAndPublish(variantIds: Array<UmbVariantId>, saveData: UmbDocumentDetailModel): Promise<void> {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		await this.#performSaveOrCreate(variantIds, saveData);

		await this.publishingRepository.publish(
			unique,
			variantIds.map((variantId) => ({ variantId })),
		);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.getUnique()!,
			entityType: this.getEntityType(),
		});

		eventContext.dispatchEvent(event);
	}

	async #handleSave() {
		const { options, selected } = await this.#determineVariantOptions();

		let variantIds: Array<UmbVariantId> = [];

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to save.
			const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const result = await modalManagerContext
				.open(this, UMB_DOCUMENT_SAVE_MODAL, {
					data: {
						options,
						pickableFilter: this.#readOnlyLanguageVariantsFilter,
					},
					value: { selection: selected },
				})
				.onSubmit()
				.catch(() => undefined);

			if (!result?.selection.length) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = await this.#data.constructData(variantIds);
		await this.#runMandatoryValidationForSaveData(saveData);
		return await this.#performSaveOrCreate(variantIds, saveData);
	}

	public override requestSubmit() {
		return this.#handleSave();
	}

	public submit() {
		return this.#handleSave();
	}

	public override invalidSubmit() {
		return this.#handleSave();
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
					pickableFilter: this.#readOnlyLanguageVariantsFilter,
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
		const { options, selected } = await this._determineVariantOptions();

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, {
				data: {
					options,
					pickableFilter: this.#readOnlyLanguageVariantsFilter,
				},
				value: { selection: selected },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to variantIds
		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (!variantIds.length) return;

		// TODO: Validate content & Save changes for the selected variants — This was how it worked in v.13 [NL]

		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		await this.publishingRepository.publishWithDescendants(
			unique,
			variantIds,
			result.includeUnpublishedDescendants ?? false,
		);
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentPropertyDatasetContext {
		return new UmbDocumentPropertyDatasetContext(host, this, variantId);
	}
}

export default UmbDocumentWorkspaceContext;
