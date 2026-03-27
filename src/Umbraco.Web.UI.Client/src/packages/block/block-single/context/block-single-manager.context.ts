import type { UmbBlockSingleLayoutModel, UmbBlockSingleTypeModel } from '../types.js';
import type { UmbBlockSingleWorkspaceOriginData } from '../index.js';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Block Single Editor.
 */
export class UmbBlockSingleManagerContext<
	BlockLayoutType extends UmbBlockSingleLayoutModel = UmbBlockSingleLayoutModel,
> extends UmbBlockManagerContext<UmbBlockSingleTypeModel, BlockLayoutType, UmbBlockSingleWorkspaceOriginData> {}
