import type { UmbPropertyActionArgs } from './types.js';
import type { UmbPropertyAction } from './property-action.interface.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

/**
 * Base class for an property action.
 * @abstract
 * @class UmbPropertyActionBase
 * @augments {UmbActionBase}
 * @implements {UmbPropertyAction}
 */
export abstract class UmbPropertyActionBase<ArgsMetaType = never>
	extends UmbActionBase<UmbPropertyActionArgs<ArgsMetaType>>
	implements UmbPropertyAction<ArgsMetaType>
{
	/**
	 * By specifying the href, the action will act as a link.
	 * The `execute` method will not be called.
	 * @abstract
	 * @returns {string | undefined}
	 */
	public getHref(): Promise<string | undefined> {
		return Promise.resolve(undefined);
	}

	/**
	 * By specifying the `execute` method, the action will act as a button.
	 * @abstract
	 * @returns {Promise<void>}
	 */
	public execute(): Promise<void> {
		return Promise.resolve();
	}
}
