import type { UmbEntityBulkAction } from './entity-bulk-action.interface.js';
import type { UmbEntityBulkActionArgs } from './types.js';
import type { MetaEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

export abstract class UmbEntityBulkActionBase<ArgsMetaType extends MetaEntityBulkAction>
	extends UmbActionBase<UmbEntityBulkActionArgs<ArgsMetaType>>
	implements UmbEntityBulkAction<ArgsMetaType>
{
	selection: Array<string> = [];

	abstract override execute(): Promise<void>;
}
