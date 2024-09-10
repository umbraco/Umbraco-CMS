import type { UmbBlockWorkspaceOriginData } from '../workspace/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataModel } from '../types.js';
import { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbClassState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContentTypeStructureManager, type UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 *
 * @param {string} entityType - The entity type
 * @param {string} guid - The entity
 * @returns {string} UDI in the format `umb://<entityType>/<guid>`
 */
function buildUdi(entityType: string, guid: string) {
	return `umb://${entityType}/${guid.replace(/-/g, '')}`;
}

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
	//
	get contentTypesLoaded() {
		return Promise.all(this.#contentTypeRequests);
	}
	#contentTypeRequests: Array<Promise<unknown>> = [];
	#contentTypeRepository = new UmbDocumentTypeDetailRepository(this);

	#propertyAlias = new UmbStringState(undefined);
	propertyAlias = this.#propertyAlias.asObservable();

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	variantId = this.#variantId.asObservable();

	#structures: Array<UmbContentTypeStructureManager> = [];

	#blockTypes = new UmbArrayState(<Array<BlockType>>[], (x) => x.contentElementTypeKey);
	public readonly blockTypes = this.#blockTypes.asObservable();

	protected _editorConfiguration = new UmbClassState<UmbPropertyEditorConfigCollection | undefined>(undefined);
	public readonly editorConfiguration = this._editorConfiguration.asObservable();

	protected _liveEditingMode = new UmbBooleanState(undefined);
	public readonly liveEditingMode = this._liveEditingMode.asObservable();

	protected _layouts = new UmbArrayState(<Array<BlockLayoutType>>[], (x) => x.contentUdi);
	public readonly layouts = this._layouts.asObservable();

	#contents = new UmbArrayState(<Array<UmbBlockDataModel>>[], (x) => x.udi);
	public readonly contents = this.#contents.asObservable();

	#settings = new UmbArrayState(<Array<UmbBlockDataModel>>[], (x) => x.udi);
	public readonly settings = this.#settings.asObservable();

	setEditorConfiguration(configs: UmbPropertyEditorConfigCollection) {
		this._editorConfiguration.setValue(configs);
		if (this._liveEditingMode.getValue() === undefined) {
			this._liveEditingMode.setValue(configs.getValueByAlias<boolean>('liveEditingMode'));
		}
	}
	getEditorConfiguration(): UmbPropertyEditorConfigCollection | undefined {
		return this._editorConfiguration.getValue();
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
	setContents(contents: Array<UmbBlockDataModel>) {
		this.#contents.setValue(contents);
	}
	setSettings(settings: Array<UmbBlockDataModel>) {
		this.#settings.setValue(settings);
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT);

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(
				propertyContext?.alias,
				(alias) => {
					this.#propertyAlias.setValue(alias);
				},
				'observePropertyAlias',
			);
			this.observe(
				propertyContext?.variantId,
				(variantId) => {
					this.#variantId.setValue(variantId);
				},
				'observePropertyVariantId',
			);
		});

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

	getContentTypeKeyOfContentUdi(contentUdi: string) {
		return this.getContentOf(contentUdi)?.contentTypeKey;
	}
	contentTypeOf(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.ownerContentType;
	}
	contentTypeNameOf(contentTypeKey: string) {
		const structure = this.#structures.find((x) => x.getOwnerContentTypeUnique() === contentTypeKey);
		if (!structure) return undefined;

		return structure.ownerContentTypePart((x) => x?.name);
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

	layoutOf(contentUdi: string) {
		return this._layouts.asObservablePart((source) => source.find((x) => x.contentUdi === contentUdi));
	}
	contentOf(udi: string) {
		return this.#contents.asObservablePart((source) => source.find((x) => x.udi === udi));
	}
	settingsOf(udi: string) {
		return this.#settings.asObservablePart((source) => source.find((x) => x.udi === udi));
	}

	getBlockTypeOf(contentTypeKey: string) {
		return this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentTypeKey);
	}
	getContentOf(contentUdi: string) {
		return this.#contents.value.find((x) => x.udi === contentUdi);
	}
	// TODO: [v15]: ignoring unused var here here to prevent a breaking change
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setOneLayout(layoutData: BlockLayoutType, originData?: BlockOriginDataType) {
		this._layouts.appendOne(layoutData);
	}
	setOneContent(contentData: UmbBlockDataModel) {
		this.#contents.appendOne(contentData);
	}
	setOneSettings(settingsData: UmbBlockDataModel) {
		this.#settings.appendOne(settingsData);
	}

	removeOneContent(contentUdi: string) {
		this.#contents.removeOne(contentUdi);
	}
	removeOneSettings(settingsUdi: string) {
		this.#settings.removeOne(settingsUdi);
	}

	setOneContentProperty(udi: string, propertyAlias: string, value: unknown) {
		this.#contents.updateOne(udi, { [propertyAlias]: value });
	}
	setOneSettingsProperty(udi: string, propertyAlias: string, value: unknown) {
		this.#settings.updateOne(udi, { [propertyAlias]: value });
	}

	contentProperty(udi: string, propertyAlias: string) {
		this.#contents.asObservablePart(
			(source) => source.find((x) => x.udi === udi)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}
	settingsProperty(udi: string, propertyAlias: string) {
		this.#contents.asObservablePart(
			(source) => source.find((x) => x.udi === udi)?.values?.find((values) => values.alias === propertyAlias)?.value,
		);
	}

	abstract create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
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
			udi: buildUdi('element', UmbId.new()),
			contentTypeKey: blockType.settingsElementTypeKey,
			values: [],
		};
	}

	protected _createBlockElementData(udi: string, elementTypeKey: string) {
		return {
			udi: udi,
			contentTypeKey: elementTypeKey,
			values: [],
		};
	}

	protected _createBlockData(contentElementTypeKey: string, partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>) {
		// Find block type.
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		// Create layout entry:
		const layout: BlockLayoutType = {
			contentUdi: buildUdi('element', UmbId.new()),
			...(partialLayoutEntry as Partial<BlockLayoutType>),
		} as BlockLayoutType;

		const content = this._createBlockElementData(layout.contentUdi, contentElementTypeKey);
		let settings: UmbBlockDataModel | undefined = undefined;

		if (blockType.settingsElementTypeKey) {
			layout.settingsUdi = buildUdi('element', UmbId.new());
			settings = this._createBlockElementData(layout.settingsUdi, blockType.settingsElementTypeKey);
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
		// TODO: [v15]: ignoring unused var here here to prevent a breaking change
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		originData: BlockOriginDataType,
	) {
		// Create content entry:
		if (layoutEntry.contentUdi) {
			this.#contents.appendOne(content);
		} else {
			throw new Error('Cannot create block, missing contentUdi');
			return false;
		}

		//Create settings entry:
		if (settings && layoutEntry.settingsUdi) {
			this.#settings.appendOne(settings);
		}
	}
}
