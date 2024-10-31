import type { MetaEntityCreateOptionAction } from './entity-create-option-action.extension.js';
import type { UmbEntityCreateOptionActionArgs } from './types.js';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

export interface UmbEntityCreateOptionAction<
	ArgsMetaType extends MetaEntityCreateOptionAction = MetaEntityCreateOptionAction,
> extends UmbAction<UmbEntityCreateOptionActionArgs<ArgsMetaType>> {
	/**
	 * The href location, the action will act as a link.
	 * @returns {Promise<string | undefined>}
	 */
	getHref(): Promise<string | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}
