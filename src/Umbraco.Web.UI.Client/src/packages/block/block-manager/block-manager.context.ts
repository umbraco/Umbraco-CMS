import type { UmbBlockTypeBase } from '../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockManagerContext<
	BlockType extends UmbBlockTypeBase = UmbBlockTypeBase,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<UmbBlockManagerContext> {
	//
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

	blockTypeOf(contentElementTypeKey: string) {
		return this.#blockTypes.asObservablePart((source) =>
			source.find((x) => x.contentElementTypeKey === contentElementTypeKey),
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
}

export const UMB_BLOCK_MANAGER_CONTEXT = new UmbContextToken<UmbBlockManagerContext, UmbBlockManagerContext>(
	'UmbBlockManagerContext',
);
