import { UMB_CURRENT_USER_HISTORY_STORE_CONTEXT } from './current-user-history.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

export type UmbCurrentUserHistoryItem = {
	unique: string;
	path: string;
	displayPath: string;
	label: string;
};

export class UmbCurrentUserHistoryStore extends UmbStoreBase<UmbCurrentUserHistoryItem> {
	public readonly history = this._data.asObservable();

	/**
	 * Returns the latest 100 history items.
	 */
	public readonly latestHistory = this._data.asObservablePart((historyItems) => historyItems.slice(-100));

	#lastAddedUnique: string | null = null;
	#lastAddedPath: string | null = null;
	#titleObserver: MutationObserver | null = null;
	#headObserver: MutationObserver | null = null;
	#titleUpdateTimer: ReturnType<typeof setTimeout> | null = null;

	#handleNavigateSuccess = () => {
		// Update label for the most recent history item after navigation completes.
		this.#scheduleTitleUpdate();
	};

	#handleNavigate = (event: any) => {
		const url = new URL(event.destination.url);
		let path = url.pathname;

		// Skip sub-routes (variant paths, view paths, tab paths)
		// These are internal navigation within a workspace, not new entity navigations.
		if (this.#isSubRoute(path)) {
			return;
		}

		// Special case: we have some workspaces that default to a collection, so we strip the
		// trailing /collection to de-duplicate with parent. This avoids two entries in the history.
		path = path.replace(/\/collection$/, '');

		// Before adding a new item, finalize the label of the previous item
		// using the current document title (before it changes).
		this.#finalizeCurrentLabel();

		const unique = UmbId.new();
		const historyItem = {
			unique,
			path,
			displayPath: this.#formatDisplayPath(path),
			label: this.#extractLabelFromPath(path),
		};
		const wasAdded = this.#pushIfNew(historyItem);
		if (wasAdded) {
			this.#lastAddedUnique = unique;
			this.#lastAddedPath = path;
		}
	};

	constructor(host: UmbControllerHost) {
		super(
			host,
			UMB_CURRENT_USER_HISTORY_STORE_CONTEXT.toString(),
			new UmbArrayState<UmbCurrentUserHistoryItem>([], (x) => x.unique),
		);
		if (!('navigation' in window)) return;
		(window as any).navigation.addEventListener('navigatesuccess', this.#handleNavigateSuccess);
		(window as any).navigation.addEventListener('navigate', this.#handleNavigate);

		this.#setupTitleObserver();
	}

	/**
	 * Check if a path is a sub-route that should not create a new history entry.
	 * Sub-routes include variant paths (/invariant, /en-us), view paths, and tab paths.
	 */
	#isSubRoute(path: string): boolean {
		// Common sub-route patterns that shouldn't create new history entries
		const subRoutePatterns = [
			/\/invariant(\/|$)/, // Variant: invariant culture
			/\/[a-z]{2}-[a-z]{2}(\/|$)/i, // Variant: culture codes like en-us, da-dk
			/\/view\//, // View sub-routes
			/\/tab\//, // Tab sub-routes
			/\/root(\/|$)/, // Root tab
		];

		return subRoutePatterns.some((pattern) => pattern.test(path));
	}

	/**
	 * Finalize the current item's label before navigating away.
	 * If the path ends in a GUID and the title wasn't resolved in time, remove the entry.
	 */
	#finalizeCurrentLabel(): void {
		if (this.#titleUpdateTimer) {
			clearTimeout(this.#titleUpdateTimer);
			this.#titleUpdateTimer = null;
		}

		this.#removeUnresolvedGuidEntry();
	}

	/**
	 * Remove the last added entry if it ends in a GUID and the label wasn't resolved from the title.
	 * This cleans up entries where the user navigated away too quickly.
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
	 */
	#getLastPathSegment(path: string): string {
		return path.split('/').filter(Boolean).pop() ?? '';
	}

	/**
	 * Extract a readable label from a path.
	 * Tries to get the last meaningful segment (usually entity ID or section name).
	 */
	#extractLabelFromPath(path: string): string {
		return this.#formatSegmentAsLabel(this.#getLastPathSegment(path));
	}

	/**
	 * Format a URL segment as a readable label.
	 * Handles special suffixes and converts kebab-case to Title Case.
	 */
	#formatSegmentAsLabel(segment: string): string {
		if (!segment) return '';

		// Special cases: Handle specific common and known segments to provide something more user-friendly.
		if (segment === 'dashboardTabsContentIntro') {
			return 'Welcome to Umbraco';
		}

		// Special cases: Handle known and common suffixes: "user-root" -> "Users", "member-management" -> "Members"
		if (segment.endsWith('-root') || segment.endsWith('-management')) {
			const base = segment.replace(/-(root|management)$/, '');
			const titleCased = this.#toTitleCase(base);
			return titleCased + 's';
		}

		// Convert kebab-case to Title Case for better display
		// e.g., "models-builder" -> "Models Builder"
		return this.#toTitleCase(segment);
	}

	/**
	 * Convert a kebab-case string to Title Case.
	 * e.g., "models-builder" -> "Models Builder"
	 */
	#toTitleCase(str: string): string {
		if (!str) return '';
		return str
			.split('-')
			.map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
			.join(' ');
	}

	/**
	 * Format a path for display by stripping common prefixes.
	 * e.g., "/umbraco/section/content/..." -> "content/..."
	 */
	#formatDisplayPath(path: string): string {
		return path.replace(/^\/(umbraco\/)?(section\/)?/, '');
	}

	#setupTitleObserver(): void {
		const titleElement = document.querySelector('title');
		if (titleElement) {
			this.#titleObserver = new MutationObserver(() => this.#onTitleChange());
			this.#titleObserver.observe(titleElement, { childList: true, characterData: true, subtree: true });
		} else {
			// If title element doesn't exist yet, observe head for its creation.
			this.#headObserver = new MutationObserver(() => {
				const title = document.querySelector('title');
				if (title) {
					this.#headObserver?.disconnect();
					this.#headObserver = null;
					this.#titleObserver = new MutationObserver(() => this.#onTitleChange());
					this.#titleObserver.observe(title, { childList: true, characterData: true, subtree: true });
				}
			});
			this.#headObserver.observe(document.head, { childList: true });
		}
	}

	#onTitleChange(): void {
		this.#scheduleTitleUpdate();
	}

	#scheduleTitleUpdate(): void {
		if (this.#titleUpdateTimer) {
			clearTimeout(this.#titleUpdateTimer);
		}
		this.#titleUpdateTimer = setTimeout(() => {
			this.#updateLatestLabel();
		}, 150);
	}

	#updateLatestLabel(): void {
		if (!this.#lastAddedUnique || !this.#lastAddedPath) return;

		const title = document.title;
		if (!title) return;

		// Extract friendly title by removing " | Umbraco" suffix.
		let friendlyTitle = title.replace(/ \| Umbraco$/, '');
		if (!friendlyTitle) return;

		// Take only the first part if there are multiple pipe-separated segments
		// e.g., "My Blog Post | Content" -> "My Blog Post".
		const pipeIndex = friendlyTitle.indexOf(' | ');
		if (pipeIndex > 0) {
			friendlyTitle = friendlyTitle.substring(0, pipeIndex);
		}

		// Skip generic/technical labels that aren't useful as entity names.
		const skipLabels = ['invariant', 'content', 'media', 'settings', 'users', 'packages', 'members', 'user'];
		if (skipLabels.includes(friendlyTitle.toLowerCase())) {
			return;
		}

		// Update the item in the store for immediate display
		this.updateItem(this.#lastAddedUnique, { label: friendlyTitle });
	}

	/**
	 * Pushes a new history item, removing any existing item with the same path.
	 * @returns true if the item was added, false if it was a duplicate of the last item
	 */
	#pushIfNew(historyItem: UmbCurrentUserHistoryItem): boolean {
		const history = this._data.getValue();
		const lastItem = history[history.length - 1];

		// Skip if this is the same as the last item (immediate duplicate).
		if (lastItem && lastItem.path === historyItem.path) {
			return false;
		}

		// Remove any earlier entry with the same path (de-duplicate).
		const filteredHistory = history.filter((item) => item.path !== historyItem.path);

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
			(window as any).navigation.removeEventListener('navigatesuccess', this.#handleNavigateSuccess);
			(window as any).navigation.removeEventListener('navigate', this.#handleNavigate);
		}
		if (this.#headObserver) {
			this.#headObserver.disconnect();
			this.#headObserver = null;
		}
		if (this.#titleObserver) {
			this.#titleObserver.disconnect();
			this.#titleObserver = null;
		}
		if (this.#titleUpdateTimer) {
			clearTimeout(this.#titleUpdateTimer);
			this.#titleUpdateTimer = null;
		}
		super.destroy();
	}
}

// Default export for the globalContext manifest:
export default UmbCurrentUserHistoryStore;
