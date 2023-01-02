import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceMediaContext } from './media-workspace.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

@customElement('umb-media-workspace')
export class UmbMediaWorkspaceElement extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
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

	private _workspaceContext?: UmbWorkspaceMediaContext;

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
			this._workspaceContext = new UmbWorkspaceMediaContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
		}
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Media"></umb-workspace-content>`;
	}
}

export default UmbMediaWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace': UmbMediaWorkspaceElement;
	}
}
