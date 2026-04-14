import { UMB_CURRENT_USER_HISTORY_STORE_CONTEXT } from './current-user-history.store.token.js';
import { umbCurrentViewTitle } from '@umbraco-cms/backoffice/view';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

/** @deprecated No longer used internally. This will be removed in Umbraco 19. [LK] */
export type UmbModelType = 'dialog' | 'sidebar';

export type UmbCurrentUserHistoryItem = {
	unique: string;
	path: string;
	/** @deprecated `label` type will be changed to `string` only in Umbraco 19. [LK] */
	label: string | Array<string>;
	/**
	 * Optional icon name (e.g. `icon-document-js`) attached to the entry by the
	 * active workspace via its title chain. When absent the consumer should fall
	 * back to a generic icon (e.g. `icon-link`).
	 */
	icon?: string;
	displayPath?: string;
};

const BREADCRUMB_SEPARATOR = ' › ';

export class UmbCurrentUserHistoryStore extends UmbStoreBase<UmbCurrentUserHistoryItem> {
	public readonly history = this._data.asObservable();

	/**
	 * Returns the latest 100 history items.
	 */
	public readonly latestHistory = this._data.asObservablePart((historyItems) => historyItems.slice(-100));

	#lastAddedUnique: string | null = null;
	#lastAddedPath: string | null = null;

	#handleNavigate = (event: any) => {
		const url = new URL(event.destination.url);
		// `path` stores the full (unnormalized) pathname so that clicking the
		// entry navigates back to the specific tab or culture variant.
		const fullPath = url.pathname;
		const normalizedPath = this.#normalizePath(fullPath);

		// Before adding a new item, finalize the state of the previous item.
		// If it ended in a GUID and never received a title, drop it.
		this.#removeUnresolvedGuidEntry();

		const unique = UmbId.new();
		const historyItem: UmbCurrentUserHistoryItem = {
			unique,
			path: fullPath,
			label: this.#extractLabelFromPath(normalizedPath),
			displayPath: undefined,
		};
		const wasAdded = this.#pushIfNew(historyItem);
		if (wasAdded) {
			this.#lastAddedUnique = unique;
			this.#lastAddedPath = normalizedPath;
		}
	};

	constructor(host: UmbControllerHost) {
		super(
			host,
			UMB_CURRENT_USER_HISTORY_STORE_CONTEXT.toString(),
			new UmbArrayState<UmbCurrentUserHistoryItem>([], (x) => x.unique),
		);
		if (!('navigation' in window)) return;
		(window as any).navigation.addEventListener('navigate', this.#handleNavigate);

		// Mirror the active view's structured title onto the latest history entry.
		// Skip modal-kind segments (modals don't change the URL — without this, an
		// opened modal would rewrite the underlying page's entry) and tab-kind
		// segments (which sub-view the user was on is noise in a breadcrumb; the
		// full URL is still preserved on `path` for click-through).
		this.observe(umbCurrentViewTitle, (view) => {
			if (!view || !this.#lastAddedUnique || !this.#lastAddedPath) return;
			// Guard against stale emissions during fast navigation.
			if (this.#normalizePath(view.path) !== this.#lastAddedPath) return;
			if (view.segments.some((s) => s.kind === 'modal')) return;

			const breadcrumb = view.segments.filter((s) => s.kind !== 'tab');
			if (!breadcrumb.length) return;

			const leaf = breadcrumb[breadcrumb.length - 1];
			const parents = breadcrumb.slice(0, -1);
			this.updateItem(this.#lastAddedUnique, {
				label: leaf.label,
				icon: leaf.icon,
				displayPath: parents.length ? parents.map((s) => s.label).join(BREADCRUMB_SEPARATOR) : undefined,
			});
		});
	}

	/**
	 * Normalize a path by stripping sub-route suffixes so that internal workspace
	 * navigation (switching tabs, variants, sub-views) doesn't create separate
	 * history entries. The first regex handles `/invariant`, `/root`, `/<locale>`,
	 * `/view`, and `/tab` segments (each possibly followed by more path), the
	 * second collapses a trailing `/collection` into the parent path.
	 * @param path
	 */
	#normalizePath(path: string): string {
		return path
			.replace(/\/(?:invariant|root|[a-z]{2}-[a-z]{2}|view|tab)\b.*$/i, '')
			.replace(/\/collection$/, '');
	}

	/**
	 * Remove the last added entry if it ends in a GUID and the label wasn't resolved from the active view's title.
	 * This cleans up entries where the user navigated away before a workspace had time to publish its title.
	 */
	#removeUnresolvedGuidEntry(): void {
		if (!this.#lastAddedUnique || !this.#lastAddedPath) return;

		const lastSegment = this.#getLastPathSegment(this.#lastAddedPath);
		if (!UmbId.validate(lastSegment)) return;

		const items = this._data.getValue();
		const item = items.find((i) => i.unique === this.#lastAddedUnique);
		if (!item || item.label !== this.#extractLabelFromPath(this.#lastAddedPath)) return;

		this._data.setValue(items.filter((i) => i.unique !== this.#lastAddedUnique));
	}

	/**
	 * Get the last segment of a path.
	 * @param path
	 */
	#getLastPathSegment(path: string): string {
		return path.split('/').filter(Boolean).pop() ?? '';
	}

	/**
	 * Extract a readable label from a path.
	 * Tries to get the last meaningful segment (usually entity ID or section name).
	 * This is a transient fallback — the real title arrives via `umbCurrentViewTitle`
	 * once the workspace publishes its segments.
	 * @param path
	 */
	#extractLabelFromPath(path: string): string {
		return this.#formatSegmentAsLabel(this.#getLastPathSegment(path));
	}

	/**
	 * Format a URL segment as a readable label: converts kebab-case to Title Case,
	 * pluralises the `-root` / `-management` tree-root conventions (e.g.
	 * `user-root` → `Users`), and handles a handful of known identifiers.
	 * @param segment
	 */
	#formatSegmentAsLabel(segment: string): string {
		if (!segment) return '';
		if (segment === 'dashboardTabsContentIntro') return 'Welcome to Umbraco';
		const rootMatch = segment.match(/^(.+)-(?:root|management)$/);
		const base = rootMatch ? rootMatch[1] : segment;
		const titleCased = base
			.split('-')
			.map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
			.join(' ');
		return rootMatch ? titleCased + 's' : titleCased;
	}

	/**
	 * Pushes a new history item, removing any existing item with the same normalized path.
	 * Paths are compared after normalization so that tab / variant switches within the
	 * same entity are treated as the same entry — but the full path (including the
	 * current tab) is preserved on the item so clicking it returns the user to the
	 * exact place they left.
	 * @param historyItem
	 * @returns true if the item was added, false if it was an immediate duplicate of the last item
	 */
	#pushIfNew(historyItem: UmbCurrentUserHistoryItem): boolean {
		const history = this._data.getValue();
		const lastItem = history[history.length - 1];
		const incomingNormalized = this.#normalizePath(historyItem.path);

		// Skip if this is the same (normalized) as the last item — but still bump the
		// stored path so the entry reflects the most recently visited tab / variant.
		if (lastItem && this.#normalizePath(lastItem.path) === incomingNormalized) {
			if (lastItem.path !== historyItem.path) {
				this.updateItem(lastItem.unique, { path: historyItem.path });
			}
			return false;
		}

		// Remove any earlier entry with the same normalized path (de-duplicate).
		const filteredHistory = history.filter((item) => this.#normalizePath(item.path) !== incomingNormalized);

		this._data.setValue([...filteredHistory, historyItem]);
		return true;
	}

	/**
	 * Pushes a new history item to the history array
	 * @public
	 * @param {UmbCurrentUserHistoryItem} historyItem
	 * @memberof UmbHistoryService
	 */
	public push(historyItem: UmbCurrentUserHistoryItem): void {
		this.#pushIfNew(historyItem);
	}

	/**
	 * Clears the history array
	 * @public
	 * @memberof UmbHistoryService
	 */
	public clear() {
		this._data.setValue([]);
		this.#lastAddedUnique = null;
		this.#lastAddedPath = null;
	}

	override destroy(): void {
		if ('navigation' in window) {
			(window as any).navigation.removeEventListener('navigate', this.#handleNavigate);
		}
		super.destroy();
	}
}

// Default export for the globalContext manifest:
export default UmbCurrentUserHistoryStore;
