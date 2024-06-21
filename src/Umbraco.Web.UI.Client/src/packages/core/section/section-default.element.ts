import type { ManifestSectionRoute } from '../extension-registry/models/section-route.model.js';
import type { UmbSectionMainViewElement } from './section-main-views/section-main-views.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type {
	ManifestSection,
	ManifestSectionSidebarApp,
	ManifestSectionSidebarAppMenuKind,
	UmbSectionElement,
} from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { IRoute, IRoutingInfo, PageComponent, UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbExtensionsApiInitializer,
	UmbExtensionsElementInitializer,
	UmbExtensionsManifestInitializer,
	createExtensionApi,
} from '@umbraco-cms/backoffice/extension-api';
import { aliasToPath, debounce } from '@umbraco-cms/backoffice/utils';

/**
 * @export
 * @class UmbBaseSectionElement
 * @description - Element hosting sections and section navigation.
 */
@customElement('umb-section-default')
export class UmbSectionDefaultElement extends UmbLitElement implements UmbSectionElement {
	private _manifest?: ManifestSection | undefined;

	@property({ type: Object, attribute: false })
	public get manifest(): ManifestSection | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestSection | undefined) {
		const oldValue = this._manifest;
		if (oldValue === value) return;
		this._manifest = value;

		this.requestUpdate('manifest', oldValue);
	}

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _sidebarApps?: Array<
		UmbExtensionElementInitializer<ManifestSectionSidebarApp | ManifestSectionSidebarAppMenuKind>
	>;

	@state()
	_splitPanelPosition = '300px';

	constructor() {
		super();

		new UmbExtensionsElementInitializer(this, umbExtensionsRegistry, 'sectionSidebarApp', null, (sidebarApps) => {
			const oldValue = this._sidebarApps;
			this._sidebarApps = sidebarApps;
			this.requestUpdate('_sidebarApps', oldValue);
		});

		this.#observeRoutes();

		const splitPanelPosition = localStorage.getItem('umb-split-panel-position');
		if (splitPanelPosition) {
			this._splitPanelPosition = splitPanelPosition;
		}
	}

	#observeRoutes(): void {
		new UmbExtensionsManifestInitializer<ManifestSectionRoute, 'sectionRoute', ManifestSectionRoute>(
			this,
			umbExtensionsRegistry,
			'sectionRoute',
			null,
			async (sectionRouteExtensions) => {
				// TODO: we only support extensions with an element prop
				const extensionsWithElement = sectionRouteExtensions.filter((extension) => extension.manifest.element);
				const extensionsWithoutElement = sectionRouteExtensions.filter((extension) => !extension.manifest.element);
				if (extensionsWithoutElement.length > 0) throw new Error('sectionRoute extensions must have an element');

				const routes: Array<IRoute> = await Promise.all(
					extensionsWithElement.map(async (extensionController) => {
						const api = await createExtensionApi(this, extensionController.manifest);

						return {
							path:
								api?.getPath?.() ||
								extensionController.manifest.meta?.path ||
								aliasToPath(extensionController.manifest.alias),
							// TODO: look into removing the "as PageComponent" type hack
							// be aware that this is kind of a hack to pass the manifest element to the router. But as the two resolve components
							// in a similar way. I currently find it more safe to let the router do the component resolving instead
							// of replicating it as a custom resolver here.
							component: extensionController.manifest.element as PageComponent | PromiseLike<PageComponent>,
							setup: (element: PageComponent, info: IRoutingInfo) => {
								api?.setup?.(element, info);
							},
						};
					}),
				);

				this.#debouncedCreateRoutes(routes);
			},
			'umbRouteExtensionApisInitializer',
		);
	}

	#debouncedCreateRoutes = debounce(this.#createRoutes, 50);

	#createRoutes(routes: Array<IRoute>) {
		this._routes = [
			...routes,
			{
				path: '**',
				component: () => import('./section-main-views/section-main-views.element.js'),
				setup: (element) => {
					(element as UmbSectionMainViewElement).sectionAlias = this.manifest?.alias;
				},
			},
		];
	}

	#onSplitPanelChange(event: CustomEvent) {
		const position = event.detail.position;
		localStorage.setItem('umb-split-panel-position', position.toString());
	}

	render() {
		return html`
			<umb-split-panel
				lock="start"
				snap="300px"
				@position-changed=${this.#onSplitPanelChange}
				.position=${this._splitPanelPosition}>
				${this._sidebarApps && this._sidebarApps.length > 0
					? html`
							<!-- TODO: these extensions should be combined into one type: sectionSidebarApp with a "subtype" -->
							<umb-section-sidebar slot="start">
								${repeat(
									this._sidebarApps,
									(app) => app.alias,
									(app) => app.component,
								)}
							</umb-section-sidebar>
						`
					: nothing}
				<umb-section-main slot="end">
					${this._routes && this._routes.length > 0
						? html`<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>`
						: nothing}
					<slot></slot>
				</umb-section-main>
			</umb-split-panel>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			umb-split-panel {
				--umb-split-panel-start-min-width: 200px;
				--umb-split-panel-start-max-width: 400px;
				--umb-split-panel-end-min-width: 600px;
				--umb-split-panel-slot-overflow: visible;
			}
			@media only screen and (min-width: 800px) {
				umb-split-panel {
					--umb-split-panel-initial-position: 300px;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-default': UmbSectionDefaultElement;
	}
}
