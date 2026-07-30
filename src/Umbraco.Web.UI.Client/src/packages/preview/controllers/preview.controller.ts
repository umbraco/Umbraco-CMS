import { UmbPreviewRepository } from '../repository/preview.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbPreviewControllerArgs {
	urlProviderAlias: string;
	unique: string;
	culture?: string | null;
	segment?: string | null;
}

export class UmbPreviewController extends UmbControllerBase {
	#previewWindow: WindowProxy | null = null;
	#previewWindowKey: string | null = null;
	#previewIsExternal = false;

	#previewRepository: UmbPreviewRepository;
	#localize = new UmbLocalizationController(this);

	/**
	 * Creates a new {@link UmbPreviewController}.
	 * @param {UmbControllerHost} host - The controller host.
	 * @param {UmbPreviewRepository} [previewRepository] - Optional repository override; defaults to a new
	 * {@link UmbPreviewRepository}. Intended for testing.
	 */
	constructor(host: UmbControllerHost, previewRepository?: UmbPreviewRepository) {
		super(host);
		this.#previewRepository = previewRepository ?? new UmbPreviewRepository(this);
	}

	/**
	 * Opens a preview window for the given document, reusing an existing window when one is already
	 * open for the same document and URL provider. If no preview URL can be resolved, an error notification
	 * is shown and an error is thrown.
	 * @param {UmbPreviewControllerArgs} args - The preview arguments.
	 * @param {string} args.urlProviderAlias - The alias of the URL provider used to resolve the preview URL.
	 * @param {string} args.unique - The unique identifier of the document to preview.
	 * @param {string | null | undefined} args.culture - The culture to preview, or null/undefined for the default culture.
	 * @param {string | null | undefined} args.segment - The segment to preview, or null/undefined for no segment.
	 * @param {WindowProxy | null} [previewWindow] - A window opened synchronously during the user gesture. Safari only
	 * allows `window.open` from within the gesture's synchronous call stack, and resolving the preview URL takes
	 * several awaits, so callers reached from a click must open the tab up front and hand it over here (#22626).
	 * @returns {Promise<void>} Resolves once the preview window has been opened or focused.
	 * @memberof UmbPreviewController
	 */
	async preview(args: UmbPreviewControllerArgs, previewWindow?: WindowProxy | null) {
		// If a preview window is still open for the same document + provider, just focus it and let
		// SignalR handle the refresh. This only holds for internal preview URLs; external URLs (custom
		// URL providers, e.g. headless setups) have no SignalR connection back to the backoffice, so we
		// must always re-request the URL — to obtain a fresh preview token — and reload the tab. See #21820.
		if (!this.#previewIsExternal && this.#tryFocusExistingWindow(args)) {
			// Opening by the document's target name reuses the existing tab, in which case the handed-over
			// window *is* the one we just focused — closing it would shut the preview the user asked for.
			if (previewWindow && previewWindow !== this.#previewWindow) {
				previewWindow.close();
			}
			return;
		}

		let previewUrlData;
		try {
			previewUrlData = await this.#previewRepository.getPreviewUrl(
				args.unique,
				args.urlProviderAlias,
				args.culture ?? undefined,
				args.segment ?? undefined,
			);
		} catch (error) {
			previewWindow?.close();
			throw error;
		}

		if (previewUrlData.url) {
			// Add cache-busting parameter to ensure the preview tab reloads with the new preview session
			const previewUrl = new URL(previewUrlData.url, window.document.baseURI);
			previewUrl.searchParams.set('rnd', Date.now().toString());
			const target = `umbpreview-${args.unique}`;

			if (previewWindow) {
				// Adopt the tab opened during the user gesture, naming it so later previews of the same
				// document reuse it exactly as window.open(url, target) would have.
				previewWindow.name = target;
				previewWindow.location.replace(previewUrl.toString());
				this.#previewWindow = previewWindow;
			} else {
				// Reusing the same window target reloads the already-open tab in place for external URLs.
				this.#previewWindow = window.open(previewUrl.toString(), target);
			}

			this.#previewWindowKey = this.#windowKey(args);
			this.#previewIsExternal = previewUrlData.isExternal;
			this.#previewWindow?.focus();
			return;
		}

		previewWindow?.close();

		if (!previewUrlData.message) {
			return;
		}

		umbPeekError(this._host, {
			color: 'danger',
			headline: this.#localize.term('general_preview'),
			message: previewUrlData.message,
		});

		throw new Error(previewUrlData.message);
	}

	#windowKey(args: UmbPreviewControllerArgs): string {
		return `${args.unique}_${args.urlProviderAlias}`;
	}

	#tryFocusExistingWindow(args: UmbPreviewControllerArgs): boolean {
		try {
			if (!this.#previewWindow?.closed && this.#previewWindowKey === this.#windowKey(args)) {
				this.#previewWindow?.focus();
				return true;
			}
		} catch {
			// Window reference is stale, continue to create new preview session
			this.#previewWindow = null;
			this.#previewWindowKey = null;
			this.#previewIsExternal = false;
		}

		return false;
	}
}
