import { directAuthProvider } from './direct-auth-provider.function.js';
import { UMB_AUTH_CONTEXT, UMB_MODAL_APP_AUTH } from '@umbraco-cms/backoffice/auth';
import type { ManifestAuthProvider, UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbAppAuthController extends UmbControllerBase {
	readonly #retrievedContext: Promise<unknown>;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#authModalOpen = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#retrievedContext = this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// If the session times out mid-use, open the auth modal over the current content instead
			// of navigating away, so any unsaved work is preserved.
			// Observing an undefined source still invokes the callback, so only act when there is a
			// context to act on — otherwise tearing the context down would throw from in here.
			this.observe(
				context?.timeoutSignal,
				() => {
					if (this.#authContext) this.#openAuthModal('timedOut');
				},
				'_authState',
			);
		}).asPromise({ preventTimeout: true });
	}

	/**
	 * Checks if the user is authorized; if not, opens the auth modal over the (empty) shell.
	 * Session verification is handled by setInitialState() (the current-user/configuration
	 * cookie probe) before the router evaluates guards.
	 * @returns {Promise<boolean>} True if the user is authorized.
	 */
	async isAuthorized(): Promise<boolean> {
		await this.#retrievedContext.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		if (this.#authContext.getIsAuthorized()) {
			return true;
		}

		// Not authorized. Decide before opening the modal so a single provider doesn't flash it: this
		// guard runs at router init, after public extensions have registered, so the provider list is
		// available. Otherwise open the modal to pick.
		// Only reached on a cold boot: a timeout goes through the timeoutSignal observer and always
		// opens the modal, because auto-navigating away from a timed-out session would discard the
		// unsaved work the modal exists to preserve.
		// TODO: counts frontend manifests only; the follow-up auth-providers endpoint will reconcile
		// against the server's actually-configured providers (and local-login-disabled state).
		try {
			const providers = await firstValueFrom(
				umbExtensionsRegistry.byType<'authProvider', ManifestAuthProvider>('authProvider'),
			);

			const directProvider = directAuthProvider(providers);

			if (directProvider) {
				// redirect: true → full-page navigate (cold boot, nothing to preserve), no modal flash.
				this.#authContext.makeAuthorizationRequest(directProvider.forProviderName, true);
				return false;
			}
		} catch {
			// Fall through to the modal if the provider list can't be resolved.
		}

		this.#openAuthModal('loggedOut');
		return false;
	}

	/**
	 * Cookie auth: authentication happens in the auth modal (login providers render inline over the
	 * current view). A real navigation to the server /umbraco/login is only used by the modal's own
	 * local-login action.
	 * @param {UmbUserLoginState} userLoginState The reason the authorization flow is being started.
	 * @returns {Promise<boolean>} True if the user is authorized.
	 */
	async #openAuthModal(userLoginState: UmbUserLoginState) {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}
		// Avoid stacking a second modal instance while one is already open (e.g. a repeated timeout
		// signal, or isAuthorized() re-checked for another guarded route).
		if (this.#authModalOpen) return;
		// Set the flag before the await so two near-simultaneous triggers (e.g. a timeout signal and a
		// route-guard re-check) can't both pass the guard and open a second modal. The try/finally
		// resets it on every path, including a failed getContext.
		this.#authModalOpen = true;

		try {
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const modal = modalManager?.open(this, UMB_MODAL_APP_AUTH, {
				modal: {
					key: 'app-auth',
				},
				data: {
					userLoginState,
				},
			});
			const result = await modal?.onSubmit();

			if (result?.success) {
				this.#renderRouteIfNoneWasRendered(userLoginState);
			}
		} catch {
			// Modal was force-closed — a subsequent timeout/guard check reopens it if still unauthorized.
		} finally {
			this.#authModalOpen = false;
		}
	}

	/**
	 * On a cold boot the route guard already resolved `false`, so no route was rendered and the
	 * router slot is showing its loading fallback. The root slot only navigates on a history change,
	 * and a session arriving from a peer tab produces none — so the tab would sit on the spinner
	 * forever. Replacing the state re-runs the guard, the same way `#redirect()` starts the router.
	 *
	 * Deliberately not done for `timedOut`: there the route is already rendered behind the modal and
	 * may hold unsaved work, and re-running the guard risks re-rendering it. That state needs no
	 * nudge anyway — closing the modal reveals the view that was there all along.
	 * @param {UmbUserLoginState} userLoginState The state the modal was opened in.
	 */
	#renderRouteIfNoneWasRendered(userLoginState: UmbUserLoginState) {
		if (userLoginState === 'timedOut') return;
		history.replaceState(null, '', location.href);
	}
}
