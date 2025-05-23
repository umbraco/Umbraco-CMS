import type { UmbWorkspaceActionMenuItemArgs } from './types.js';
import type { UmbWorkspaceActionMenuItem } from './workspace-action-menu-item.interface.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

/**
 * Base class for an workspace action.
 * @abstract
 * @class UmbWorkspaceActionMenuItemBase
 * @augments {UmbActionBase}
 * @implements {UmbWorkspaceActionMenuItem}
 * @template RepositoryType
 */
export abstract class UmbWorkspaceActionMenuItemBase<ArgsMetaType = never>
	extends UmbActionBase<UmbWorkspaceActionMenuItemArgs<ArgsMetaType>>
	implements UmbWorkspaceActionMenuItem<ArgsMetaType>
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
