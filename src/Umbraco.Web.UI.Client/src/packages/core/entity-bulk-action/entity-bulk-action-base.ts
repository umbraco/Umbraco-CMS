import type { UmbEntityBulkAction } from './entity-bulk-action.interface.js';
import type { UmbEntityBulkActionArgs } from './types.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

export abstract class UmbEntityBulkActionBase<ArgsMetaType = unknown>
	extends UmbActionBase<UmbEntityBulkActionArgs<ArgsMetaType>>
	implements UmbEntityBulkAction<ArgsMetaType>
{
	selection: Array<string> = [];

	abstract execute(): Promise<void>;
}
