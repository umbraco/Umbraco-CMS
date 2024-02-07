import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel } from '../types.js';
import type { UmbBlockManagerContext } from './block-manager.context.js';
import { UMB_BLOCK_ENTRIES_CONTEXT } from './block-entries.context-token.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export abstract class UmbBlockEntriesContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<
	UmbBlockEntriesContext<BlockManagerContextTokenType, BlockManagerContextType, BlockType, BlockLayoutType>
> {
	//
	_manager?: BlockManagerContextType;

	#layoutEntries = new UmbArrayState<BlockLayoutType>([], (x) => x.contentUdi);
	layoutEntries = this.#layoutEntries.asObservable();

	setLayoutEntries(layoutEntries: Array<BlockLayoutType>) {
		this.#layoutEntries.setValue(layoutEntries);
	}
	getLayoutEntries() {
		return this.#layoutEntries.value;
	}

	constructor(host: UmbControllerHost, blockManagerContextToken: BlockManagerContextTokenType) {
		super(host, UMB_BLOCK_ENTRIES_CONTEXT.toString());

		// TODO: Observe Blocks of the layout entries of this component.
		this.consumeContext(blockManagerContextToken, (blockGridManager) => {
			this._manager = blockGridManager;
			this._gotBlockManager();
		});
	}

	protected abstract _gotBlockManager(): void;

	// Public methods:

	public abstract getPathForCreateBlock(index: number): string | undefined;
	public abstract getPathForClipboard(index: number): string | undefined;

	//edit?
	//editSettings
	//requestDelete
	//delete
	// - should recursively delete all children of areas and their content/settings.
	//copy
}
