import { UMB_MEMBER_GROUP_WORKSPACE_CONTEXT } from './member-group-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-member-group-workspace-editor')
export class UmbMemberGroupWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestWorkspace;

	@state()
	private _name = '';

	#workspaceContext?: typeof UMB_MEMBER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_GROUP_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name ?? ''));
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#onInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	#renderBackButton() {
		return html`
			<uui-button compact href="/section/member-management/view/member-groups">
				<uui-icon name="icon-arrow-left"> </uui-icon>
			</uui-button>
		`;
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.MemberGroup">
				<div id="header" slot="header">
					${this.#renderBackButton()}
					<uui-input id="nameInput" .value=${this._name} @input="${this.#onInput}"></uui-input>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
			#header {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: center;
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbMemberGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-editor': UmbMemberGroupWorkspaceEditorElement;
	}
}
