import type { UmbBlockWorkspaceOriginData } from '../workspace/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataModel, UmbBlockExposeModel } from '../types.js';
import { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
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
import { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import {
	UmbPropertyValuePresetVariantBuilderController,
	type UmbPropertyTypePresetModel,
	type UmbPropertyTypePresetModelTypeModel,
} from '@umbraco-cms/backoffice/property';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';

export type UmbBlockDataObjectModel<LayoutEntryType extends UmbBlockLayoutBaseModel> = {
	layout: LayoutEntryType;
	content: UmbBlockDataModel;
	settings?: UmbBlockDataModel;
};
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

	protected _layouts = new UmbArrayState(<Array<BlockLayoutType>>[], (x) => x.contentKey);
	public readonly layouts = this._layouts.asObservable();

	readonly #contents = new UmbArrayState(<Array<UmbBlockDataModel>>[], (x) => x.key);
	public readonly contents = this.#contents.asObservable();

	readonly #settings = new UmbArrayState(<Array<UmbBlockDataModel>>[], (x) => x.key);
	public readonly settings = this.#settings.asObservable();

	// TODO: This is a bad seperation of concerns, this should be self initializing, not defined from the outside. [NL]
	public readonly readOnlyState = new UmbReadOnlyVariantGuardManager(this);

	readonly #exposes = new UmbArrayState(
		<Array<UmbBlockExposeModel>>[],
		(x) => x.contentKey + '_' + x.culture + '_' + x.segment,
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
		this._layouts.setValue(layouts);
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
	setContents(contents: Array<UmbBlockDataModel>) {
		this.#contents.setValue(contents);
	}

	/**
	 * Get all contents.
	 * @returns {Array<UmbBlockDataModel>} - All contents.
	 */
	getContents(): Array<UmbBlockDataModel> {
		return this.#contents.value;
	}

	/**
	 * Set all settings.
	 * @param {Array<UmbBlockDataModel>} settings - All settings.
	 */
	setSettings(settings: Array<UmbBlockDataModel>) {
		this.#settings.setValue(settings);
	}

	/**
	 * Get all settings.
	 * @returns {Array<UmbBlockDataModel>} - All settings.
	 */
	getSettings(): Array<UmbBlockDataModel> {
		return this.#settings.value;
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
				blockTypes.forEach((x) => {
					this.#ensureContentType(x.contentElementTypeKey);
					if (x.settingsElementTypeKey) {
						this.#ensureContentType(x.settingsElementTypeKey);
					}
				});
			},
			null,
		);
	}

	async #ensureContentType(unique: string) {
		if (this.#structures.find((x) => x.getOwnerContentTypeUnique() === unique)) return;

		// Lets try to go with the UmbContentTypeModel, to make this as compatible with other ContentTypes as possible, but maybe if off with this as Blocks are always based on ElementTypes.. [NL]
		const structure = new UmbContentTypeStructureManager<UmbContentTypeModel>(this, this.#contentTypeRepository);
		const initialRequest = structure.loadType(unique);
		this.#contentTypeRequests.push(initialRequest);
		this.#structures.push(structure);
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
		return this.#contents.asObservablePart((source) => source.find((x) => x.key === key));
	}
	settingsOf(key: string) {
		return this.#settings.asObservablePart((source) => source.find((x) => x.key === key));
	}
	currentExposeOf(contentKey: string) {
		const variantId = this.getVariantId();
		if (!variantId) return;
		return mergeObservables(
			[this.#exposes.asObservablePart((source) => source.filter((x) => x.contentKey === contentKey)), this.variantId],
			([exposes, variantId]) => (variantId ? exposes.find((x) => variantId.compare(x)) : undefined),
		);
	}

	hasExposeOf(contentKey: string, variantId: UmbVariantId) {
		if (!variantId) return;
		return this.#exposes.asObservablePart((source) =>
			source.some((x) => x.contentKey === contentKey && variantId.compare(x)),
		);
	}

	getBlockTypeOf(contentTypeKey: string) {
		return this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentTypeKey);
	}
	getContentOf(contentKey: string) {
		return this.#contents.value.find((x) => x.key === contentKey);
	}
	getSettingsOf(settingsKey: string) {
		return this.#settings.value.find((x) => x.key === settingsKey);
	}
	// originData param is used by some implementations. [NL] should be here, do not remove it.
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
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
		this.#exposes.filter((x) => !(x.contentKey === contentKey && variantId.compare(x)));
	}

	setOneContentProperty(key: string, propertyAlias: string, value: unknown) {
		this.#contents.updateOne(key, { [propertyAlias]: value });
	}
	setOneSettingsProperty(key: string, propertyAlias: string, value: unknown) {
		this.#settings.updateOne(key, { [propertyAlias]: value });
	}

	contentProperty(key: string, propertyAlias: string) {
		this.#contents.asObservablePart(
			(source) => source.find((x) => x.key === key)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}
	settingsProperty(key: string, propertyAlias: string) {
		this.#settings.asObservablePart(
			(source) => source.find((x) => x.key === key)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}

	/**
	 * @deprecated Use `createWithPresets` instead. Which is Async. Will be removed in v.17
	 * @param contentElementTypeKey
	 * @param partialLayoutEntry
	 * @param originData
	 */
	abstract create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		originData?: BlockOriginDataType,
	): never;

	abstract createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
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
			key: UmbId.new(),
			contentTypeKey: blockType.settingsElementTypeKey,
			values: [],
		};
	}

	protected async _createBlockElementData(key: string, contentTypeKey: string) {
		//
		const appLanguage = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		if (!appLanguage) {
			throw new Error('Could not retrieve app language context.');
		}

		const contentStructure = this.getStructure(contentTypeKey);
		if (!contentStructure) {
			throw new Error(`Cannot create Preset for Block, missing content structure for ${contentTypeKey}`);
		}

		// Set culture and segment for all values:
		const cutlures = contentStructure.variesByCulture ? await appLanguage.getCultures() : [];
		if (cutlures.length === 0) {
			throw new Error('Could not retrieve app cultures.');
		}
		// TODO: Receive the segments from somewhere. [NL]
		const segments: Array<string> | undefined = contentStructure.variesBySegment ? [] : undefined;

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
						variesByCulture: property.variesByCulture,
						variesBySegment: property.variesBySegment,
					} as UmbPropertyTypePresetModelTypeModel,
				} as UmbPropertyTypePresetModel;
			}),
		);

		const controller = new UmbPropertyValuePresetVariantBuilderController(this);
		controller.setCultures(cutlures);
		if (segments) {
			controller.setSegments(segments);
		}
		const values = await controller.create(valueDefinitions);

		// Set culture and segment for all values:

		return {
			key,
			contentTypeKey,
			values,
		};
	}

	protected async _createBlockData(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
	) {
		// Find block type.
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		// Create layout entry:
		const layout: BlockLayoutType = {
			contentKey: UmbId.new(),
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
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
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

	protected removeBlockKey(contentKey: string) {
		this.#contents.removeOne(contentKey);
	}
}
