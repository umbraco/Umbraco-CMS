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
 * Reads the blocks rendered by `<umb-grouped-action-list>` as a sequence: '[label' for a group, '[Other' for the
 * named block of actions without a group or '[' when that block has no name, then the actions with '|' marking a line
 * inside the block, then ']'.
 * @param {Element | null | undefined} list - The `<umb-grouped-action-list>` element.
 * @param {(action: Element) => string} readAction - Reads one action element.
 * @returns {Array<string>} The sequence.
 */
export function readGroupedActionList(list: Element | null | undefined, readAction: (action: Element) => string) {
	const readActions = (children: Iterable<Element>): Array<string> =>
		Array.from(children).flatMap((child, index) =>
			child.classList.contains('segment')
				? [...(index > 0 ? ['|'] : []), ...readActions(child.children)]
				: [readAction(child)],
		);
	const readName = (block: HTMLElement) => {
		if (block.getAttribute('role') !== 'group') return '';
		return block.dataset.mark ? block.getAttribute('aria-label') : 'Other';
	};
	return Array.from(list?.shadowRoot?.querySelectorAll<HTMLElement>('.block') ?? []).flatMap((block) => [
		`[${readName(block)}`,
		...readActions(block.children),
		']',
	]);
}
