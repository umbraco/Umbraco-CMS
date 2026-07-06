import { UmbPreviewRepository } from './repository/preview.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';

export interface UmbPreviewControllerArgs {
	urlProviderAlias: string;
	unique: string;
	culture: string | undefined | null;
	segment: string | undefined | null;
}

export class UmbPreviewController extends UmbControllerBase {
	#previewWindow: WindowProxy | null = null;
	#previewWindowDocumentId: string | null = null;
	#previewWindowUrlProviderAlias: string | null = null;

	#localize = new UmbLocalizationController(this);

	/**
	 * Opens a preview window for the given document, reusing an existing window when one is already
	 * open for the same document and URL provider. If no preview URL can be resolved, an error notification
	 * is shown and an error is thrown.
	 * @param {UmbPreviewControllerArgs} args - The preview arguments.
	 * @param {string} args.urlProviderAlias - The alias of the URL provider used to resolve the preview URL.
	 * @param {string} args.unique - The unique identifier of the document to preview.
	 * @param {string | undefined | null} args.culture - The culture to preview, or undefined/null for the default culture.
	 * @param {string | undefined | null} args.segment - The segment to preview, or undefined/null for no segment.
	 * @returns {Promise<void>} Resolves once the preview window has been opened or focused.
	 * @memberof UmbPreviewController
	 */
	async preview(args: UmbPreviewControllerArgs) {
		// Check if preview window is still open and showing the same document + provider
		// If so, just focus it and let SignalR handle the refresh
		try {
			if (
				this.#previewWindow &&
				!this.#previewWindow.closed &&
				this.#previewWindowDocumentId === args.unique &&
				this.#previewWindowUrlProviderAlias === args.urlProviderAlias
			) {
				this.#previewWindow.focus();
				return;
			}
		} catch {
			// Window reference is stale, continue to create new preview session
			this.#previewWindow = null;
			this.#previewWindowDocumentId = null;
			this.#previewWindowUrlProviderAlias = null;
		}

		const previewRepository = new UmbPreviewRepository(this);
		const previewUrlData = await previewRepository.getPreviewUrl(
			args.unique,
			args.urlProviderAlias,
			args.culture ?? undefined,
			args.segment ?? undefined,
		);

		if (previewUrlData.url) {
			// Add cache-busting parameter to ensure the preview tab reloads with the new preview session
			const previewUrl = new URL(previewUrlData.url, window.document.baseURI);
			previewUrl.searchParams.set('rnd', Date.now().toString());
			this.#previewWindow = window.open(previewUrl.toString(), `umbpreview-${args.unique}`);
			this.#previewWindowDocumentId = args.unique;
			this.#previewWindowUrlProviderAlias = args.urlProviderAlias;
			this.#previewWindow?.focus();
			return;
		}

		if (previewUrlData.message) {

			umbPeekError(this._host, {
				color: 'danger',
				headline: this.#localize.term('general_preview'),
				message: previewUrlData.message,
			});

			throw new Error(previewUrlData.message);
		}
	}
}
