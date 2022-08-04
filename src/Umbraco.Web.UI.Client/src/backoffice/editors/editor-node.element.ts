import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbNodeStore } from '../../core/stores/node.store';
import { map, Subscription } from 'rxjs';
import { DocumentNode } from '../../mocks/data/content.data';
import { UmbNotificationService } from '../../core/services/notification.service';
import { UmbExtensionManifestEditorView, UmbExtensionRegistry } from '../../core/extension';
import { IRoute, IRoutingInfo, RouterSlot } from 'router-slot';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import '../editor-views/editor-view-node-edit.element';
import '../editor-views/editor-view-node-info.element';

@customElement('umb-editor-node')
export class UmbEditorNodeElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			uui-input {
				width: 100%;
				margin-left: 16px;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				flex-wrap: nowrap;
				height: 60px;
			}

			uui-tab {
				font-size: 0.8rem;
			}

			uui-box hr {
				margin-bottom: var(--uui-size-6);
			}
		`,
	];

	@property()
	id!: string;

	@state()
	_node?: DocumentNode;

	@state()
	private _routes: Array<IRoute> = [];

	@state()
	private _editorViews: Array<UmbExtensionManifestEditorView> = [];

	@state()
	private _currentView = '';

	private _nodeStore?: UmbNodeStore;
	private _nodeSubscription?: Subscription;

	private _notificationService?: UmbNotificationService;

	private _extensionRegistry?: UmbExtensionRegistry;
	private _editorViewsSubscription?: Subscription;

	private _routerFolder = '';

	constructor() {
		super();

		this.consumeContext('umbNodeStore', (store: UmbNodeStore) => {
			this._nodeStore = store;
			this._useNode();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		this.consumeContext('umbExtensionRegistry', (extensionRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._useEditorViews();
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	connectedCallback(): void {
		super.connectedCallback();
		/* TODO: find a way to construct absolute urls */
		this._routerFolder = window.location.pathname.split('/view')[0];
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;

		// TODO: Set value.
		const property = this._node?.properties.find((x) => x.alias === target.property.alias);
		if (property) {
			this._setPropertyValue(property.alias, target.value);
		} else {
			console.error('property was not found', target.property.alias);
		}
	};

	private _setPropertyValue(alias: string, value: unknown) {
		this._node?.data.forEach((data) => {
			if (data.alias === alias) {
				data.value = value;
			}
		});
	}

	private _useNode() {
		this._nodeSubscription?.unsubscribe();

		this._nodeSubscription = this._nodeStore?.getById(parseInt(this.id)).subscribe((node) => {
			if (!node) return; // TODO: Handle nicely if there is no node.
			this._node = node;
			// TODO: merge observables
			this._createRoutes();
		});
	}

	private _useEditorViews() {
		this._editorViewsSubscription?.unsubscribe();

		// TODO: how do we know which editor to show the views for?
		this._editorViewsSubscription = this._extensionRegistry
			?.extensionsOfType('editorView')
			.pipe(
				map((extensions) =>
					extensions
						.filter((extension) => extension.meta.editors.includes('Umb.Editor.Node'))
						.sort((a, b) => b.meta.weight - a.meta.weight)
				)
			)
			.subscribe((editorViews) => {
				this._editorViews = editorViews;
				// TODO: merge observables
				this._createRoutes();
			});
	}

	private _onSaveAndPublish() {
		this._onSave();
	}

	private _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (this._node) {
			this._nodeStore?.save([this._node]).then(() => {
				this._notificationService?.peek('Document saved');
			});
		}
	}

	private _onSaveAndPreview() {
		this._onSave();
	}

	// TODO: simplify setting up editors with views. This code has to be duplicated in each editor.
	private async _createRoutes() {
		if (this._node && this._editorViews.length > 0) {
			this._routes = [];

			this._routes = this._editorViews.map((view) => {
				return {
					path: `view/${view.meta.pathname}`,
					component: () => document.createElement(view.elementName || 'div'),
					setup: (element: HTMLElement, info: IRoutingInfo) => {
						// TODO: make interface for EditorViews
						const editorView = element as any;
						// TODO: how do we pass data to views? Maybe we should use a context?
						editorView.node = this._node;
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

	// TODO: Fgure out why this has been necessary for this case. Come up with another case
	private _forceRouteRender() {
		const routerSlotEl = this.shadowRoot?.querySelector('router-slot') as RouterSlot;
		if (routerSlotEl) {
			routerSlotEl.render();
		}
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._nodeSubscription?.unsubscribe();
		delete this._node;
	}

	render() {
		return html`
			<umb-editor-layout>
				<uui-input slot="name" .value="${this._node?.name}"></uui-input>
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

				<router-slot .routes="${this._routes}"></router-slot>

				<div slot="actions">
					<uui-button @click=${this._onSaveAndPreview} label="Save and preview"></uui-button>
					<uui-button @click=${this._onSave} look="secondary" label="Save"></uui-button>
					<uui-button
						@click=${this._onSaveAndPublish}
						look="primary"
						color="positive"
						label="Save and publish"></uui-button>
				</div>
			</umb-editor-layout>
		`;
	}
}

export default UmbEditorNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-node': UmbEditorNodeElement;
	}
}
