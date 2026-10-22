import type { UmbBlockWorkspaceOriginData } from '../workspace/index.js';
import type {
	UmbBlockDataModel,
	UmbBlockDataValueModel,
	UmbBlockExposeModel,
	UmbBlockExternalContentFailureReason,
	UmbBlockLayoutBaseModel,
} from '../types.js';
import { UmbBlockInsertedEvent } from '../events/block-inserted.event.js';
import { UMB_BLOCK_TRANSFER_TO_ELEMENT_LIBRARY_MODAL } from '../modals/transfer-to-element-library/transfer-to-element-library-modal.token.js';
import { UMB_BLOCK_CONTENT_DATA_PATH_PROPERTY_NAME, UMB_BLOCK_SETTINGS_DATA_PATH_PROPERTY_NAME } from '../constants.js';
import { umbBlockExposeSortCompare } from '../utils/block-expose-sort-compare.function.js';
import {
	umbGetBlockedTransferVariants,
	umbResolveBlockTransferCultures,
	type UmbBlockTransferLanguageAccess,
} from '../utils/block-transfer-variants.function.js';
import { getExternalContentFailureReason } from '../utils/get-external-content-failure-reason.function.js';
import { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
import {
	findExternalContentVariant,
	type UmbBlockManagerExternalContentVariantModel,
} from './block-manager-external-content.function.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	UmbBooleanState,
	UmbClassState,
	UmbStringState,
	type MappingFunction,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbContentTypeStructureManager, type UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { escapeHTML, UmbDeprecation, UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import {
	UmbPropertyValueFlatMapperController,
	UmbPropertyValuePresetVariantBuilderController,
	type UmbPropertyTypePresetModel,
	type UmbPropertyTypePresetModelTypeModel,
} from '@umbraco-cms/backoffice/property';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import {
	UmbElementDetailRepository,
	UmbElementPublishingRepository,
	UmbElementValidationRepository,
} from '@umbraco-cms/backoffice/element';
import type { UmbElementDetailModel } from '@umbraco-cms/backoffice/element';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal, umbInfoModal } from '@umbraco-cms/backoffice/modal';
import { UmbRepositoryDetailsManager } from '@umbraco-cms/backoffice/repository';
import { UmbApiError, apiErrorWasNotified } from '@umbraco-cms/backoffice/resources';
import {
	UMB_VALIDATION_CONTEXT,
	UmbValidationCleanUpByUniqueManager,
	type UmbValidationController,
} from '@umbraco-cms/backoffice/validation';

const UMB_CONTENT_VALIDATION_CLEAN_UP_ALIAS = Symbol();
const UMB_SETTINGS_VALIDATION_CLEAN_UP_ALIAS = Symbol();

export type UmbBlockDataObjectModel<LayoutEntryType extends UmbBlockLayoutBaseModel> = {
	layout: LayoutEntryType;
	content: UmbBlockDataModel;
	settings?: UmbBlockDataModel;
};

const isPublishValidationError = (error: UmbApiError) =>
	error.status === 400 &&
	(Object.keys(error.problemDetails.errors ?? {}).length > 0 || 'invalidProperties' in error.problemDetails);

export interface UmbBlockTransferToExternalContentResult {
	/** The unique of the created Element. */
	unique: string;
	/**
	 * Where the Element ended up: `published`, or `draft` when the block was not exposed in any culture.
	 * Otherwise why it could not be published: `validation-failed` when the Element has invalid values, or `publish-failed`.
	 */
	status: 'published' | 'draft' | 'validation-failed' | 'publish-failed';
	/** Whether the failed publish was already reported to the user, so no further notification is needed. */
	notified?: boolean;
}

/**
 * Maps a resolved Element to the block data shape used for local and external block content alike.
 * @param {UmbElementDetailModel} element - the resolved Element.
 * @returns {UmbBlockDataModel} the block data.
 */
function elementToBlockData(element: UmbElementDetailModel): UmbBlockDataModel {
	return {
		key: element.unique,
		contentTypeKey: element.documentType.unique,
		values: element.values.map(
			(v): UmbBlockDataValueModel => ({
				alias: v.alias,
				editorAlias: v.editorAlias,
				culture: v.culture,
				segment: v.segment,
				value: v.value,
			}),
		),
	};
}

/**
 * Maps a resolved Element's variant to the external content variant shape.
 * @param {UmbElementDetailModel['variants'][number]} variant - the Element's variant.
 * @returns {UmbBlockManagerExternalContentVariantModel} the external content variant.
 */
function elementVariantToExternalContentVariant(
	variant: UmbElementDetailModel['variants'][number],
): UmbBlockManagerExternalContentVariantModel {
	return {
		culture: variant.culture ?? null,
		state: variant.state ?? null,
		name: variant.name ?? null,
	};
}

export abstract class UmbBlockManagerContext<
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockOriginDataType extends UmbBlockWorkspaceOriginData = UmbBlockWorkspaceOriginData,
> extends UmbContextBase {
	get contentTypesLoaded() {
		return Promise.all(this.#contentTypeRequests);
	}
	#contentTypeRequests: Array<Promise<unknown>> = [];
	#contentTypeRepository = new UmbDocumentTypeDetailRepository(this);

	#propertyAlias = new UmbStringState(undefined);
	propertyAlias = this.#propertyAlias.asObservable();
	setPropertyAlias(propertyAlias: string | undefined) {
		this.#propertyAlias.setValue(propertyAlias);
	}

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	variantId = this.#variantId.asObservable();
	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
	}
	getVariantId(): UmbVariantId | undefined {
		return this.#variantId.getValue();
	}

	readonly #structures: Array<UmbContentTypeStructureManager> = [];

	#blockTypes = new UmbArrayState(<Array<BlockType>>[], (x) => x.contentElementTypeKey);
	public readonly blockTypes = this.#blockTypes.asObservable();

	protected _editorConfiguration = new UmbClassState<UmbPropertyEditorConfigCollection | undefined>(undefined);
	public readonly editorConfiguration = this._editorConfiguration.asObservable();

	protected _liveEditingMode = new UmbBooleanState(undefined);
	public readonly liveEditingMode = this._liveEditingMode.asObservable();

	protected _layouts = new UmbArrayState(<Array<BlockLayoutType>>[], (x) => x.key);
	public readonly layouts = this._layouts.asObservable();

	readonly #contents = new UmbArrayState<UmbBlockDataModel, string, undefined>(undefined, (x) => x.key);
	// TODO: Remove ?? [] fallback in v.19 (or v.20)
	public readonly contents = this.#contents.asObservablePart((x) => x ?? []);
	readonly #contentKeys = this.#contents.asObservablePart((x) => (x ? x.map((y) => y.key) : undefined));

	readonly #externalContentValues = new UmbArrayState(<Array<UmbBlockDataModel>>[], (x) => x.key);

	/**
	 * Combined observable of local block content and resolved external (library element) content.
	 * Use this alongside `contents` when you also need to react to library elements becoming available.
	 * The two arrays are concatenated without de-duplication — a key present in both (which should not
	 * normally happen) appears twice, with the local entry taking precedence in lookups like `getContentOf`.
	 */
	public readonly allContents = mergeObservables(
		[this.#contents.asObservable(), this.#externalContentValues.asObservable()],
		([local, external]) => [...(local ?? []), ...(external ?? [])],
	);
	readonly #externalContentVariants = new UmbArrayState(
		<Array<{ key: string; variants: Array<UmbBlockManagerExternalContentVariantModel> }>>[],
		(x) => x.key,
	);
	readonly #externalContentFailures = new UmbArrayState(
		<Array<{ key: string; reason: UmbBlockExternalContentFailureReason }>>[],
		(x) => x.key,
	);
	#unavailableExternalContentKeys = new Array<string>();
	#isProbingExternalContentFailures = false;
	#elementRepository = new UmbElementDetailRepository(this);
	#elementPublishingRepository = new UmbElementPublishingRepository(this);
	#elementValidationRepository = new UmbElementValidationRepository(this);
	readonly #localize = new UmbLocalizationController(this);
	#externalContentManager = new UmbRepositoryDetailsManager<UmbElementDetailModel>(this, this.#elementRepository);

	readonly #settings = new UmbArrayState<UmbBlockDataModel, string, undefined>(undefined, (x) => x.key);
	// TODO: Remove ?? [] fallback in v.19 (or v.20)
	public readonly settings = this.#settings.asObservablePart((x) => x ?? []);
	readonly #settingsKeys = this.#settings.asObservablePart((x) => (x ? x.map((y) => y.key) : undefined));

	// TODO: This is a bad seperation of concerns, this should be self initializing, not defined from the outside. [NL]
	public readonly readOnlyState = new UmbReadOnlyVariantGuardManager(this);

	readonly #exposes = new UmbArrayState(<Array<UmbBlockExposeModel>>[], (x) => x.contentKey + ':' + x.culture).sortBy(
		umbBlockExposeSortCompare,
	);
	public readonly exposes = this.#exposes.asObservable();

	setEditorConfiguration(configs: UmbPropertyEditorConfigCollection) {
		this._editorConfiguration.setValue(configs);
		if (this._liveEditingMode.getValue() === undefined) {
			this._liveEditingMode.setValue(configs.getValueByAlias<boolean>('useLiveEditing'));
		}
	}
	getEditorConfiguration(): UmbPropertyEditorConfigCollection | undefined {
		return this._editorConfiguration.getValue();
	}
	editorConfigurationPart(method: MappingFunction<UmbPropertyEditorConfigCollection | undefined, unknown>) {
		return this._editorConfiguration.asObservablePart(method);
	}

	setBlockTypes(blockTypes: Array<BlockType>) {
		this.#blockTypes.setValue(blockTypes);
	}
	getBlockTypes() {
		return this.#blockTypes.value;
	}

	/**
	 * Set all layouts.
	 * @param {Array<BlockLayoutType>} layouts - All layouts.
	 */
	setLayouts(layouts: Array<BlockLayoutType>) {
		// Backwards compatibility: ensure all layouts have a key (persisted data may not have one)
		this._layouts.setValue(layouts.map((layout) => (layout.key ? layout : { ...layout, key: layout.contentKey })));
	}

	/**
	 * Get all layouts.
	 * @returns {Array<BlockLayoutType>} - All layouts.
	 */
	getLayouts(): Array<BlockLayoutType> {
		return this._layouts.getValue();
	}

	/**
	 * Set all contents.
	 * @param {Array<UmbBlockDataModel>} contents - All contents.
	 */
	setContents(contents: Array<UmbBlockDataModel> | undefined) {
		this.#contents.setValue(contents);
	}

	// TODO: make return undefined when undefined in v.19
	/**
	 * Get all contents.
	 * @returns {Array<UmbBlockDataModel>} - All contents.
	 */
	getContents(): Array<UmbBlockDataModel> {
		return this.#contents.value ?? [];
	}

	/**
	 * Set all settings.
	 * @param {Array<UmbBlockDataModel>} settings - All settings.
	 */
	setSettings(settings: Array<UmbBlockDataModel> | undefined) {
		this.#settings.setValue(settings);
	}

	// TODO: make return undefined when undefined in v.19
	/**
	 * Get all settings.
	 * @returns {Array<UmbBlockDataModel>} - All settings.
	 */
	getSettings(): Array<UmbBlockDataModel> {
		return this.#settings.value ?? [];
	}

	/**
	 * Set all exposes.
	 * @param {Array<UmbBlockExposeModel>} exposes - All exposes.
	 */
	setExposes(exposes: Array<UmbBlockExposeModel>) {
		this.#exposes.setValue(exposes);
	}

	/**
	 * Get all exposes.
	 * @returns {Array<UmbBlockExposeModel>} - All exposes.
	 */
	getExposes(): Array<UmbBlockExposeModel> {
		return this.#exposes.value;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT);

		this.observe(
			this.blockTypes,
			(blockTypes) => {
				this.#ensureContentTypes(blockTypes);
			},
			null,
		);

		// Keep the details manager's uniques in sync with any layout marked as external, so it resolves
		// (and re-resolves on `UmbEntityUpdatedEvent`) the Elements those layouts reference, and drops any
		// that are no longer referenced.
		this.observe(
			this._layouts.asObservable(),
			(layouts) => {
				this.#externalContentManager.setUniques([...new Set(this._collectExternalContentKeys(layouts))]);
			},
			null,
		);

		// Reflect the resolved Elements into the external content states whenever they change.
		this.observe(
			this.#externalContentManager.entries,
			(entries) => {
				this.#externalContentValues.setValue(entries.map(elementToBlockData));
				this.#externalContentVariants.setValue(
					entries.map((entry) => ({
						key: entry.unique,
						variants: entry.variants.map(elementVariantToExternalContentVariant),
					})),
				);
			},
			null,
		);

		// The batched request only says that an element is missing, not why. Ask for each missing element on its own
		// to tell a deleted element from one the current user may not access.
		this.observe(
			this.#externalContentManager.statuses,
			(statuses) => {
				this.#externalContentFailures.remove(
					statuses.filter((status) => status.state.type === 'success').map((status) => status.unique),
				);
				this.#unavailableExternalContentKeys = statuses.filter((status) => status.state.type === 'error').map((status) => status.unique);
				this.#requestExternalContentFailures();
			},
			null,
		);
		// Clean up validation messages for Block content/settings that are no longer part of this Block
		// Editor's data. Deliberately does not skip the host: we want the Validation Context of the
		// Property Editor hosting this Block Manager, which is provided on this very same element. [NL]
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => this.#gotValidationContext(context));
	}

	#gotValidationContext(context: UmbValidationController | undefined) {
		// Only accept a Validation Context that is actually ours — a Block Manager can legitimately be
		// hosted without one, in which case no clean up happens (silent no-op, not an error). [NL]
		if (!context || context.getHostElement() !== this.getHostElement()) {
			this.removeUmbControllerByAlias(UMB_CONTENT_VALIDATION_CLEAN_UP_ALIAS);
			this.removeUmbControllerByAlias(UMB_SETTINGS_VALIDATION_CLEAN_UP_ALIAS);
			return;
		}

		new UmbValidationCleanUpByUniqueManager(
			this,
			context,
			`$.${UMB_BLOCK_CONTENT_DATA_PATH_PROPERTY_NAME}`,
			this.#contentKeys,
			(queryParams) => queryParams.key,
			UMB_CONTENT_VALIDATION_CLEAN_UP_ALIAS,
		);
		new UmbValidationCleanUpByUniqueManager(
			this,
			context,
			`$.${UMB_BLOCK_SETTINGS_DATA_PATH_PROPERTY_NAME}`,
			this.#settingsKeys,
			(queryParams) => queryParams.key,
			UMB_SETTINGS_VALIDATION_CLEAN_UP_ALIAS,
		);
	}

	#ensureContentTypes(blockTypes: Array<BlockType>) {
		// Collect all unique content type keys
		const allUniques = new Set<string>();
		blockTypes.forEach((x) => {
			allUniques.add(x.contentElementTypeKey);
			if (x.settingsElementTypeKey) {
				allUniques.add(x.settingsElementTypeKey);
			}
		});

		// Filter out already loaded types
		const uniquesToFetch = [...allUniques].filter(
			(unique) => !this.#structures.find((x) => x.getOwnerContentTypeUnique() === unique),
		);

		if (uniquesToFetch.length === 0) return;

		// IMPORTANT: Create and register all structure managers SYNCHRONOUSLY first
		// This ensures getStructure() can find them immediately, before any async operations
		const structuresToInit: Array<{ structure: UmbContentTypeStructureManager<UmbContentTypeModel>; unique: string }> =
			[];

		for (const unique of uniquesToFetch) {
			const structure = new UmbContentTypeStructureManager<UmbContentTypeModel>(this, this.#contentTypeRepository);
			// Set the unique synchronously so getStructure() can find it immediately
			structure.setOwnerContentTypeUnique(unique);
			this.#structures.push(structure);
			structuresToInit.push({ structure, unique });
		}

		// Now perform async initialization — push the promise synchronously so
		// contentTypesLoaded (Promise.all(#contentTypeRequests)) properly waits.
		this.#contentTypeRequests.push(this.#initializeStructures(structuresToInit));
	}

	async #initializeStructures(
		structuresToInit: Array<{ structure: UmbContentTypeStructureManager<UmbContentTypeModel>; unique: string }>,
	) {
		const uniques = structuresToInit.map((x) => x.unique);

		// Bulk fetch all content types if supported
		const contentTypesMap = new Map<string, UmbContentTypeModel>();

		if (this.#contentTypeRepository.requestByUniques) {
			const { error, data } = await this.#contentTypeRepository.requestByUniques(uniques);
			if (error || !data) {
				throw error?.message ?? 'Repository could not request Content Types by Uniques.';
			}
			if (data) {
				data.forEach((item) => contentTypesMap.set(item.unique, item));
			}
		}

		// Initialize each structure with pre-fetched data or fall back to individual loading
		const initPromises: Array<Promise<unknown>> = [];
		for (const { structure, unique } of structuresToInit) {
			const contentType = contentTypesMap.get(unique);

			if (contentType) {
				// Use pre-fetched data directly via setType to avoid re-fetching
				initPromises.push(structure.setType(contentType));
			} else {
				// Fallback to individual load if not in bulk response
				initPromises.push(structure.loadType(unique));
			}
		}
		await Promise.all(initPromises);
	}

	getStructure(unique: string) {
		return this.#structures.find((x) => x.getOwnerContentTypeUnique() === unique);
	}

	getContentTypeKeyOfContentKey(contentKey: string) {
		return this.getContentOf(contentKey)?.contentTypeKey;
	}
	contentTypeOf(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.ownerContentType;
	}
	contentTypeNameOf(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.ownerContentTypeObservablePart((x) => x?.name);
	}
	getContentTypeNameOf(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.getOwnerContentType()?.name;
	}
	getContentTypeHasProperties(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.getHasProperties();
	}
	blockTypeOf(contentTypeKey: string) {
		return this.#blockTypes.asObservablePart((source) =>
			source.find((x) => x.contentElementTypeKey === contentTypeKey),
		);
	}

	layoutOf(contentKey: string) {
		return this._layouts.asObservablePart((source) => source.find((x) => x.contentKey === contentKey));
	}
	contentOf(key: string) {
		return mergeObservables(
			[
				this.#contents.asObservablePart((source) => source?.find((x) => x.key === key)),
				this.#externalContentValues.asObservablePart((source) => source.find((x) => x.key === key)),
			],
			([localContent, externalContent]) => localContent ?? externalContent ?? undefined,
		);
	}

	/**
	 * Collects the content keys of every layout entry marked as external content. Override to also reach
	 * layout entries that are nested inside other entries.
	 * @param {Array<BlockLayoutType>} layouts - the top-level layout entries.
	 * @returns {Array<string>} the content keys of the external content layouts, one per reference.
	 */
	protected _collectExternalContentKeys(layouts: Array<BlockLayoutType>): Array<string> {
		return layouts.filter((layout) => layout.isExternalContent && layout.contentKey).map((layout) => layout.contentKey);
	}

	/**
	 * Returns an observable that emits true when a layout entry for the given contentKey, at any nesting
	 * level reached by {@link _collectExternalContentKeys}, has `isExternalContent` set (i.e., the block
	 * references external content).
	 * @param key
	 */
	isExternalContentOf(key: string) {
		return this._layouts.asObservablePart((layouts) => this._collectExternalContentKeys(layouts).includes(key));
	}

	/**
	 * Returns an observable of the external content variant state.
	 * Emits the state string (e.g., 'Published', 'Draft') or null if not resolved yet.
	 * @param key
	 */
	externalContentStateOf(key: string) {
		return mergeObservables(
			[this.#externalContentVariants.asObservablePart((source) => source.find((x) => x.key === key)), this.variantId],
			([entry, variantId]) => findExternalContentVariant(entry?.variants, variantId)?.state ?? null,
		);
	}

	/**
	 * Returns an observable of the external content's name (from the Element it references), matching the
	 * current variant. Emits null if the external content has not resolved yet, or is not external content.
	 * @param key
	 */
	externalContentNameOf(key: string) {
		return mergeObservables(
			[this.#externalContentVariants.asObservablePart((source) => source.find((x) => x.key === key)), this.variantId],
			([entry, variantId]) => findExternalContentVariant(entry?.variants, variantId)?.name ?? null,
		);
	}

	/**
	 * Returns an observable of why the external content is unavailable.
	 * Emits `undefined` while the content is available or not yet resolved.
	 * @param key The content key of the external content.
	 */
	externalContentFailureOf(key: string) {
		return this.#externalContentFailures.asObservablePart((source) => source.find((x) => x.key === key)?.reason);
	}

	async #requestExternalContentFailures() {
		// One request at a time, so that a server that is down is asked once, not once per referenced element.
		if (this.#isProbingExternalContentFailures) return;
		this.#isProbingExternalContentFailures = true;

		try {
			let key = this.#nextUnexplainedExternalContentKey();
			while (key) {
				const { error } = await this.#elementRepository.requestByUnique(key);
				const reason = getExternalContentFailureReason(error);
				// A failure that says nothing about availability (the server being down, for example) is no reason to ask
				// about the other elements as well. They are asked again the next time the statuses change.
				if (!reason) return;

				this.#externalContentFailures.appendOne({ key, reason });
				key = this.#nextUnexplainedExternalContentKey();
			}
		} finally {
			this.#isProbingExternalContentFailures = false;
		}
	}

	#nextUnexplainedExternalContentKey() {
		const explained = this.#externalContentFailures.getValue();
		return this.#unavailableExternalContentKeys.find((key) => !explained.some((x) => x.key === key));
	}

	settingsOf(key: string) {
		return this.#settings.asObservablePart((source) => source?.find((x) => x.key === key));
	}

	currentExposeOf(contentKey: string) {
		return mergeObservables(
			[this.#exposes.asObservablePart((source) => source.filter((x) => x.contentKey === contentKey)), this.variantId],
			([exposes, variantId]) => {
				if (!variantId) {
					return undefined;
				}

				const contentTypeKey = this.getContentTypeKeyOfContentKey(contentKey);
				if (!contentTypeKey) {
					return false;
				}
				const contentStructure = this.getStructure(contentTypeKey);
				if (!contentStructure) {
					throw new Error(`Cannot lookup expose of block, missing content structure for ${contentTypeKey}`);
				}

				const varyByCulture = contentStructure.getVariesByCulture();
				const varyBySegment = contentStructure.getVariesBySegment();
				const blockVariantId = variantId.toVariant(varyByCulture, varyBySegment);

				return exposes.find((x) => blockVariantId.culture === x.culture);
			},
		);
	}

	hasExposeOf(contentKey: string, variantId: UmbVariantId) {
		if (!variantId) return;

		const contentTypeKey = this.getContentTypeKeyOfContentKey(contentKey);
		if (!contentTypeKey) {
			// Not created yet and therefor not exposed.
			// (This currently does not give any trouble,
			// but this is not returning a observable,
			// meaning one trying to observe this would
			// not be updated when it is exposed. But it
			// is not a problem currently. [NL])
			return;
		}
		const contentStructure = this.getStructure(contentTypeKey);
		if (!contentStructure) {
			throw new Error(`Cannot lookup expose of block, missing content structure for ${contentTypeKey}`);
		}
		const varyByCulture = contentStructure.getVariesByCulture();
		const blockVariantId = variantId.toVariant(varyByCulture);

		return this.#exposes.asObservablePart((exposes) => {
			return exposes.some((x) => x.contentKey === contentKey && blockVariantId.culture === x.culture);
		});
	}

	getBlockTypeOf(contentTypeKey: string) {
		return this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentTypeKey);
	}
	getContentOf(contentKey: string) {
		return (
			this.#contents.value?.find((x) => x.key === contentKey) ??
			this.#externalContentValues.value.find((x) => x.key === contentKey)
		);
	}
	getSettingsOf(settingsKey: string) {
		return this.#settings.value?.find((x) => x.key === settingsKey);
	}
	// originData param is used by some implementations. [NL] should be here, do not remove it.

	setOneLayout(layoutData: BlockLayoutType, _originData?: BlockOriginDataType) {
		this._layouts.appendOne(layoutData);
	}
	setOneContent(contentData: UmbBlockDataModel) {
		this.#contents.appendOne(contentData);
	}
	setOneSettings(settingsData: UmbBlockDataModel) {
		this.#settings.appendOne(settingsData);
	}
	setOneExpose(contentKey: string, variantId: UmbVariantId) {
		if (!variantId) return;
		this.#exposes.appendOne({ contentKey, ...variantId.toObject() });
	}

	removeOneContent(contentKey: string) {
		this.#contents.removeOne(contentKey);
	}
	removeOneSettings(settingsKey: string) {
		this.#settings.removeOne(settingsKey);
	}

	removeManyContent(contentKeys: Array<string>) {
		this.#contents.remove(contentKeys);
	}
	removeManySettings(settingsKeys: Array<string>) {
		this.#settings.remove(settingsKeys);
	}

	removeExposesOf(contentKey: string) {
		this.#exposes.filter((x) => x.contentKey !== contentKey);
	}
	removeCurrentExpose(contentKey: string) {
		const variantId = this.getVariantId();
		if (!variantId) return;
		this.#exposes.filter((x) => !(x.contentKey === contentKey && variantId.culture === x.culture));
	}

	/**
	 * Insert a block whose content is an external content reference.
	 * Only creates a layout entry — no contentData entry is added.
	 * The layout observer handles fetching the element data.
	 * The layout is placed through `setOneLayout`, so implementations can position it from the origin data.
	 * @param {string} elementKey - The key of the external element to reference.
	 * @param {BlockOriginDataType} [originData] - Where the block was requested from, used by implementations to position it.
	 */
	insertExternalContent(elementKey: string, originData?: BlockOriginDataType) {
		const layout = { key: UmbId.new(), contentKey: elementKey, isExternalContent: true } as BlockLayoutType;
		this.setOneLayout(layout, originData);
	}

	/**
	 * Request to transfer a local block's content to external content (Element Library).
	 * The Element is created from all cultures the block holds data in, and published in the cultures the block is exposed in.
	 * Nothing is transferred when the user lacks access to a language the block holds data in.
	 * @param {string} key the block layout key.
	 * @param name
	 * @returns {Promise<UmbBlockTransferToExternalContentResult | undefined>} The outcome, or `undefined` when nothing was transferred.
	 */
	async requestTransferToExternalContent(
		key: string,
		name?: string,
	): Promise<UmbBlockTransferToExternalContentResult | undefined> {
		const layout = this._layouts.getValue().find((x) => x.key === key);
		if (!layout) return;
		const contentKey = layout.contentKey;
		const content = this.getContentOf(contentKey);
		if (!content) return;

		await this.contentTypesLoaded;
		const variesByCulture = this.getStructure(content.contentTypeKey)?.getVariesByCulture() ?? false;
		const valueCultures = await this.#getValueCultures(content.values);

		const access = await this.#getTransferLanguageAccess();
		if (!access) return;

		const blocked = umbGetBlockedTransferVariants({ valueCultures, variesByCulture, access });
		if (blocked.cultures.length > 0 || blocked.invariant) {
			await this.#showTransferNotPermitted(blocked);
			return;
		}

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT).catch(() => undefined);
		if (!modalManager) return;
		const result = await modalManager
			.open(this, UMB_BLOCK_TRANSFER_TO_ELEMENT_LIBRARY_MODAL, {
				data: { name, contentTypeKey: content.contentTypeKey },
			})
			.onSubmit()
			.catch(() => undefined);
		if (!result) return;

		const { cultures, culturesToPublish } = umbResolveBlockTransferCultures({
			valueCultures,
			exposedCultures: this.getExposes()
				.filter((x) => x.contentKey === contentKey)
				.map((x) => x.culture),
			variesByCulture,
			fallbackCulture: this.getVariantId()?.culture,
			access,
		});

		const { data: scaffold } = await this.#elementRepository.createScaffold({
			documentType: { unique: content.contentTypeKey, collection: null },
			values: content.values,
			variants: cultures.map((culture) => ({
				culture,
				state: null,
				name: result.name,
				publishDate: null,
				createDate: null,
				updateDate: null,
			})),
		});
		if (!scaffold) return;

		const { data: created } = await this.#elementRepository.create(scaffold, result.parentUnique);
		if (!created) return;

		// Publish before swapping the layout, as the layout change makes the external content manager fetch the Element.
		const publishResult = await this.#publishTransferredElement(created, culturesToPublish);

		this.#contents.removeOne(contentKey);
		this.removeExposesOf(contentKey);
		this._layouts.updateOne(key, {
			contentKey: created.unique,
			isExternalContent: true,
		} as Partial<BlockLayoutType>);

		return { unique: created.unique, ...publishResult };
	}

	async #getValueCultures(values: Array<UmbBlockDataValueModel>): Promise<Array<string | null>> {
		const cultures = await new UmbPropertyValueFlatMapperController(this).flatMapMany(values, (property) =>
			// Be aware that a value of `false` or `""` or `[]` is not captured here:
			property.value ? property.culture : undefined,
		);
		return cultures.filter((culture): culture is string | null => culture !== undefined);
	}

	async #getTransferLanguageAccess(): Promise<UmbBlockTransferLanguageAccess | undefined> {
		const context = await this.getContext(UMB_CURRENT_USER_CONTEXT).catch(() => undefined);
		if (!context) return;
		const user = await this.observe(context.currentUser).asPromise();
		return {
			hasAccessToAllLanguages: user.hasAccessToAllLanguages,
			languages: user.languages,
			hasAccessToInvariantForVariant: user.hasAccessToInvariantForVariant !== false,
		};
	}

	async #showTransferNotPermitted(blocked: ReturnType<typeof umbGetBlockedTransferVariants>) {
		const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT).catch(() => undefined);
		// Resolves once the languages have loaded.
		await appLanguageContext?.getMandatoryLanguages();
		const languages = appLanguageContext ? await this.observe(appLanguageContext.languages).asPromise() : [];
		const languageNames = blocked.cultures.map(
			(culture) => languages.find((language) => language.unique === culture)?.name ?? culture,
		);

		const content: Array<string> = [];
		if (languageNames.length > 0) {
			content.push(
				this.#localize.term(
					'blockEditor_transferToElementLibraryLanguagesNotPermitted',
					escapeHTML(languageNames.join(', ')),
				),
			);
		}
		if (blocked.invariant) {
			content.push(this.#localize.term('blockEditor_transferToElementLibrarySharedNotPermitted'));
		}

		await umbInfoModal(this, {
			headline: '#blockEditor_transferToElementLibraryNotPermitted',
			content: content.join(' '),
		}).catch(() => undefined);
	}

	async #publishTransferredElement(
		element: UmbElementDetailModel,
		culturesToPublish: Array<string | null>,
	): Promise<Omit<UmbBlockTransferToExternalContentResult, 'unique'>> {
		if (culturesToPublish.length === 0) return { status: 'draft' };

		const variantIds = culturesToPublish.map((culture) => new UmbVariantId(culture));

		// Validated without notifications, so a failure is reported once, by the caller.
		const { error: validationError } = await this.#elementValidationRepository.validateSave(element, variantIds);
		if (validationError) {
			if (UmbApiError.isUmbApiError(validationError) && isPublishValidationError(validationError)) {
				return { status: 'validation-failed' };
			}
			return { status: 'publish-failed' };
		}

		const { error } = await this.#elementPublishingRepository.publish(
			element.unique,
			variantIds.map((variantId) => ({ variantId })),
		);
		if (!error) return { status: 'published' };

		if (UmbApiError.isUmbApiError(error) && isPublishValidationError(error)) return { status: 'validation-failed' };
		return { status: 'publish-failed', notified: apiErrorWasNotified(error) };
	}

	/**
	 * Request to disconnect a block from external content (Element Library).
	 * @param {string} key the block layout key.
	 */
	async requestDisconnectFromExternalContent(key: string) {
		const layout = this._layouts.getValue().find((x) => x.key === key);
		if (!layout) return;
		const elementKey = layout.contentKey;

		try {
			await umbConfirmModal(this, {
				headline: '#blockEditor_disconnectFromElementLibrary',
				content: '#blockEditor_disconnectFromElementLibraryConfirm',
				confirmLabel: '#blockEditor_disconnectFromElementLibrary',
				color: 'warning',
			});
		} catch {
			return; // user cancelled
		}

		const { data: element } = await this.#elementRepository.requestByUnique(elementKey);
		if (!element) return;

		const contentTypeKey = element.documentType.unique;
		const newContent: UmbBlockDataModel = {
			key: UmbId.new(),
			contentTypeKey,
			values: element.values.map(
				(v): UmbBlockDataValueModel => ({
					alias: v.alias,
					editorAlias: v.editorAlias,
					culture: v.culture,
					segment: v.segment,
					value: v.value,
				}),
			),
		};
		this.#contents.appendOne(newContent);
		this._layouts.updateOne(key, {
			contentKey: newContent.key,
			isExternalContent: undefined,
		} as Partial<BlockLayoutType>);
		// Only set expose if the content type structure is loaded (it may not be for external content
		// whose type was not in the block type list)
		if (this.getStructure(contentTypeKey)) {
			this.#setInitialBlockExpose(newContent);
		}
	}

	/**
	 * @param key
	 * @param propertyAlias
	 * @param value
	 * @deprecated this is not working, use the Property Dataset instead.
	 */
	// TODO: Deprecate and remove this.
	setOneContentProperty(key: string, propertyAlias: string, value: unknown) {
		this.#contents.updateOne(key, { [propertyAlias]: value });
	}
	/**
	 * @param key
	 * @param propertyAlias
	 * @param value
	 * @deprecated this is not working, use the Property Dataset instead.
	 */
	// TODO: Deprecate and remove this.
	setOneSettingsProperty(key: string, propertyAlias: string, value: unknown) {
		this.#settings.updateOne(key, { [propertyAlias]: value });
	}

	/**
	 * @param key
	 * @param propertyAlias
	 * @deprecated this is not working, use the Property Dataset instead.
	 */
	// TODO: Deprecate and remove this.
	contentProperty(key: string, propertyAlias: string) {
		this.#contents.asObservablePart(
			(source) => source?.find((x) => x.key === key)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}
	/**
	 * @param key
	 * @param propertyAlias
	 * @deprecated this is not working, use the Property Dataset instead.
	 */
	// TODO: Deprecate and remove this.
	settingsProperty(key: string, propertyAlias: string) {
		this.#settings.asObservablePart(
			(source) => source?.find((x) => x.key === key)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}

	abstract createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey' | 'key'>,
		originData?: BlockOriginDataType,
	): Promise<UmbBlockDataObjectModel<BlockLayoutType> | undefined>;

	public async createBlockSettingsData(contentElementTypeKey: string) {
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block settings, missing block type for ${contentElementTypeKey}`);
		}
		if (!blockType.settingsElementTypeKey) {
			throw new Error(`Cannot create block settings, missing settings element type for ${contentElementTypeKey}`);
		}

		// TODO: Handle presets here [NL]

		return {
			contentTypeKey: blockType.settingsElementTypeKey,
			key: UmbId.new(),
			values: [],
		};
	}

	protected async _createBlockElementData(key: string, contentTypeKey: string) {
		const appLanguage = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		if (!appLanguage) {
			throw new Error('Could not retrieve app language context.');
		}

		const contentStructure = this.getStructure(contentTypeKey);
		if (!contentStructure) {
			throw new Error(`Cannot create Preset for Block, missing content structure for ${contentTypeKey}`);
		}

		// Ensure the content type and all its compositions are fully loaded before reading properties.
		// Without this, getContentTypeProperties() may return only the owner type's properties
		// if compositions haven't finished loading yet.
		await contentStructure.whenLoaded();

		// Use the synchronous boolean getters, not the Observable properties (which are always truthy).
		const hostVariesByCulture = contentStructure.getVariesByCulture() ?? false;
		const hostVariesBySegment = contentStructure.getVariesBySegment() ?? false;

		const cultures = hostVariesByCulture ? await appLanguage.getCultures() : [];
		if (hostVariesByCulture && cultures.length === 0) {
			throw new Error('Could not retrieve app cultures.');
		}
		// TODO: Receive the segments from somewhere. [NL]
		const segments: Array<string> | undefined = hostVariesBySegment ? [] : undefined;

		const repo = new UmbDataTypeDetailRepository(this);

		const propertyTypes = await contentStructure.getContentTypeProperties();
		const valueDefinitions = await Promise.all(
			propertyTypes.map(async (property) => {
				// TODO: Implement caching for data-type requests. [NL]
				const dataType = (await repo.requestByUnique(property.dataType.unique)).data;
				// This means if its not loaded this will never resolve and the error below will never happen.
				if (!dataType) {
					throw new Error(`DataType of "${property.dataType.unique}" not found.`);
				}
				if (!dataType.editorUiAlias) {
					throw new Error(`DataType of "${property.dataType.unique}" did not have a editorUiAlias.`);
				}

				return {
					alias: property.alias,
					propertyEditorUiAlias: dataType.editorUiAlias,
					propertyEditorSchemaAlias: dataType.editorAlias,
					config: dataType.values,
					typeArgs: {
						// Normalize property variance against the host content type, mirroring the
						// server-side logic in ContentTypeCompositionBase (propertyType.Variations &= Variations).
						// Properties from compositions are loaded individually and carry their own variance flags,
						// which may not match the host's effective variance (GH-22230).
						variesByCulture: property.variesByCulture && hostVariesByCulture,
						variesBySegment: property.variesBySegment && hostVariesBySegment,
					} as UmbPropertyTypePresetModelTypeModel,
				} as UmbPropertyTypePresetModel;
			}),
		);

		const controller = new UmbPropertyValuePresetVariantBuilderController(this);
		controller.setCultures(cultures);
		if (segments) {
			controller.setSegments(segments);
		}
		const values = await controller.create(valueDefinitions, {
			entityType: 'block',
			entityUnique: key,
			entityTypeUnique: contentTypeKey,
		});

		return {
			key,
			contentTypeKey,
			values,
		};
	}

	protected async _createBlockData(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey' | 'key'>,
	) {
		// Find block type.
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		const contentKey = UmbId.new();

		// Create layout entry:
		const layout: BlockLayoutType = {
			key: contentKey,
			contentKey,
			...(partialLayoutEntry as Partial<BlockLayoutType>),
		} as BlockLayoutType;

		const content = await this._createBlockElementData(layout.contentKey, contentElementTypeKey);
		let settings: UmbBlockDataModel | undefined = undefined;

		if (blockType.settingsElementTypeKey) {
			layout.settingsKey = UmbId.new();
			settings = await this._createBlockElementData(layout.settingsKey, blockType.settingsElementTypeKey);
		}

		return {
			layout,
			content,
			settings,
		};
	}

	abstract insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: BlockOriginDataType,
	): boolean;

	protected insertBlockData(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,

		_originData: BlockOriginDataType,
	) {
		// Create content entry:
		if (layoutEntry.contentKey) {
			this.#contents.appendOne(content);
		} else {
			throw new Error('Cannot create block, missing contentKey');
		}

		//Create settings entry:
		if (settings && layoutEntry.settingsKey) {
			this.#settings.appendOne(settings);
		}

		// Expose inserted block:
		this.#setInitialBlockExpose(content);
	}

	protected async notifyBlockInserted(layoutEntry: BlockLayoutType, originData: BlockOriginDataType) {
		// Await one rendering frame, to make sure new Block has been rendered at the time of the Event firing. [NL]
		await new Promise((resolve) => requestAnimationFrame(() => resolve(true)));
		this.dispatchEvent(
			new UmbBlockInsertedEvent({
				originData,
				layout: layoutEntry,
			}),
		);
	}

	async #setInitialBlockExpose(content: UmbBlockDataModel) {
		await this.contentTypesLoaded;
		const contentStructure = this.getStructure(content.contentTypeKey);
		if (!contentStructure) {
			throw new Error(`Cannot expose block, missing content structure for ${content.contentTypeKey}`);
		}
		const variantId = this.getVariantId();
		if (!variantId) {
			throw new Error(`Cannot expose block, missing variantId`);
		}

		const varyByCulture = contentStructure.getVariesByCulture();
		const varyBySegment = contentStructure.getVariesBySegment();
		const blockVariantId = variantId.toVariant(varyByCulture, varyBySegment);
		this.setOneExpose(content.key, blockVariantId);

		if (varyByCulture) {
			// get all mandatory cultures:
			const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
			if (!appLanguageContext) {
				throw new Error('Could not retrieve app language context.');
			}
			const mandatoryLanguages = await appLanguageContext.getMandatoryLanguages();
			mandatoryLanguages.forEach((x) => {
				// No need to insert the same expose twice:
				if (blockVariantId.culture !== x.unique) {
					this.setOneExpose(content.key, new UmbVariantId(x.unique));
				}
			});
		}
	}

	/**
	 * @deprecated Use `removeOneContent` instead. Scheduled for removal in Umbraco 20.
	 * @param {string} contentKey - The content key of the layout element to delete.
	 * @internal
	 */
	protected removeBlockKey(contentKey: string) {
		new UmbDeprecation({
			deprecated: 'removeBlockKey is deprecated.',
			removeInVersion: '20.0.0',
			solution: 'Use removeOneContent instead.',
		}).warn();
		this.removeOneContent(contentKey);
	}
}
