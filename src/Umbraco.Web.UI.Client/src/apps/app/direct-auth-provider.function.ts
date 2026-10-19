import type { ManifestAuthProvider } from '@umbraco-cms/backoffice/auth';

/**
 * The provider to send the user to without showing the chooser, if there is one.
 *
 * Only for a cold boot. A timed-out session always opens the modal instead, because navigating away
 * would discard the unsaved work the modal exists to preserve.
 * @param {ManifestAuthProvider[]} providers The registered auth providers.
 * @returns {ManifestAuthProvider | undefined} The provider to initiate directly, or undefined to let
 * the user choose.
 */
export function directAuthProvider(providers: ManifestAuthProvider[]): ManifestAuthProvider | undefined {
	// Nothing to choose between.
	if (providers.length === 1) {
		return providers[0];
	}

	// One of several asks to be gone to directly.
	return providers.find((provider) => provider.meta?.behavior?.autoRedirect);
}
