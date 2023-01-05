import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceDocumentContext } from './document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
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
			this._workspaceContext?.load(this._entityKey);
		}
	}

	@property()
	public set create(parentKey: string | null) {
		this._workspaceContext?.create(parentKey);
	}

	private _workspaceContext?: UmbWorkspaceDocumentContext;


	constructor() {
		super();
		this._workspaceContext = new UmbWorkspaceDocumentContext(this);
		this.provideContext('umbWorkspaceContext', this._workspaceContext);
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Document"></umb-workspace-content>`;
	}
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
