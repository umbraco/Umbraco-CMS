import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDrawerManagerContext extends UmbContextBase {
	#current = new UmbBasicState<string | undefined>(undefined);
	public readonly current = this.#current.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DRAWER_MANAGER_CONTEXT);
	}

	/**
	 * Opens the drawer for the given drawerApp alias.
	 * Replaces any drawer that is currently open.
	 */
	public open(alias: string): void {
		this.#current.setValue(alias);
	}

	/**
	 * Closes the currently open drawer (no-op if none is open).
	 */
	public close(): void {
		this.#current.setValue(undefined);
	}
}

export const UMB_DRAWER_MANAGER_CONTEXT = new UmbContextToken<UmbDrawerManagerContext>('UmbDrawerManagerContext');
