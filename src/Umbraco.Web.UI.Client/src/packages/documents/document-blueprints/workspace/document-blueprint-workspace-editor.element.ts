import type { UmbDocumentBlueprintVariantOptionModel } from '../types.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT } from './document-blueprint-workspace.context-token.js';
import { UmbDocumentBlueprintWorkspaceSplitViewElement } from './document-blueprint-workspace-split-view.element.js';
import { customElement, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { resolveStaleVariantRoute, UMB_WORKSPACE_PATH_VARIANT_DELIMITER } from '@umbraco-cms/backoffice/workspace';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';

// TODO: Refactor across all four content workspace editors (document, document blueprint, media, member) to use a base component. [NL]
@customElement('umb-document-blueprint-workspace-editor')
export class UmbDocumentBlueprintWorkspaceEditorElement extends UmbLitElement {
	//
	// TODO: Refactor: when having a split view/variants context token, we can rename the split view/variants component to a generic and make this component generic as well. [NL]
	private _splitViewElement = new UmbDocumentBlueprintWorkspaceSplitViewElement();

	#appLanguage?: typeof UMB_APP_LANGUAGE_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT.TYPE;

	#workspaceRoute?: string;
	#appCulture?: string;

	#variants?: Array<UmbDocumentBlueprintVariantOptionModel>;

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _loading?: boolean = true;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguage = instance;
			this.observe(this.#appLanguage?.appLanguageCulture, (appCulture) => {
				const previousCulture = this.#appCulture;
				this.#appCulture = appCulture;
				if (previousCulture === undefined) {
					// Only relevant to call generate routes initially, a later update of appCulture has no effect on the routes.
					this.#generateRoutes();
				}
				this.#syncUrlToCulture(previousCulture, appCulture);
			});
		});

		this.consumeContext(UMB_DOCUMENT_BLUEPRINT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeVariants();
			this.#observeLoading();
		});
	}

	// Re-validate on every navigation — closing a modal restores the pre-modal URL, which may
	// hold a variant that no longer exists after the content type changed while the modal was open:
	#onChangeState = () => this.#redirectStaleVariantUrl();

	override connectedCallback(): void {
		super.connectedCallback();
		window.addEventListener('changestate', this.#onChangeState);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		window.removeEventListener('changestate', this.#onChangeState);
	}

	#observeVariants() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.variantOptions,
			(variants) => {
				this.#variants = variants;
				this.#generateRoutes();
				this.#redirectStaleVariantUrl();
			},
			'_observeVariants',
		);
	}

	#observeLoading() {
		this.observe(
			this.#workspaceContext?.loading.isOn,
			(loading) => {
				this._loading = loading ?? false;
			},
			'_observeLoading',
		);
	}

	#syncUrlToCulture(previousCulture: string | undefined, appCulture: string | undefined) {
		if (!previousCulture || !appCulture || previousCulture === appCulture || !this.#workspaceRoute) return;

		const currentPath = window.location.pathname;
		const routePrefix = this.#workspaceRoute + '/';

		if (!currentPath.startsWith(routePrefix)) return;

		const remainingPath = currentPath.substring(routePrefix.length);

		// Skip split-view paths
		if (remainingPath.includes(UMB_WORKSPACE_PATH_VARIANT_DELIMITER)) return;

		// Separate the variant unique from any trailing path (e.g., /view/info)
		const slashIndex = remainingPath.indexOf('/');
		const variantUnique = slashIndex === -1 ? remainingPath : remainingPath.substring(0, slashIndex);
		const pathSuffix = slashIndex === -1 ? '' : remainingPath.substring(slashIndex);

		// Find the current variant to verify its culture matches the previous one
		const currentVariant = this.#variants?.find((v) => v.unique === variantUnique);
		if (currentVariant?.culture !== previousCulture) return;

		// Find the equivalent variant with the new culture, preserving the segment
		const newVariant = this.#variants?.find((v) => v.culture === appCulture && v.segment === currentVariant.segment);
		if (!newVariant) return;

		history.replaceState(
			null,
			'',
			`${this.#workspaceRoute}/${newVariant.unique}${pathSuffix}${window.location.search}`,
		);
	}

	#redirectStaleVariantUrl() {
		if (!this.#workspaceRoute || !this.#variants?.length) return;
		const newPath = resolveStaleVariantRoute({
			currentPath: window.location.pathname,
			workspaceRoute: this.#workspaceRoute,
			variants: this.#variants,
			appCulture: this.#appCulture,
		});
		if (newPath) {
			history.replaceState(null, '', newPath + window.location.search);
		}
	}

	async #generateRoutes() {
		if (!this.#variants || this.#variants.length === 0 || !this.#appCulture) {
			this._routes = [this.#createNotFoundRoute()];
			return;
		}

		// Generate split view routes for all available routes
		const routes: Array<UmbRoute> = [];

		routes.push({
			path: '/:variantPath/',
			preserveQuery: true,
			component: this._splitViewElement,
			setup: async (_component, info) => {
				const variants = this.#variants;
				if (!variants) {
					throw new Error('Variants are not available when resolving the route.');
				}
				if (!this.#workspaceContext) {
					throw new Error('Workspace context is not available when resolving the route.');
				}

				const consumed = info.match.fragments.consumed;

				this.#workspaceContext?.splitView.setVariantParts(consumed);
			},
		});

		if (routes.length !== 0 && this.#variants?.length) {
			// Find a decent variant to use as the default route:
			routes.push({
				path: '',
				preserveQuery: true,
				pathMatch: 'full',
				resolve: async () => {
					if (!this.#workspaceContext) {
						throw new Error('Workspace context is not available when resolving the default route.');
					}

					// get current get variables from url, and check if openCollection is set:
					const urlSearchParams = new URLSearchParams(window.location.search);
					const openCollection = urlSearchParams.has('openCollection');
					const view = openCollection ? `/view/collection` : '';

					// Is there a path matching the current culture?
					let variant = this.#variants?.find((v) => v.culture === this.#appCulture);

					if (!variant) {
						// If no match on app culture, then try to find a variant with no culture:
						variant = this.#variants?.find((v) => !v.culture);
					}

					if (!variant) {
						// If none then just use the first variant as a fallback.
						variant = this.#variants?.[0];
					}

					history.replaceState({}, '', `${this.#workspaceRoute}/${variant?.unique}${view}`);
				},
			});
		}

		routes.push(this.#createNotFoundRoute());

		this._routes = routes;
	}
	#createNotFoundRoute(): UmbRoute {
		return {
			path: '**',
			component: async () => {
				const router = await import('@umbraco-cms/backoffice/router');
				return this.#workspaceContext?.forbidden.getIsOn()
					? router.UmbRouteForbiddenElement
					: router.UmbRouteNotFoundElement;
			},
		};
	}

	private _gotWorkspaceRoute = (e: UmbRouterSlotInitEvent) => {
		this.#workspaceRoute = e.target.absoluteRouterPath;
		this.#workspaceContext?.splitView.setWorkspaceRoute(this.#workspaceRoute);
		this.#redirectStaleVariantUrl();
	};

	override render() {
		return !this._loading && this._routes
			? html`<umb-router-slot .routes=${this._routes} @init=${this._gotWorkspaceRoute}></umb-router-slot>`
			: html`<umb-view-loader></umb-view-loader>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;

				--uui-color-invalid: var(--uui-color-warning);
				--uui-color-invalid-emphasis: var(--uui-color-warning-emphasis);
				--uui-color-invalid-standalone: var(--uui-color-warning-standalone);
				--uui-color-invalid-contrast: var(--uui-color-warning-contrast);
			}
		`,
	];
}

export default UmbDocumentBlueprintWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-workspace-editor': UmbDocumentBlueprintWorkspaceEditorElement;
	}
}
