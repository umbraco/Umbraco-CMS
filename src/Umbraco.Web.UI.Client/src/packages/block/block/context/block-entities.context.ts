import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel } from '../types.js';
import type { UMB_BLOCK_MANAGER_CONTEXT, UmbBlockManagerContext } from '../manager/index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbBlockContext<
	BlockManagerContextTokenType extends UmbContextToken<BlockManagerContextType, BlockManagerContextType>,
	BlockManagerContextType extends UmbBlockManagerContext<BlockType, BlockLayoutType>,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<
	UmbBlockContext<BlockManagerContextTokenType, BlockManagerContextType, BlockType, BlockLayoutType>
> {
	//
	_manager?: BlockManagerContextType;

	constructor(host: UmbControllerHost, blockManagerContextToken: BlockManagerContextTokenType) {
		super(host, UMB_BLOCK_ENTITY_CONTEXT.toString());
	}

	// Public methods:

	//edit?
	//editSettings
	//requestDelete
	//delete
	//copy
}

export const UMB_BLOCK_ENTITY_CONTEXT = new UmbContextToken<
	UmbBlockContext<typeof UMB_BLOCK_MANAGER_CONTEXT, typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>
>('UmbBlockEntryContext');
