import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbClassState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { buildUdi, getKeyFromUdi } from '@umbraco-cms/backoffice/utils';
import type { UmbBlockTypeBaseModel, UmbBlockWorkspaceData } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_MANAGER_CONTEXT, UMB_BLOCK_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/block';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

// TODO: We are using backend model here, I think we should get our own model:
type ElementTypeModel = UmbContentTypeModel;

export abstract class UmbBlockManagerContext<
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<UmbBlockManagerContext> {
	//
	#contentTypeRepository = new UmbDocumentTypeDetailRepository(this);
	#workspaceModal: UmbModalRouteRegistrationController;

	#workspacePath = new UmbStringState(undefined);
	workspacePath = this.#workspacePath.asObservable();

	#propertyAlias = new UmbStringState(undefined);
	propertyAlias = this.#propertyAlias.asObservable();

	#contentTypes = new UmbArrayState(<Array<ElementTypeModel>>[], (x) => x.unique);
	public readonly contentTypes = this.#contentTypes.asObservable();

	#blockTypes = new UmbArrayState(<Array<BlockType>>[], (x) => x.contentElementTypeKey);
	public readonly blockTypes = this.#blockTypes.asObservable();

	#editorConfiguration = new UmbClassState<UmbPropertyEditorConfigCollection | undefined>(undefined);
	public readonly editorConfiguration = this.#editorConfiguration.asObservable();

	protected _layouts = new UmbArrayState(<Array<BlockLayoutType>>[], (x) => x.contentUdi);
	public readonly layouts = this._layouts.asObservable();

	#contents = new UmbArrayState(<Array<UmbBlockDataType>>[], (x) => x.udi);
	public readonly contents = this.#contents.asObservable();

	#settings = new UmbArrayState(<Array<UmbBlockDataType>>[], (x) => x.udi);
	public readonly settings = this.#settings.asObservable();

	// TODO: maybe its bad to consume Property Context, and instead wire this up manually in the property editor? With these:
	/*setPropertyAlias(alias: string) {
		this.#propertyAlias.setValue(alias);
		console.log('!!!!!manager got alias: ', alias);
		this.#workspaceModal.setUniquePathValue('propertyAlias', alias);
	}
	getPropertyAlias() {
		this.#propertyAlias.value;
	}*/

	setEditorConfiguration(configs: UmbPropertyEditorConfigCollection) {
		this.#editorConfiguration.setValue(configs);
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

	setContents(contents: Array<UmbBlockDataType>) {
		this.#contents.setValue(contents);
	}
	setSettings(settings: Array<UmbBlockDataType>) {
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
		});

		this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_WORKSPACE_MODAL)
			.addUniquePaths(['propertyAlias'])
			.addAdditionalPath('block')
			.onSetup(() => {
				return { data: { entityType: 'block', preset: {} }, modal: { size: 'medium' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newPath = routeBuilder({});
				this.#workspacePath.setValue(newPath);
			});

		this.observe(this.propertyAlias, (alias) => {
			this.#workspaceModal.setUniquePathValue('propertyAlias', alias);
		});
	}

	async ensureContentType(unique?: string) {
		if (!unique) return;
		if (this.#contentTypes.getValue().find((x) => x.unique === unique)) return;
		const contentType = await this.#loadContentType(unique);
		return contentType;
	}

	async #loadContentType(unique?: string) {
		if (!unique) return {};

		const { data } = await this.#contentTypeRepository.requestByUnique(unique);
		if (!data) return {};

		// We could have used the global store of Document Types, but to ensure we first react ones the latest is loaded then we have our own local store:
		// TODO: Revisit if this is right to do. Notice this can potentially be proxied to the global store.
		this.#contentTypes.appendOne(data);

		return data;
	}

	contentTypeOf(contentTypeUdi: string) {
		const contentTypeUnique = getKeyFromUdi(contentTypeUdi);
		return this.#contentTypes.asObservablePart((source) => source.find((x) => x.unique === contentTypeUnique));
	}
	contentTypeNameOf(contentTypeKey: string) {
		return this.#contentTypes.asObservablePart((source) => source.find((x) => x.unique === contentTypeKey)?.name);
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

	setOneLayout(layoutData: BlockLayoutType) {
		return this._layouts.appendOne(layoutData);
	}
	setOneContent(contentData: UmbBlockDataType) {
		this.#contents.appendOne(contentData);
	}
	setOneSettings(settingsData: UmbBlockDataType) {
		this.#settings.appendOne(settingsData);
	}

	abstract create(
		modalData: UmbBlockWorkspaceData,
		layoutEntry: BlockLayoutType,
		contentElementTypeKey: string,
	): boolean;

	public createBlock<ModalDataType extends UmbBlockWorkspaceData>(
		modalData: ModalDataType,
		layoutEntry: Omit<BlockLayoutType, 'contentUdi'>,
		contentElementTypeKey: string,
		callback: (modalData: ModalDataType, layoutEntry: BlockLayoutType, contentElementTypeKey: string) => boolean,
	) {
		// Find block type.
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
			return false;
		}

		// Create layout entry:
		const fullLayoutEntry: BlockLayoutType = {
			contentUdi: buildUdi('element', UmbId.new()),
			...(layoutEntry as Partial<BlockLayoutType>),
		} as BlockLayoutType;
		if (blockType.settingsElementTypeKey) {
			fullLayoutEntry.settingsUdi = buildUdi('element', UmbId.new());
		}

		if (callback(modalData, fullLayoutEntry, contentElementTypeKey) === false) {
			return false;
		}

		// Create content entry:
		if (fullLayoutEntry.contentUdi) {
			this.#contents.appendOne({
				contentTypeKey: contentElementTypeKey,
				udi: fullLayoutEntry.contentUdi,
			});
		} else {
			throw new Error('Cannot create block, missing contentUdi');
			return false;
		}

		//Create settings entry:
		if (blockType.settingsElementTypeKey) {
			if (fullLayoutEntry.settingsUdi) {
				this.#contents.appendOne({
					contentTypeKey: blockType.settingsElementTypeKey,
					udi: fullLayoutEntry.settingsUdi,
				});
			} else {
				throw new Error('Cannot create block, missing settingsUdi');
				return false;
			}
		}

		return true;
	}

	// Idea: should we return true if it was successful?
	deleteBlock(contentUdi: string) {
		const layout = this._layouts.value.find((x) => x.contentUdi === contentUdi);
		if (!layout) {
			throw new Error(`Cannot delete block, missing layout for ${contentUdi}`);
		}

		if (layout.settingsUdi) {
			this.#settings.removeOne(layout.settingsUdi);
		}

		this._layouts.removeOne(contentUdi);
		this.#contents.removeOne(contentUdi);
	}
}
