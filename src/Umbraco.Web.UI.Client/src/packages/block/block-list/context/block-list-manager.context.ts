import type { UmbBlockListLayoutModel, UmbBlockListTypeModel } from '../types.js';
import type { UmbBlockListWorkspaceOriginData } from '../index.js';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Block List Editor.
 */
export class UmbBlockListManagerContext<
	BlockLayoutType extends UmbBlockListLayoutModel = UmbBlockListLayoutModel,
> extends UmbBlockManagerContext<UmbBlockListTypeModel, BlockLayoutType, UmbBlockListWorkspaceOriginData> {}
