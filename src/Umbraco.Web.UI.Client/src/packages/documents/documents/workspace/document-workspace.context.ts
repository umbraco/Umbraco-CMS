import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDatasetContext } from '../property-dataset-context/document-property-dataset.context.js';
import type { UmbDocumentDetailRepository } from '../repository/index.js';
import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UmbDocumentSegmentRepository } from '../repository/index.js';
import type { UmbDocumentDetailModel, UmbDocumentVariantModel } from '../types.js';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_DOCUMENT_COLLECTION_ALIAS,
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_SAVE_MODAL,
	UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../constants.js';
import { UmbDocumentPreviewRepository } from '../repository/preview/index.js';
import { UmbDocumentValidationRepository } from '../repository/validation/index.js';
import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../index.js';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD, UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';
import { UmbContentDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/content';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
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
	implements UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbDocumentVariantModel>
{
	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);

	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);

	/*
	 * @deprecated Use `collection.hasCollection` instead, will be removed in v.18
	 */
	readonly contentTypeHasCollection = this._data.createObservablePartOfCurrent(
		(data) => !!data?.documentType.collection,
	);

	readonly contentTypeIcon = this._data.createObservablePartOfCurrent((data) => data?.documentType.icon || null);

	readonly templateId = this._data.createObservablePartOfCurrent((data) => data?.template?.unique || null);

	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#documentSegmentRepository = new UmbDocumentSegmentRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			workspaceAlias: UMB_DOCUMENT_WORKSPACE_ALIAS,
			collectionAlias: UMB_DOCUMENT_COLLECTION_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentValidationRepository: UmbDocumentValidationRepository,
			skipValidationOnSubmit: false,
			ignoreValidationResultOnSubmit: true,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			contentTypePropertyName: 'documentType',
			saveModalToken: UMB_DOCUMENT_SAVE_MODAL,
		});

		this.consumeContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT, async (context) => {
			const config = await context?.getDocumentConfiguration();
			const allowSegmentCreation = config?.allowNonExistingSegmentsCreation ?? false;
			const allowEditInvariantFromNonDefault = config?.allowEditInvariantFromNonDefault ?? true;

			// Deprecation warning for allowNonExistingSegmentsCreation (default from server is true, so we warn on false)
			if (!allowSegmentCreation) {
				new UmbDeprecation({
					deprecated: 'The "AllowNonExistingSegmentsCreation" setting is deprecated.',
					removeInVersion: '19.0.0',
					solution: 'This functionality will be moved to a client-side extension.',
				}).warn();
			}

			this._variantOptionsFilter = (variantOption) => {
				const isNotCreatedSegmentVariant = variantOption.segment && !variantOption.variant;

				// Do not allow creating a segment variant
				if (!allowSegmentCreation && isNotCreatedSegmentVariant) {
					return false;
				}

				return true;
			};

			if (allowEditInvariantFromNonDefault === false) {
				this.#preventEditInvariantFromNonDefault();
			}
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

		this.observe(
			this.isNew,
			(isNew) => {
				if (isNew === undefined) return;
				if (isNew) {
					this.#enforceUserPermission(
						UMB_USER_PERMISSION_DOCUMENT_CREATE,
						'You do not have permission to create documents.',
					);
				} else {
					this.#enforceUserPermission(
						UMB_USER_PERMISSION_DOCUMENT_UPDATE,
						'You do not have permission to update documents.',
					);
				}
			},
			null,
		);

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
				setup: async (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					await this.load(unique);
				},
			},
		]);
	}

	#enforceUserPermission(verb: string, message: string) {
		// We set the initial permission state to false because the condition is false by default and only execute the callback if it changes.
		this.#handleUserPermissionChange(verb, false, message);

		createExtensionApiByAlias(this, UMB_DOCUMENT_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [verb],
				},
				onChange: (permitted: boolean) => {
					this.#handleUserPermissionChange(verb, permitted, message);
				},
			},
		]);
	}

	override resetState(): void {
		super.resetState();
		this.#isTrashedContext.setIsTrashed(false);
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		if (response?.data) {
			this.#isTrashedContext.setIsTrashed(response.data.isTrashed);
		}

		return response;
	}

	protected override async loadSegments(): Promise<void> {
		this.observe(
			this.unique,
			async (unique) => {
				if (!unique) {
					this._segments.setValue([]);
					return;
				}
				const { data } = await this.#documentSegmentRepository.getDocumentByIdSegmentOptions(unique, {
					skip: 0,
					take: 9999,
				});
				this._segments.setValue(data?.items ?? []);
			},
			'_loadSegmentsUnique',
		);
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string, blueprintUnique?: string) {
		if (blueprintUnique) {
			const blueprintRepository = new UmbDocumentBlueprintDetailRepository(this);
			const { data } = await blueprintRepository.scaffoldByUnique(blueprintUnique);

			if (!data) throw new Error('Blueprint data is missing');

			return this.createScaffold({
				parent,
				preset: {
					documentType: data.documentType,
					values: data.values,
					variants: data.variants as Array<UmbDocumentVariantModel>,
				},
			});
		}

		return this.createScaffold({
			parent,
			preset: {
				documentType: {
					unique: documentTypeUnique,
				},
			},
		});
	}

	/** @deprecated will be removed in v.18 */
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

	protected override async _handleSave() {
		const elementStyle = (this.getHostElement() as HTMLElement).style;
		elementStyle.setProperty('--uui-color-invalid', 'var(--uui-color-warning)');
		elementStyle.setProperty('--uui-color-invalid-emphasis', 'var(--uui-color-warning-emphasis)');
		elementStyle.setProperty('--uui-color-invalid-standalone', 'var(--uui-color-warning-standalone)');
		elementStyle.setProperty('--uui-color-invalid-contrast', 'var(--uui-color-warning-contrast)');
		await super._handleSave();
	}

	public async saveAndPreview(urlProviderAlias?: string): Promise<void> {
		return await this.#handleSaveAndPreview(urlProviderAlias ?? 'umbDocumentUrlProvider');
	}

	async #handleSaveAndPreview(urlProviderAlias: string) {
		if (!urlProviderAlias) throw new Error('Url provider alias is missing');

		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let firstVariantId = UmbVariantId.CreateInvariant();

		// Save document (the active variant) before previewing.
		const { selected } = await this._determineVariantOptions();
		if (selected.length > 0) {
			firstVariantId = UmbVariantId.FromString(selected[0]);
			const variantIds = [firstVariantId];
			const saveData = await this._data.constructData(variantIds);
			await this.runMandatoryValidationForSaveData(saveData);
			await this.performCreateOrUpdate(variantIds, saveData);
		}

		// Get the preview URL from the server.
		const previewRepository = new UmbDocumentPreviewRepository(this);
		const previewUrlData = await previewRepository.getPreviewUrl(
			unique,
			urlProviderAlias,
			firstVariantId.culture ?? undefined,
			firstVariantId.segment ?? undefined,
		);

		if (previewUrlData.url) {
			const previewWindow = window.open(previewUrlData.url, `umbpreview-${unique}`);
			previewWindow?.focus();
			return;
		}

		if (previewUrlData.message) {
			umbPeekError(this._host, { color: 'danger', headline: 'Preview error', message: previewUrlData.message });
			throw new Error(previewUrlData.message);
		}
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentPropertyDatasetContext {
		return new UmbDocumentPropertyDatasetContext(host, this, variantId);
	}

	async #handleUserPermissionChange(identifier: string, permitted: boolean, message: string) {
		if (permitted) {
			this.readOnlyGuard?.removeRule(identifier);
			return;
		}

		this.readOnlyGuard?.addRule({
			unique: identifier,
			message,
			/* This guard is a bit backwards. The rule is permitted to be read-only.
			If the user does not have permission, we set it to true = permitted to be read-only. */
			permitted: true,
		});
	}

	#preventEditInvariantFromNonDefault() {
		this.observe(
			observeMultiple([this.structure.contentTypeProperties, this.variantOptions]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				variantOptions.forEach((variantOption) => {
					// Do not add a rule for the default language. It is always permitted to edit.
					if (variantOption.language.isDefault) return;

					const datasetVariantId = UmbVariantId.CreateFromPartial(variantOption);
					const invariantVariantId = UmbVariantId.CreateInvariant();
					const unique = `UMB_PREVENT_EDIT_INVARIANT_FROM_NON_DEFAULT_DATASET=${datasetVariantId.toString()}_PROPERTY_${invariantVariantId.toString()}`;

					const rule: UmbVariantPropertyGuardRule = {
						unique,
						message: 'Shared properties can only be edited in the default language',
						variantId: invariantVariantId,
						datasetVariantId,
						permitted: false,
					};

					this.propertyWriteGuard.addRule(rule);
				});
			},
		);
	}
}

export default UmbDocumentWorkspaceContext;
