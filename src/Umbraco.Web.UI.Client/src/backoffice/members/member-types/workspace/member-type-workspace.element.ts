import { UUIInputEvent, UUIInputElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceMemberTypeContext } from './member-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-member-type-workspace')
export class UmbMemberTypeWorkspaceElement extends UmbLitElement {
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

	@state()
	private _memberTypeName = '';

	@state()
	private _unique?: string;

	#workspaceContext = new UmbWorkspaceMemberTypeContext(this);

	public load(entityKey: string) {
		this.#workspaceContext?.load(entityKey);
		this._unique = entityKey;
	}

	public create() {
		this.#workspaceContext.createScaffold();
	}

	constructor() {
		super();
		this.observe(this.#workspaceContext.name, (memberTypeName) => {
			if (memberTypeName !== this._memberTypeName) {
				this._memberTypeName = memberTypeName ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.MemberType">
				<uui-input id="header" slot="header" .value=${this._unique} @input="${this._handleInput}"></uui-input>
			</umb-workspace-layout>
		`;
	}
}

export default UmbMemberTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-type-workspace': UmbMemberTypeWorkspaceElement;
	}
}
