import type { UmbWorkspaceElement } from '../workspace/workspace.element.js';
import type { UmbSectionViewsElement } from './section-views/section-views.element.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	nothing,
	customElement,
	property,
	state,
	PropertyValueMap,
} from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	ManifestSection,
	ManifestSectionSidebarApp,
	UmbSectionExtensionElement,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @export
 * @class UmbBaseSectionElement
 * @description - Element hosting sections and section navigation.
 */
@customElement('umb-section-default')
export class UmbSectionDefaultElement extends UmbLitElement implements UmbSectionExtensionElement {
	@property()
	private _manifest?: ManifestSection | undefined;
	public get manifest(): ManifestSection | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestSection | undefined) {
		const oldValue = this._manifest;
		if (oldValue === value) return;
		this._manifest = value;
		this.#observeSectionSidebarApps();
		this.requestUpdate('manifest', oldValue);
	}

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _menus?: Array<Omit<ManifestSectionSidebarApp, 'kind'>>;

	constructor() {
		super();
		this.#createRoutes();
	}

	#createRoutes() {
		this._routes = [
			{
				path: 'workspace/:entityType',
				component: () => import('../workspace/workspace.element.js'),
				setup: (element, info) => {
					(element as UmbWorkspaceElement).entityType = info.match.params.entityType;
				},
			},
			{
				path: '**',
				component: () => import('./section-views/section-views.element.js'),
				setup: (element) => {
					(element as UmbSectionViewsElement).sectionAlias = this.manifest?.alias;
				},
			},
		];
	}

	// TODO: Can this be omitted? or can the same data be used for the extension slot or alike extension presentation?
	#observeSectionSidebarApps() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('sectionSidebarApp')
				.pipe(
					map((manifests) =>
						manifests.filter((manifest) => manifest.conditions.sections.includes(this._manifest?.alias ?? ''))
					)
				),
			(manifests) => {
				const oldValue = this._menus;
				this._menus = manifests;
				this.requestUpdate('_menu', oldValue);
			}
		);
	}

	render() {
		return html`
			${this._menus && this._menus.length > 0
				? html`
						<!-- TODO: these extensions should be combined into one type: sectionSidebarApp with a "subtype" -->
						<umb-section-sidebar>
							<umb-extension-slot
								type="sectionSidebarApp"
								.filter=${(items: ManifestSectionSidebarApp) =>
									items.conditions.sections.includes(this.manifest?.alias ?? '')}></umb-extension-slot>
						</umb-section-sidebar>
				  `
				: nothing}
			<umb-section-main>
				${this._routes && this._routes.length > 0
					? html`<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>`
					: nothing}
				<slot></slot>
			</umb-section-main>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-default': UmbSectionDefaultElement;
	}
}
