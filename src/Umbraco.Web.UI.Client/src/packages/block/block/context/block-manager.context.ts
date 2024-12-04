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
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';

export type UmbBlockDataObjectModel<LayoutEntryType extends UmbBlockLayoutBaseModel> = {
	layout: LayoutEntryType;
	content: UmbBlockDataModel;
	settings?: UmbBlockDataModel;
};
export abstract class UmbBlockManagerContext<
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockOriginDataType extends UmbBlockWorkspaceOriginData = UmbBlockWorkspaceOriginData,
> extends UmbContextBase<UmbBlockManagerContext> {
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

	public readonly readOnlyState = new UmbReadOnlyVariantStateManager(this);

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

	setLayouts(layouts: Array<BlockLayoutType>) {
		this._layouts.setValue(layouts);
	}
	getLayouts() {
		return this._layouts.getValue();
	}
	setContents(contents: Array<UmbBlockDataModel>) {
		this.#contents.setValue(contents);
	}
	getContents() {
		return this.#contents.value;
	}
	setSettings(settings: Array<UmbBlockDataModel>) {
		this.#settings.setValue(settings);
	}
	setExposes(exposes: Array<UmbBlockExposeModel>) {
		this.#exposes.setValue(exposes);
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT);

		this.observe(this.blockTypes, (blockTypes) => {
			blockTypes.forEach((x) => {
				this.#ensureContentType(x.contentElementTypeKey);
				if (x.settingsElementTypeKey) {
					this.#ensureContentType(x.settingsElementTypeKey);
				}
			});
		});
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

	abstract create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		originData?: BlockOriginDataType,
	): UmbBlockDataObjectModel<BlockLayoutType> | undefined;

	public createBlockSettingsData(contentElementTypeKey: string) {
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block settings, missing block type for ${contentElementTypeKey}`);
		}
		if (!blockType.settingsElementTypeKey) {
			throw new Error(`Cannot create block settings, missing settings element type for ${contentElementTypeKey}`);
		}

		return {
			key: UmbId.new(),
			contentTypeKey: blockType.settingsElementTypeKey,
			values: [],
		};
	}

	protected _createBlockElementData(key: string, elementTypeKey: string) {
		return {
			key: key,
			contentTypeKey: elementTypeKey,
			values: [],
		};
	}

	protected _createBlockData(contentElementTypeKey: string, partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>) {
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

		const content = this._createBlockElementData(layout.contentKey, contentElementTypeKey);
		let settings: UmbBlockDataModel | undefined = undefined;

		if (blockType.settingsElementTypeKey) {
			layout.settingsKey = UmbId.new();
			settings = this._createBlockElementData(layout.settingsKey, blockType.settingsElementTypeKey);
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
		this.contentTypesLoaded.then(() => {
			const contentStructure = this.getStructure(content.contentTypeKey);
			if (!contentStructure) {
				throw new Error(`Cannot expose block, missing content structure for ${content.contentTypeKey}`);
			}
			const variantId = this.getVariantId();
			if (!variantId) {
				throw new Error(`Cannot expose block, missing variantId`);
			}
			const blockVariantId = variantId.toVariant(
				contentStructure.getVariesByCulture(),
				contentStructure.getVariesBySegment(),
			);
			this.setOneExpose(content.key, blockVariantId);
		});
	}

	protected removeBlockKey(contentKey: string) {
		this.#contents.removeOne(contentKey);
	}
}
