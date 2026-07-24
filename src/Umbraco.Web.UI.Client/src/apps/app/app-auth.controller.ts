import { UMB_AUTH_CONTEXT, UMB_MODAL_APP_AUTH } from '@umbraco-cms/backoffice/auth';
import type { UmbUserLoginState } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbAppAuthController extends UmbControllerBase {
	#retrievedContext: Promise<unknown>;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#authModalOpen = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#retrievedContext = this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// If the session times out mid-use, open the auth modal over the current content instead
			// of navigating away, so any unsaved work is preserved.
			this.observe(context?.timeoutSignal, () => this.#openAuthModal('timedOut'), '_authState');
		}).asPromise({ preventTimeout: true });
	}

	/**
	 * Checks if the user is authorized; if not, opens the auth modal over the (empty) shell.
	 * Session verification is handled by setInitialState() (the current-user/configuration
	 * cookie probe) before the router evaluates guards.
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
		// available. Exactly one provider → initiate it directly (full-page); otherwise open the modal
		// to pick. (Timeouts always open the modal — see the timeoutSignal observer.)
		// TODO: counts frontend manifests only; the follow-up auth-providers endpoint will reconcile
		// against the server's actually-configured providers (and local-login-disabled state).
		try {
			const providers = await firstValueFrom(this.#authContext.getAuthProviders(umbExtensionsRegistry));
			if (providers.length === 1) {
				this.#authContext.autoInitiateLogin(providers[0]);
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
	 */
	async #openAuthModal(userLoginState: UmbUserLoginState) {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}
		// Stay put on the logout landing: it already renders the full login view inline (see
		// app-auth.element.ts), so popping the modal there too would double up the login UI.
		const logoutPath = new URL(this.#authContext.getPostLogoutRedirectUrl()).pathname;
		if (window.location.pathname === logoutPath) {
			return;
		}

		// Avoid stacking a second modal instance while one is already open (e.g. a repeated timeout
		// signal, or isAuthorized() re-checked for another guarded route).
		if (this.#authModalOpen) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		this.#authModalOpen = true;
		const modal = modalManager?.open(this, UMB_MODAL_APP_AUTH, {
			modal: {
				key: 'app-auth',
			},
			data: {
				userLoginState,
			},
		});

		try {
			await modal?.onSubmit();
		} catch {
			// Modal was force-closed — a subsequent timeout/guard check reopens it if still unauthorized.
		} finally {
			this.#authModalOpen = false;
		}
	}
}
