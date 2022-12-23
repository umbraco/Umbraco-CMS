import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbDataTypeStore } from '../../../core/stores/data-type/data-type.store';
import { UmbWorkspaceDataTypeContext } from './workspace-data-type.context';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

/**
 *  @element umb-workspace-data-type
 *  @description - Element for displaying a Data Type Workspace
 */
@customElement('umb-workspace-data-type')
export class UmbWorkspaceDataTypeElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex:1 1 auto;
			}
		`,
	];

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		this._provideWorkspace();
	}

	private _workspaceContext?:UmbWorkspaceDataTypeContext;

	@state()
	private _dataTypeName = '';
	

	constructor() {
		super();

		this._registerExtensions();

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if(this._entityKey) {
			this._workspaceContext = new UmbWorkspaceDataTypeContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
			this._observeWorkspace()
		}
	}
	private _registerExtensions() {
		const extensions: Array<any> = [
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.DataType.Edit',
				name: 'Data Type Workspace Edit View',
				loader: () => import('./views/edit/workspace-view-data-type-edit.element'),
				weight: 90,
				meta: {
					workspaces: ['Umb.Workspace.DataType'],
					label: 'Edit',
					pathname: 'edit',
					icon: 'edit',
				},
			},
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.DataType.Info',
				name: 'Data Type Workspace Info View',
				loader: () => import('./views/info/workspace-view-data-type-info.element'),
				weight: 90,
				meta: {
					workspaces: ['Umb.Workspace.DataType'],
					label: 'Info',
					pathname: 'info',
					icon: 'info',
				},
			},
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.DataType.Save',
				name: 'Save Data Type Workspace Action',
				loader: () => import('../shared/actions/save/workspace-action-node-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.DataType'],
					look: 'primary',
					color: 'positive'
				},
			},
		];

		extensions.forEach((extension) => {
			if (umbExtensionsRegistry.isRegistered(extension.alias)) return;
			umbExtensionsRegistry.register(extension);
		});
	}

	private _observeWorkspace() {
		if (!this._workspaceContext) return;

		this.observe<DataTypeDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (dataType) => {
			if (dataType && dataType.name !== this._dataTypeName) {
				this._dataTypeName = dataType.name ?? '';
			}
		});
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;
		this._workspaceContext?.setPropertyValue(target?.alias, target?.value);
	};

	// TODO. find a way where we don't have to do this for all Workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext?.update({ name: target.value });
			}
		}
	}

	render() {
		return html`
			<umb-workspace-entity alias="Umb.Workspace.DataType">
				<uui-input id="header" slot="header" .value=${this._dataTypeName} @input="${this._handleInput}"></uui-input>
			</umb-workspace-entity>
		`;
	}
}

export default UmbWorkspaceDataTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-data-type': UmbWorkspaceDataTypeElement;
	}
}
