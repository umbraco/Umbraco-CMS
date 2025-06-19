import type { UmbEntityCreateOptionActionArgs } from './types.js';
import type { UmbEntityCreateOptionAction } from './entity-create-option-action.interface.js';
import type { MetaEntityCreateOptionAction } from './entity-create-option-action.extension.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

export abstract class UmbEntityCreateOptionActionBase<
		ArgsMetaType extends MetaEntityCreateOptionAction = MetaEntityCreateOptionAction,
	>
	extends UmbActionBase<UmbEntityCreateOptionActionArgs<ArgsMetaType>>
	implements UmbEntityCreateOptionAction<ArgsMetaType>
{
	/**
	 * By specifying the href, the action will act as a link.
	 * The `execute` method will not be called.
	 * @abstract
	 * @returns {string | undefined} - A promise which resolves into a HREF string or undefined.
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
