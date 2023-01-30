import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceDataTypeContext } from './data-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-data-type-workspace
 * @description - Element for displaying a Data Type Workspace
 */
@customElement('umb-data-type-workspace')
export class UmbDataTypeWorkspaceElement extends UmbLitElement {
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
				flex: 1 1 auto;
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
		if (this._entityKey) {
			this._workspaceContext.load(this._entityKey);
		}
	}

	@property()
	public set create(parentKey: string | null) {
		this._workspaceContext.create(parentKey);
	}

	private _workspaceContext: UmbWorkspaceDataTypeContext = new UmbWorkspaceDataTypeContext(this);

	@state()
	private _dataTypeName = '';

	constructor() {
		super();
		this.provideContext('umbWorkspaceContext', this._workspaceContext);
		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (dataType) => {
			if (dataType && dataType.name !== this._dataTypeName) {
				this._dataTypeName = dataType.name ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.DataType">
				<uui-input id="header" slot="header" .value=${this._dataTypeName} @input="${this._handleInput}"></uui-input>
			</umb-workspace-layout>
		`;
	}
}

export default UmbDataTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace': UmbDataTypeWorkspaceElement;
	}
}
