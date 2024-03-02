import type { UmbEntityActionArgs } from './types.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Base class for an entity action.
 * @export
 * @abstract
 * @class UmbEntityActionBase
 * @extends {UmbActionBase}
 * @implements {UmbEntityAction}
 * @template RepositoryType
 */
export abstract class UmbEntityActionBase<ArgsMetaType> extends UmbActionBase<UmbEntityActionArgs<ArgsMetaType>> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<ArgsMetaType>) {
		super(host, args);
	}

	/**
	 * By specifying the href, the action will act as a link.
	 * The `execute` method will not be called.
	 * @abstract
	 * @returns {string | null | undefined}
	 */
	public getHref(): Promise<string | null | undefined> {
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
