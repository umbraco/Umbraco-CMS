import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export const defaultA11yConfig = {
	ignoredRules: [],
};

export type UmbTestRunnerWindow = Window &
	typeof globalThis & {
		__UMBRACO_TEST_RUN_A11Y_TEST: boolean;
	};

/**
 * Swallows "ResizeObserver loop completed with undelivered notifications" errors for the duration of a
 * test. UUI components that measure themselves — `uui-tab-group` observes itself and every tab — provoke
 * these, and the test runner turns any uncaught error into a failure for every test in the file.
 * @returns {() => void} Restores the previous handler.
 */
export function ignoreResizeObserverLoopErrors(): () => void {
	const previousHandler = window.onerror;

	window.onerror = (event, source, lineno, colno, error) => {
		if (typeof event === 'string' && event.includes('ResizeObserver loop')) return true;
		return previousHandler?.call(window, event, source, lineno, colno, error) ?? false;
	};

	return () => {
		window.onerror = previousHandler;
	};
}

/**
 * Registers an item repository that answers every requested unique with `createItem(unique)`, so a picker
 * input can render its selection without a server or an item store.
 * @param {string} alias - The repository alias the picker input resolves.
 * @param {(unique: string) => { unique: string }} createItem - Builds the item returned for a unique.
 * @returns {() => void} Unregisters the repository.
 */
export function registerTestItemRepository(alias: string, createItem: (unique: string) => { unique: string }) {
	umbExtensionsRegistry.register({
		type: 'repository',
		alias,
		name: `Test Item Repository: ${alias}`,
		api: class extends UmbControllerBase {
			async requestItems(uniques: Array<string>) {
				const data = uniques.map(createItem);
				return { data, asObservable: () => of(data) };
			}
		},
	});
	return () => umbExtensionsRegistry.unregister(alias);
}

/**
 * Provides a modal manager to the descendants of `host` that resolves every modal it opens with `value`.
 * @param {HTMLElement} host - The element whose descendants open modals.
 * @param {unknown} value - The value each opened modal submits.
 * @returns {Array<unknown>} The data each opened modal was given, in the order they were opened.
 */
export function provideTestModalManager(host: HTMLElement, value: unknown): Array<unknown> {
	const opened: Array<unknown> = [];
	const modalManager = {
		getHostElement: () => host,
		open: (_host: unknown, _modalAlias: unknown, args?: { data?: unknown }) => {
			opened.push(args?.data);
			return { onSubmit: () => Promise.resolve(value) };
		},
	};
	new UmbContextProvider(host, UMB_MODAL_MANAGER_CONTEXT, modalManager as never).hostConnected();
	return opened;
}
