import { UmbPreviewRepository } from '../repository/preview.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbPreviewSessionRenewalArgs {
	unique?: string;
	culture?: string;
	segment?: string;
}

// The preview app always previews through the built-in document URL provider, whose preview URL is internal
// and so (re)starts the preview session on the server.
const UMB_PREVIEW_SESSION_URL_PROVIDER_ALIAS = 'umbDocumentUrlProvider';

/**
 * Keeps the website preview session alive for as long as the back-office session is, by asking the server to
 * renew it whenever the back-office session is renewed.
 */
export class UmbPreviewSessionRenewalController extends UmbControllerBase {
	#previewRepository: UmbPreviewRepository;

	/**
	 * Creates a new {@link UmbPreviewSessionRenewalController}.
	 * @param {UmbControllerHost} host - The controller host.
	 * @param {UmbPreviewRepository} [previewRepository] - Optional repository override; defaults to a new
	 * {@link UmbPreviewRepository}. Intended for testing.
	 */
	constructor(host: UmbControllerHost, previewRepository?: UmbPreviewRepository) {
		super(host);
		this.#previewRepository = previewRepository ?? new UmbPreviewRepository(this);
	}

	/**
	 * Renews the preview session each time the given back-office session emits a live session.
	 * @param {Observable<unknown>} session - The back-office session; emits a falsy value when there is no session.
	 * @param {() => UmbPreviewSessionRenewalArgs} getArgs - Provides the document currently being previewed.
	 */
	observeSession(session: Observable<unknown>, getArgs: () => UmbPreviewSessionRenewalArgs) {
		this.observe(
			session,
			(value) => {
				if (value) {
					this.renew(getArgs());
				}
			},
			'_observePreviewSession',
		);
	}

	/**
	 * Renews the preview session for the given document.
	 * @param {UmbPreviewSessionRenewalArgs} args - The document currently being previewed.
	 * @returns {Promise<void>} Resolves once the renewal has completed or failed.
	 */
	async renew(args: UmbPreviewSessionRenewalArgs): Promise<void> {
		if (!args.unique) return;

		try {
			await this.#previewRepository.getPreviewUrl(
				args.unique,
				UMB_PREVIEW_SESSION_URL_PROVIDER_ALIAS,
				args.culture,
				args.segment,
			);
		} catch {
			// The preview carries on with its current session until that expires.
		}
	}
}
