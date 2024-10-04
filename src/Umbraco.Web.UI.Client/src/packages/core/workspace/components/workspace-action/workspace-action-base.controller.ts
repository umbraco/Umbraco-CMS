import type { UmbWorkspaceActionArgs } from './types.js';
import type { UmbWorkspaceAction } from './workspace-action.interface.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

/**
 * Base class for an workspace action.
 * @abstract
 * @class UmbWorkspaceActionBase
 * @augments {UmbActionBase}
 * @implements {UmbEntityAction}
 * @template RepositoryType
 */
export abstract class UmbWorkspaceActionBase<ArgsMetaType = never>
	extends UmbActionBase<UmbWorkspaceActionArgs<ArgsMetaType>>
	implements UmbWorkspaceAction<ArgsMetaType>
{
	protected _isDisabled = new UmbBooleanState(false);
	public isDisabled = this._isDisabled.asObservable();

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

	/**
	 * Disables the action.
	 * @memberof UmbWorkspaceActionBase
	 */
	public disable(): void {
		this._isDisabled.setValue(true);
	}

	/**
	 * Enables the action.
	 * @memberof UmbWorkspaceActionBase
	 */
	public enable(): void {
		this._isDisabled.setValue(false);
	}
}
