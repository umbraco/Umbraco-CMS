import type { UmbBlockTypeBase } from '../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDocumentTypeDetailModel, UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { getKeyFromUdi } from '@umbraco-cms/backoffice/utils';

// TODO: We are using backend model here, I think we should get our own model:
type ElementTypeModel = UmbDocumentTypeDetailModel;

export class UmbBlockManagerContext<
	BlockType extends UmbBlockTypeBase = UmbBlockTypeBase,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<UmbBlockManagerContext> {
	//
	#contentTypeRepository = new UmbDocumentTypeDetailRepository(this);

	#contentTypes = new UmbArrayState(<Array<ElementTypeModel>>[], (x) => x.unique);
	public readonly contentTypes = this.#contentTypes.asObservable();

	#blockTypes = new UmbArrayState(<Array<BlockType>>[], (x) => x.contentElementTypeKey);
	public readonly blockTypes = this.#blockTypes.asObservable();

	#layouts = new UmbArrayState(<Array<BlockLayoutType>>[], (x) => x.contentUdi);
	public readonly layouts = this.#layouts.asObservable();

	#contents = new UmbArrayState(<Array<UmbBlockDataType>>[], (x) => x.udi);
	public readonly contents = this.#contents.asObservable();

	#settings = new UmbArrayState(<Array<UmbBlockDataType>>[], (x) => x.udi);
	public readonly settings = this.#settings.asObservable();

	setBlockTypes(blockTypes: Array<BlockType>) {
		this.#blockTypes.next(blockTypes);
	}
	getBlockTypes() {
		return this.#blockTypes.value;
	}

	setLayouts(layouts: Array<BlockLayoutType>) {
		this.#layouts.next(layouts);
	}

	setContents(contents: Array<UmbBlockDataType>) {
		this.#contents.next(contents);
	}
	setSettings(settings: Array<UmbBlockDataType>) {
		this.#settings.next(settings);
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT);
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
	contentTypeNameOf(contentTypeUdi: string) {
		const contentTypeUnique = getKeyFromUdi(contentTypeUdi);
		return this.#contentTypes.asObservablePart((source) => source.find((x) => x.unique === contentTypeUnique)?.name);
	}
	blockTypeOf(contentTypeKey: string) {
		return this.#blockTypes.asObservablePart((source) =>
			source.find((x) => x.contentElementTypeKey === contentTypeKey),
		);
	}
	layoutOf(contentUdi: string) {
		return this.#layouts.asObservablePart((source) => source.find((x) => x.contentUdi === contentUdi));
	}
	contentOf(udi: string) {
		return this.#contents.asObservablePart((source) => source.find((x) => x.udi === udi));
	}
	settingsOf(udi: string) {
		return this.#settings.asObservablePart((source) => source.find((x) => x.udi === udi));
	}

	createBlock(layoutEntry: BlockLayoutType, contentElementTypeKey: string) {
		// Find block type.
		const blockType = this.#blockTypes.value.find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		this.#layouts.appendOne(layoutEntry);

		// Create content entry:
		if (layoutEntry.contentUdi) {
			this.#contents.appendOne({
				contentTypeKey: contentElementTypeKey,
				udi: layoutEntry.contentUdi,
			});
		} else {
			throw new Error('Cannot create block, missing contentUdi');
		}

		//Create settings entry:
		if (blockType.settingsElementTypeKey) {
			if (layoutEntry.settingsUdi) {
				this.#contents.appendOne({
					contentTypeKey: blockType.settingsElementTypeKey,
					udi: layoutEntry.settingsUdi,
				});
			} else {
				throw new Error('Cannot create block, missing settingsUdi');
			}
		}
	}

	deleteBlock(contentUdi: string) {
		const layout = this.#layouts.value.find((x) => x.contentUdi === contentUdi);
		if (!layout) return;

		if (layout.settingsUdi) {
			this.#settings.removeOne(layout.settingsUdi);
		}

		this.#layouts.removeOne(contentUdi);
		this.#contents.removeOne(contentUdi);
	}
}

export const UMB_BLOCK_MANAGER_CONTEXT = new UmbContextToken<UmbBlockManagerContext, UmbBlockManagerContext>(
	'UmbBlockManagerContext',
);
