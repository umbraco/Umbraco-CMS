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

	protected _isPending = new UmbBooleanState(false);
	public isPending = this._isPending.asObservable();

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

	/**
	 * Signals whether the action is currently performing real work. Subclasses
	 * should call this with `true` once any preceding modal has been confirmed
	 * and the actual work is about to begin, and reset to `false` at the start
	 * of the next `execute()` call.
	 * @param {boolean} value - `true` when real work has started; `false` otherwise.
	 * @memberof UmbWorkspaceActionBase
	 */
	protected setPending(value: boolean): void {
		this._isPending.setValue(value);
	}
}
