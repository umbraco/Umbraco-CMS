import { resolveStaleVariantRoute } from './resolve-stale-variant-route.function.js';
import type { UmbStaleVariantRouteResolverVariant } from './resolve-stale-variant-route.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_MODAL_ROUTE_PATH_SEGMENT } from '@umbraco-cms/backoffice/router';

interface UmbContentTypeVarianceChangeRedirectHost extends UmbControllerHost {
	readonly splitView: { getWorkspaceRoute(): string | undefined };
	getVariesByCulture(): boolean | undefined;
	getVariesBySegment(): boolean | undefined;
	getVariantOptions(): Promise<Array<UmbStaleVariantRouteResolverVariant>>;
}

interface UmbContentTypeVariance {
	variesByCulture: boolean | undefined;
	variesBySegment: boolean | undefined;
}

/**
 * Moves the workspace URL to an existing variant when the content type's variance is changed from within the
 * workspace, for example by editing the content type in an infinite editing modal. Only the editor who made the
 * change is redirected; a URL referring to a variant that does not exist is otherwise left alone.
 * @internal
 */
export class UmbContentTypeVarianceChangeRedirectController extends UmbControllerBase {
	readonly #workspace: UmbContentTypeVarianceChangeRedirectHost;
	#appLanguage?: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE;
	#trackedVariance?: UmbContentTypeVariance;

	constructor(host: UmbContentTypeVarianceChangeRedirectHost) {
		super(host);
		this.#workspace = host;
		// TODO: Should not consume, but instead get as part of the onChangeState event handler. [NL]
		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.#appLanguage = context;
		});
	}

	/**
	 * Call before the content type is edited from within the workspace, for example when opening it in a modal.
	 * Tracking ends once the URL settles outside of any modal, that is when the modal has closed: if the variance
	 * changed by then, redirects to the best matching variant.
	 *
	 * The URL is checked on navigation rather than on the modal's result: a variance change removes the variant
	 * the modal was opened from, which closes the modal without resolving it.
	 */
	// TODO: Could we instead of starting this when a user navigated to a Document-Type instead, react when a Document Type got changed? — This would be relevant when we implement Signal-R or when an updated Document Type reaches our system [NL]
	trackVarianceChange(): void {
		this.#trackedVariance = this.#getVariance();
		window.addEventListener('changestate', this.#onChangeState);
	}

	readonly #onChangeState = async () => {
		const previous = this.#trackedVariance;
		// TODO: isInModal is not good enough, you could have documents opened from a modal, that got it's Document-Type changed.
		if (!previous || this.#isInModal()) return;
		this.#stopTracking();

		const current = this.#getVariance();
		if (previous.variesByCulture === current.variesByCulture && previous.variesBySegment === current.variesBySegment) {
			return;
		}

		const workspaceRoute = this.#workspace.splitView.getWorkspaceRoute();
		if (!workspaceRoute) return;

		const variants = await this.#workspace.getVariantOptions();
		const newPath = resolveStaleVariantRoute({
			currentPath: window.location.pathname,
			workspaceRoute,
			variants,
			appCulture: this.#appLanguage?.getAppCulture(),
		});
		if (newPath) {
			history.replaceState(null, '', newPath + window.location.search);
		}
	};

	#isInModal(): boolean {
		const workspaceRoute = this.#workspace.splitView.getWorkspaceRoute() ?? '';
		const currentPath = window.location.pathname;
		const workspacePath = currentPath.startsWith(workspaceRoute) ? currentPath.substring(workspaceRoute.length) : '';
		return workspacePath.split('/').includes(UMB_MODAL_ROUTE_PATH_SEGMENT);
	}

	#stopTracking(): void {
		this.#trackedVariance = undefined;
		window.removeEventListener('changestate', this.#onChangeState);
	}

	#getVariance(): UmbContentTypeVariance {
		return {
			variesByCulture: this.#workspace.getVariesByCulture(),
			variesBySegment: this.#workspace.getVariesBySegment(),
		};
	}

	public override destroy(): void {
		this.#stopTracking();
		super.destroy();
	}
}
