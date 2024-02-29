import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from './member-workspace.context.js';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-member-workspace-editor')
export class UmbMemberWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestWorkspace;

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _name: string = '';

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.observe(this.#workspaceContext.name, (name) => {
				this._name = name || '';
			});
		});
	}

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
			<uui-button compact href="/section/member-management/view/members">
				<uui-icon name="icon-arrow-left"> </uui-icon>
			</uui-button>
		`;
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.Member">
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

export default UmbMemberWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-editor': UmbMemberWorkspaceEditorElement;
	}
}
