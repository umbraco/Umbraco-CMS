import type { UmbEntityActionArgs } from './types.js';
import type { UmbEntityAction } from './entity-action.interface.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

/**
 * Base class for an entity action.
 * @abstract
 * @class UmbEntityActionBase
 * @augments {UmbActionBase}
 * @implements {UmbEntityAction}
 * @template RepositoryType
 */
export abstract class UmbEntityActionBase<ArgsMetaType>
	extends UmbActionBase<UmbEntityActionArgs<ArgsMetaType>>
	implements UmbEntityAction<ArgsMetaType>
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
