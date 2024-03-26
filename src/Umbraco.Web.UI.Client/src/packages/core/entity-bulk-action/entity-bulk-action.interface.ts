import type { UmbEntityBulkActionArgs } from './types.js';
import type { MetaEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

export interface UmbEntityBulkAction<ArgsMetaType extends MetaEntityBulkAction>
	extends UmbAction<UmbEntityBulkActionArgs<ArgsMetaType>> {
	selection: Array<string>;

	// I don't think we need this one, now that we have the above one? [NL]
	//setSelection(selection: Array<string>): void;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}
