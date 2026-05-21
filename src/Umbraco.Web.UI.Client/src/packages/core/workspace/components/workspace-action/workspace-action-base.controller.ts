import type { UmbWorkspaceActionArgs } from './types.js';
import type { UmbWorkspaceAction } from './workspace-action.interface.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

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

	protected _isExecuting?: UmbBooleanState<boolean>;
	public isExecuting?: Observable<boolean>;

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
	 * Signals whether `execute()` is currently performing real work. Subclasses
	 * that opt in to modal-aware execution feedback should:
	 *
	 * 1. Call `this.setExecuting(false)` in their constructor so `isExecuting`
	 *    is exposed before the workspace-action element observes it.
	 * 2. Call `this.setExecuting(true)` once any preceding modal has been
	 *    confirmed and real work is about to begin.
	 * 3. Wrap the work in `try/finally` and call `this.setExecuting(false)` on
	 *    completion so the observable honours its contract.
	 *
	 * Subclasses that never call this method keep `isExecuting` undefined,
	 * which signals consumers to fall back to legacy eager-feedback behaviour.
	 * @param {boolean} value - `true` while real work is in flight; `false` otherwise.
	 * @memberof UmbWorkspaceActionBase
	 */
	protected setExecuting(value: boolean): void {
		if (!this._isExecuting) {
			this._isExecuting = new UmbBooleanState(false);
			this.isExecuting = this._isExecuting.asObservable();
		}
		this._isExecuting.setValue(value);
	}
}
