import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	PageComponent,
	UmbRoute,
	UmbRouterSlotInitEvent,
	UmbRouterSlotChangeEvent,
} from '@umbraco-cms/backoffice/router';
import {
	ManifestWorkspaceEditorView,
	ManifestWorkspaceViewCollection,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { componentHasManifestProperty } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-workspace-editor
 * @description
 * @slot icon - Slot for icon
 * @slot header - Slot for workspace header
 * @slot name - Slot for name
 * @slot footer - Slot for workspace footer
 * @slot actions - Slot for workspace footer actions
 * @slot default - slot for main content
 * @export
 * @class UmbWorkspaceLayout
 * @extends {UmbLitElement}
 */
// TODO: stop naming this something with layout. as its not just an layout. it hooks up with extensions.
@customElement('umb-workspace-editor')
export class UmbWorkspaceEditorElement extends UmbLitElement {
	@property()
	public headline = '';

	@property()
	public hideNavigation = false;

	@property()
	public enforceNoFooter = false;

	private _alias = '';
	/**
	 * Alias of the workspace. The Layout will render the workspace views that are registered for this workspace alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property()
	public get alias() {
		return this._alias;
	}
	public set alias(value) {
		const oldValue = this._alias;
		this._alias = value;
		if (oldValue !== this._alias) {
			this._observeWorkspaceViews();
			this.requestUpdate('alias', oldValue);
		}
	}

	@state()
	private _workspaceViews: Array<ManifestWorkspaceEditorView | ManifestWorkspaceViewCollection> = [];

	@state()
	private _routes?: UmbRoute[];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath?: string;

	private _observeWorkspaceViews() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfTypes<ManifestWorkspaceEditorView>(['workspaceEditorView', 'workspaceViewCollection'])
				.pipe(
					map((extensions) => extensions.filter((extension) => extension.conditions.workspaces.includes(this.alias)))
				),
			(workspaceViews) => {
				this._workspaceViews = workspaceViews;
				this._createRoutes();
			},
			'_observeWorkspaceViews'
		);
	}

	// TODO: Move into a helper function:
	private componentHasManifest(component: HTMLElement): component is HTMLElement & { manifest: unknown } {
		return component ? 'manifest' in component : false;
	}

	private _createRoutes() {
		this._routes = [];

		if (this._workspaceViews.length > 0) {
			this._routes = this._workspaceViews.map((manifest) => {
				return {
					path: `view/${manifest.meta.pathname}`,
					component: () => {
						if (manifest.type === 'workspaceViewCollection') {
							return import(
								'../workspace-content/views/collection/workspace-view-collection.element.js'
							) as unknown as Promise<HTMLElement>;
						}
						return createExtensionElement(manifest);
					},
					setup: (component) => {
						if (component && componentHasManifestProperty(component)) {
							component.manifest = manifest;
						}
					},
				} as UmbRoute;
			});

			// If we have a post fix then we need to add a direct from the empty url of the split-view-index:
			const firstView = this._workspaceViews[0];
			if (firstView) {
				this._routes.push({
					path: ``,
					redirectTo: `view/${firstView.meta.pathname}`,
				});
			}
		}
	}

	render() {
		return html`
			<umb-body-layout main-no-padding .headline=${this.headline}>
				<slot name="header" slot="header"></slot>
				${this.#renderViews()}
				<slot name="action-menu" slot="action-menu"></slot>
				${this.#renderRoutes()}
				<slot></slot>
				${this.enforceNoFooter
					? ''
					: html`
							<umb-workspace-footer slot="footer" alias=${this.alias}>
								<slot name="footer-info"></slot>
								<slot name="actions" slot="actions"></slot>
							</umb-workspace-footer>
					  `}
			</umb-body-layout>
		`;
	}

	#renderViews() {
		return html`
			${!this.hideNavigation && this._workspaceViews.length > 1
				? html`
						<uui-tab-group slot="navigation">
							${repeat(
								this._workspaceViews,
								(view) => view.alias,
								(view) => html`
									<uui-tab
										.label="${view.meta.label || view.name}"
										href="${this._routerPath}/view/${view.meta.pathname}"
										?active="${'view/' + view.meta.pathname === this._activePath}">
										<uui-icon slot="icon" name="${view.meta.icon}"></uui-icon>
										${view.meta.label || view.name}
									</uui-tab>
								`
							)}
						</uui-tab-group>
				  `
				: nothing}
		`;
	}

	#renderRoutes() {
		return html`
			${this._routes && this._routes.length > 0
				? html`
						<umb-router-slot
							id="router-slot"
							.routes="${this._routes}"
							@init=${(event: UmbRouterSlotInitEvent) => {
								this._routerPath = event.target.absoluteRouterPath;
							}}
							@change=${(event: UmbRouterSlotChangeEvent) => {
								this._activePath = event.target.localActiveViewPath;
							}}></umb-router-slot>
				  `
				: nothing}
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#router-slot {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			uui-input {
				width: 100%;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			umb-extension-slot[slot='actions'] {
				display: flex;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-editor': UmbWorkspaceEditorElement;
	}
}
