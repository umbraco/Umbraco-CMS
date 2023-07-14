import type { UmbWorkspaceElement } from '../workspace/workspace.element.js';
import type { UmbSectionViewsElement } from './section-views/section-views.element.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import {
	ManifestSection,
	ManifestSectionSidebarApp,
	ManifestSectionSidebarAppMenuKind,
	UmbSectionExtensionElement,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbExtensionElementController, UmbExtensionsElementController } from '@umbraco-cms/backoffice/extension-api';

/**
 * @export
 * @class UmbBaseSectionElement
 * @description - Element hosting sections and section navigation.
 */
@customElement('umb-section-default')
export class UmbSectionDefaultElement extends UmbLitElement implements UmbSectionExtensionElement {
	private _manifest?: ManifestSection | undefined;
	private _extensionController?: UmbExtensionsElementController<'sectionSidebarApp'>;

	@property({ type: Object, attribute: false })
	public get manifest(): ManifestSection | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestSection | undefined) {
		const oldValue = this._manifest;
		if (oldValue === value) return;
		this._manifest = value;

		// It was important to ensure that the UmbExtensionsElementController first got created a little later.
		// TODO: this needs further investigation, to figure out why it happens.
		if (!this._extensionController) {
			this._extensionController = new UmbExtensionsElementController(this, 'sectionSidebarApp', null, (sidebarApps) => {
				this._sidebarApps = sidebarApps;
				this.requestUpdate('_sidebarApps');
			});
		}
		this.requestUpdate('manifest', oldValue);
	}

	@state()
	private _routes?: Array<UmbRoute>;

	@state()
	private _sidebarApps?: Array<
		UmbExtensionElementController<ManifestSectionSidebarApp | ManifestSectionSidebarAppMenuKind>
	>;

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

	render() {
		return html`
			${this._sidebarApps && this._sidebarApps.length > 0
				? html`
						<!-- TODO: these extensions should be combined into one type: sectionSidebarApp with a "subtype" -->
						<umb-section-sidebar>
							${repeat(
								this._sidebarApps,
								(app) => app.alias,
								(app) => app.component
							)}
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
