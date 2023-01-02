import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceDocumentContext } from './workspace-document.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-workspace-document')
export class UmbWorkspaceDocumentElement extends UmbObserverMixin(
	UmbContextConsumerMixin(UmbContextProviderMixin(LitElement))
) {
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
		this._provideWorkspace();
	}

	private _workspaceContext?: UmbWorkspaceDocumentContext;

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if (this._entityKey) {
			this._workspaceContext = new UmbWorkspaceDocumentContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
		}
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Document"></umb-workspace-content>`;
	}
}

export default UmbWorkspaceDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-document': UmbWorkspaceDocumentElement;
	}
}
