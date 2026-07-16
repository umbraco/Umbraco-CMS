import { resolveStaleVariantRoute } from './resolve-stale-variant-route.function.js';
import type { UmbStaleVariantRouteResolverVariant } from './resolve-stale-variant-route.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * The accessors providing the current workspace state to {@link UmbWorkspaceStaleVariantRedirectController}.
 */
export interface UmbWorkspaceStaleVariantRedirectControllerArgs {
	getWorkspaceRoute: () => string | undefined;
	getVariants: () => Array<UmbStaleVariantRouteResolverVariant> | undefined;
	getAppCulture: () => string | undefined;
}

/**
 * Redirects the workspace URL to the best matching variant when its variant part no longer maps to an
 * existing variant option — e.g. after the content type's variance changed. Re-validates on every
 * navigation, as closing a modal restores the pre-modal URL, which may hold a stale variant.
 */
export class UmbWorkspaceStaleVariantRedirectController extends UmbControllerBase {
	#args: UmbWorkspaceStaleVariantRedirectControllerArgs;

	constructor(host: UmbControllerHost, args: UmbWorkspaceStaleVariantRedirectControllerArgs) {
		super(host);
		this.#args = args;
	}

	#onChangeState = () => this.redirect();

	override hostConnected(): void {
		super.hostConnected();
		window.addEventListener('changestate', this.#onChangeState);
	}

	override hostDisconnected(): void {
		super.hostDisconnected();
		window.removeEventListener('changestate', this.#onChangeState);
	}

	/**
	 * Applies the corrected variant URL via history.replaceState when the current URL is stale.
	 */
	public redirect(): void {
		const workspaceRoute = this.#args.getWorkspaceRoute();
		const variants = this.#args.getVariants();
		if (!workspaceRoute || !variants?.length) return;
		const newPath = resolveStaleVariantRoute({
			currentPath: window.location.pathname,
			workspaceRoute,
			variants,
			appCulture: this.#args.getAppCulture(),
		});
		if (newPath) {
			history.replaceState(null, '', newPath + window.location.search);
		}
	}

	public override destroy(): void {
		window.removeEventListener('changestate', this.#onChangeState);
		super.destroy();
	}
}
