import { html, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';

/**
 * Try to load a custom view.
 * Supports both HTML and JS files (JS files must have a default export).
 *
 * @param view The path to the custom view.
 * @returns A view part.
 */
export async function loadCustomView<T extends HTMLElement>(view: string): Promise<T | string> {
	if (view.endsWith('.html')) {
		return fetch(view).then((response) => response.text());
	}

	const module = await import(/* @vite-ignore */ view);

	if (!module.default) throw new Error('No default export found');

	return new module.default() as T;
}

export function renderCustomView<T extends HTMLElement>(view: T | string) {
	if (typeof view === 'string') {
		return html`${unsafeHTML(view)}`;
	}

	return view;
}
