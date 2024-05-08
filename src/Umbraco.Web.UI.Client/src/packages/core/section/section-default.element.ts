import type { UmbWorkspaceElement } from '../workspace/workspace.element.js';
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
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

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

		this.#createRoutes();

		//Load the split panel position from localStorage
		const splitPanelPosition = localStorage.getItem('umb-split-panel-position');
		if (splitPanelPosition) {
			this._splitPanelPosition = splitPanelPosition;
		}
	}

	#createRoutes() {
		this._routes = [
			{
				path: UMB_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('../workspace/workspace.element.js'),
				setup: (element, info) => {
					(element as UmbWorkspaceElement).entityType = info.match.params.entityType;
				},
			},
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
		// Save to localStorage
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

	static styles = [
		UmbTextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
				display: flex;
			}

			umb-split-panel {
				/* --umb-split-panel-initial-position: 200px; */
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
