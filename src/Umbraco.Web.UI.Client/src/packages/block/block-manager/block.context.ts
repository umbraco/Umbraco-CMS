import type { UmbBlockTypeBase } from '../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockContext<
	BlockType extends UmbBlockTypeBase = UmbBlockTypeBase,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<UmbBlockContext> {
	//
	#config = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly config = this.#config.asObservable();

	#layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this.#layout.asObservable();

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly content = this.#content.asObservable();

	#setting = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly setting = this.#setting.asObservable();

	// Public properties:
	public readonly contentUdi: string;

	constructor(host: UmbControllerHost, contentUdi) {
		super(host, UMB_BLOCK_CONTEXT);
		this.contentUdi = contentUdi;

		// Consume block manager?
		// observe data.
	}
}

export const UMB_BLOCK_CONTEXT = new UmbContextToken<UmbBlockContext, UmbBlockContext>('UmbBlockContext');
