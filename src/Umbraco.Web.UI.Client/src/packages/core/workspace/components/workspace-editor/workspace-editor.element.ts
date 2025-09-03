import { UMB_WORKSPACE_VIEW_PATH_PATTERN } from '../../paths.js';
import type { ManifestWorkspaceView } from '../../types.js';
import { UmbWorkspaceEditorContext } from './workspace-editor.context.js';
import type { UmbWorkspaceViewContext } from './workspace-view.context.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbRoute, UmbRouterSlotInitEvent, UmbRouterSlotChangeEvent } from '@umbraco-cms/backoffice/router';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';

/**
 * @element umb-workspace-editor
 * @description
 * @slot header - Slot for workspace header
 * @slot action-menu - Slot for workspace header
 * @slot footer-info - Slot for workspace footer
 * @slot actions - Slot for workspace footer actions
 * @slot - slot for main content
 * @class UmbWorkspaceEditorElement
 * @augments {UmbLitElement}
 */
@customElement('umb-workspace-editor')
export class UmbWorkspaceEditorElement extends UmbLitElement {
	//
	#navigationContext = new UmbWorkspaceEditorContext(this);
	#workspaceViewHintObservers: Array<UmbObserverController> = [];

	@property()
	public headline = '';

	@property({ type: Boolean })
	public hideNavigation = false;

	@property({ type: Boolean })
	public enforceNoFooter = false;

	@property({ attribute: 'back-path' })
	public backPath?: string;

	@property({ type: Boolean })
	public loading = false;

	@property({ attribute: false })
	public get variantId(): UmbVariantId | undefined {
		return this._variantId;
	}
	public set variantId(value: UmbVariantId | undefined) {
		if (value && this._variantId?.equal(value)) {
			return;
		}
		this._variantId = value;
		this.#navigationContext.setVariantId(value);
		this.#observeWorkspaceViewHints();
	}
	private _variantId?: UmbVariantId | undefined;

	@property({ attribute: false })
	public set overrides(value: Array<UmbDeepPartialObject<ManifestWorkspaceView>> | undefined) {
		this.#navigationContext.setOverrides(value);
	}

	@state()
	private _workspaceViews: Array<UmbWorkspaceViewContext> = [];

	@state()
	private _hintMap: Map<string, UmbVariantHint> = new Map();

	@state()
	private _routes?: UmbRoute[];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath?: string;

	constructor() {
		super();
		this.observe(
			this.#navigationContext.views,
			(views) => {
				this._workspaceViews = views;
				this.#observeWorkspaceViewHints();
				this.#createRoutes();
			},
			null,
		);
	}

	#observeWorkspaceViewHints() {
		this.#workspaceViewHintObservers.forEach((observer) => observer.destroy());
		this._hintMap = new Map();
		this.#workspaceViewHintObservers = this._workspaceViews.map((view, index) =>
			this.observe(
				view.firstHintOfVariant,
				(hint) => {
					if (hint) {
						this._hintMap.set(view.manifest.alias, hint);
					} else {
						this._hintMap.delete(view.manifest.alias);
					}
					this.requestUpdate('_hintMap');
				},
				'umbObserveState_' + index,
			),
		);
	}

	#createRoutes() {
		let newRoutes: UmbRoute[] = [];

		if (this._workspaceViews.length > 0) {
			newRoutes = this._workspaceViews.map((context) => {
				const manifest = context.manifest;
				return {
					path: UMB_WORKSPACE_VIEW_PATH_PATTERN.generateLocal({ viewPathname: manifest.meta.pathname }),
					component: () => createExtensionElement(manifest),
					setup: (component?: any) => {
						if (component) {
							context.provideAt(component);
							component.manifest = manifest;
						}
					},
				};
			});

			// Duplicate first workspace and use it for the empty path scenario. [NL]
			newRoutes.push({ ...newRoutes[0], unique: newRoutes[0].path, path: '' });
		}

		// Add a catch-all route for not found
		// This will be the last route, so it will only match if no other routes match or if no workspace views are defined.
		newRoutes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});

		this._routes = newRoutes;
	}

	override render() {
		// Notice if no routes then fallback to use a slot.
		// TODO: Deprecate the slot feature, to rely purely on routes, cause currently bringing an additional route would mean the slotted content would never be shown. [NL]
		return html`
			<umb-body-layout main-no-padding .headline=${this.headline} ?loading=${this.loading}>
				${this.#renderBackButton()}
				<slot name="header" slot="header"></slot>
				<slot name="action-menu" slot="action-menu"></slot>
				${this.#renderViews()} ${this.#renderRoutes()}
				<slot></slot>
				${when(
					!this.enforceNoFooter,
					() => html`
						<umb-workspace-footer slot="footer" data-mark="workspace:footer">
							<slot name="footer-info"></slot>
							<slot name="actions" slot="actions" data-mark="workspace:footer-actions"></slot>
						</umb-workspace-footer>
					`,
				)}
			</umb-body-layout>
		`;
	}

	#renderViews() {
		return html`
			${!this.hideNavigation && this._workspaceViews.length > 1
				? html`
						<uui-tab-group slot="navigation" data-mark="workspace:view-links">
							${repeat(
								this._workspaceViews,
								(view) => view.manifest.alias,
								(view, index) => {
									const manifest = view.manifest;
									const displayName = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
									const hint = this._hintMap.get(manifest.alias);
									const active =
										'view/' + manifest.meta.pathname === this._activePath || (index === 0 && this._activePath === '');
									// Notice how we use index 0 to determine which workspace that is active with empty path. [NL]
									return html`
										<uui-tab
											href="${this._routerPath}/view/${manifest.meta.pathname}"
											.label=${displayName}
											?active=${active}
											data-mark="workspace:view-link:${manifest.alias}">
											<div slot="icon">
												<umb-icon name=${manifest.meta.icon}></umb-icon> ${hint && !active
													? html`<uui-badge .color=${hint.color ?? 'default'} ?attention=${hint.color === 'invalid'}
															>${hint.text}</uui-badge
														>`
													: nothing}
											</div>
											${displayName}
										</uui-tab>
									`;
								},
							)}
						</uui-tab-group>
					`
				: nothing}
		`;
	}

	#renderBackButton() {
		if (!this.backPath) return nothing;
		return html`
			<uui-button
				slot="header"
				class="back-button"
				compact
				href=${this.backPath}
				label=${this.localize.term('general_back')}
				data-mark="action:back">
				<uui-icon name="icon-arrow-left"></uui-icon>
			</uui-button>
		`;
	}

	#renderRoutes() {
		if (!this._routes || this._routes.length === 0 || !this._workspaceViews || this._workspaceViews.length === 0) {
			return nothing;
		}
		return html`
			<umb-router-slot
				inherit-addendum
				id="router-slot"
				.routes=${this._routes}
				@init=${(event: UmbRouterSlotInitEvent) => {
					this._routerPath = event.target.absoluteRouterPath;
				}}
				@change=${(event: UmbRouterSlotChangeEvent) => {
					this._activePath = event.target.localActiveViewPath;
				}}></umb-router-slot>
		`;
	}

	static override styles = [
		UmbTextStyles,
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

			.back-button {
				margin-right: var(--uui-size-space-4);
			}

			uui-input {
				width: 100%;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			div[slot='icon'] {
				position: relative;
			}

			uui-badge {
				position: absolute;
				font-size: var(--uui-type-small-size);
				top: -0.5em;
				right: auto;
				left: calc(50% + 0.8em);
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
