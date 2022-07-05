import { UUIButtonState, UUIComboboxListElement, UUIComboboxListEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo, RouterSlot } from 'router-slot';
import { map, Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbExtensionManifestEditorView, UmbExtensionRegistry } from '../../core/extension';
import { UmbNotificationService } from '../../core/services/notification.service';
import { UmbDataTypeStore } from '../../core/stores/data-type.store';
import { DataTypeEntity } from '../../mocks/data/data-type.data';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import '../editor-views/editor-view-data-type-edit.element';

@customElement('umb-editor-data-type')
export class UmbEditorDataTypeElement extends UmbContextConsumerMixin(LitElement) {
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
		`,
	];

	@property()
	id!: string;

	@state()
	private _dataType?: DataTypeEntity;

	@state()
	private _editorViews: Array<UmbExtensionManifestEditorView> = [];

	@state()
	private _currentView = '';

	@state()
	private _routes: Array<IRoute> = [];

	@state()
	private _saveButtonState?: UUIButtonState;

	private _dataTypeStore?: UmbDataTypeStore;
	private _dataTypeSubscription?: Subscription;
	private _extensionRegistry?: UmbExtensionRegistry;
	private _editorViewsSubscription?: Subscription;
	private _notificationService?: UmbNotificationService;

	private _routerFolder = '';

	constructor() {
		super();

		this.consumeContext('umbDataTypeStore', (store: UmbDataTypeStore) => {
			this._dataTypeStore = store;
			this._useDataType();
		});

		this.consumeContext('umbExtensionRegistry', (extensionRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._useEditorViews();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		// TODO: temp solution to handle property editor UI change
		this.addEventListener('change', this._handleChange);
	}

	private _handleChange(event: any) {
		if (!this._dataType) return;

		const target = event.composedPath()[0] as UUIComboboxListElement;
		const value = target.value as string;
		this._dataType.propertyEditorUIAlias = value;
	}

	connectedCallback(): void {
		super.connectedCallback();
		/* TODO: find a way to construct absolute urls */
		this._routerFolder = window.location.pathname.split('/view')[0];
	}

	private _useDataType() {
		this._dataTypeSubscription?.unsubscribe();

		this._dataTypeSubscription = this._dataTypeStore?.getById(parseInt(this.id)).subscribe((dataType) => {
			if (!dataType) return; // TODO: Handle nicely if there is no node.
			this._dataType = dataType;
			// TODO: merge observables
			this._createRoutes();
		});
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
						.filter((extension) => extension.meta.editors.includes('Umb.Editor.DataType'))
						.sort((a, b) => b.meta.weight - a.meta.weight)
				)
			)
			.subscribe((editorViews) => {
				this._editorViews = editorViews;
				// TODO: merge observables
				this._createRoutes();
			});
	}

	private async _createRoutes() {
		if (this._dataType && this._editorViews.length > 0) {
			this._routes = [];

			this._routes = this._editorViews.map((view) => {
				return {
					path: `view/${view.meta.pathname}`,
					component: () => document.createElement(view.elementName || 'div'),
					setup: (element: HTMLElement, info: IRoutingInfo) => {
						// TODO: make interface for EditorViews
						const editorView = element as any;
						// TODO: how do we pass data to views? Maybe we should use a context?
						editorView.dataType = this._dataType;
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

	private async _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._dataTypeStore) return;
		if (!this._dataType) return;

		try {
			this._saveButtonState = 'waiting';
			await this._dataTypeStore.save([this._dataType]);
			this._notificationService?.peek('Data Type saved');
			this._saveButtonState = 'success';
		} catch (error) {
			this._saveButtonState = 'failed';
		}
	}

	render() {
		return html`
			<umb-editor-layout>
				<uui-input slot="name" .value="${this._dataType?.name}"></uui-input>

				<uui-tab-group slot="apps">
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
					<uui-button
						@click=${this._onSave}
						look="primary"
						color="positive"
						label="Save"
						.state="${this._saveButtonState}"></uui-button>
				</div>
			</umb-editor-layout>
		`;
	}
}

export default UmbEditorDataTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-data-type': UmbEditorDataTypeElement;
	}
}
