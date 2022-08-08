import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionManifestEditorView, UmbExtensionRegistry } from '../../../core/extension';
import { map, Subscription } from 'rxjs';
import { IRoute, IRoutingInfo, RouterSlot } from 'router-slot';

import './editor-layout.element';
@customElement('umb-editor-entity')
class UmbEditorEntity extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				gap: 16px;
				align-items: center;
				min-height: 60px;
			}

			#footer {
				display: flex;
				height: 100%;
				align-items: center;
				gap: 16px;
			}

			#actions {
				display: block;
				margin-left: auto;
			}

			uui-input {
				width: 100%;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
				flex-wrap: nowrap;
				height: 60px;
			}
		`,
	];

	@property()
	alias = '';

	@property()
	name = '';

	@state()
	private _editorViews: Array<UmbExtensionManifestEditorView> = [];

	@state()
	private _currentView = '';

	@state()
	private _routes: Array<IRoute> = [];

	private _extensionRegistry?: UmbExtensionRegistry;
	private _editorViewsSubscription?: Subscription;
	private _routerFolder = '';

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (extensionRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._useEditorViews();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		/* TODO: find a way to construct absolute urls */
		this._routerFolder = window.location.pathname.split('/view')[0];
	}

	// TODO: simplify setting up editors with views. This code has to be duplicated in each editor.
	private _useEditorViews() {
		this._editorViewsSubscription?.unsubscribe();

		// TODO: how do we know which editor to show the views for?
		this._editorViewsSubscription = this._extensionRegistry
			?.extensionsOfType('editorView')
			.pipe(
				map((extensions) =>
					extensions
						.filter((extension) => extension.meta.editors.includes(this.alias))
						.sort((a, b) => b.meta.weight - a.meta.weight)
				)
			)
			.subscribe((editorViews) => {
				this._editorViews = editorViews;
				this._createRoutes();
			});
	}

	private async _createRoutes() {
		if (this._editorViews.length > 0) {
			this._routes = [];

			this._routes = this._editorViews.map((view) => {
				return {
					path: `view/${view.meta.pathname}`,
					component: () => document.createElement(view.elementName || 'div'),
					setup: (element: HTMLElement, info: IRoutingInfo) => {
						this._currentView = info.match.route.path;
					},
				};
			});

			this._routes.push({
				path: '**',
				redirectTo: `view/${this._editorViews?.[0].meta.pathname}`,
			});

			this.requestUpdate();
			await this.updateComplete;

			this._forceRouteRender();
		}
	}

	// TODO: Figure out why this has been necessary for this case. Come up with another case
	private _forceRouteRender() {
		const routerSlotEl = this.shadowRoot?.querySelector('router-slot') as RouterSlot;
		if (routerSlotEl) {
			routerSlotEl.render();
		}
	}

	private _renderViews() {
		return html`
			${this._editorViews?.length > 0
				? html`
						<uui-tab-group slot="views">
							${this._editorViews.map(
								(view: UmbExtensionManifestEditorView) => html`
									<uui-tab
										.label="${view.name}"
										href="${this._routerFolder}/view/${view.meta.pathname}"
										?active="${this._currentView.includes(view.meta.pathname)}">
										<uui-icon slot="icon" name="${view.meta.icon}"></uui-icon>
										${view.name}
									</uui-tab>
								`
							)}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}

	render() {
		return html`
			<umb-editor-layout>
				<div id="header" slot="header">
					<slot id="icon" name="icon"></slot>
					<uui-input .value="${this.name}"></uui-input>
					${this._renderViews()}
				</div>

				<router-slot .routes="${this._routes}"></router-slot>
				<slot></slot>

				<div id="footer" slot="footer">
					<slot name="footer"></slot>
					<slot id="actions" name="actions"></slot>
				</div>
			</umb-editor-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-entity': UmbEditorEntity;
	}
}
