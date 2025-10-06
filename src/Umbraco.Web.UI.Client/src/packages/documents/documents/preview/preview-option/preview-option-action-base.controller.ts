import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

/**
 * Base class for a preview option action.
 * @abstract
 * @class UmbPreviewOptionActionBase
 * @augments {UmbActionBase}
 */
export abstract class UmbPreviewOptionActionBase<ArgsMetaType = never> extends UmbActionBase<UmbAction<ArgsMetaType>> {
	/**
	 * By specifying the `execute` method, the action will act as a button.
	 * @abstract
	 * @returns {Promise<void>}
	 */
	public execute(): Promise<void> {
		return Promise.resolve();
	}
}
