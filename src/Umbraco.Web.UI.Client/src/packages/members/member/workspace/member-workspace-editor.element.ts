import { UMB_MEMBER_WORKSPACE_CONTEXT } from './member-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-member-workspace-editor')
export class UmbMemberWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestWorkspace;

	@state()
	private _unique?: string;
	@state()
	private _email?: string;

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeData();
		});
	}

	// Only for CRUD demonstration purposes
	#observeData() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.unique, (unique) => {
			this._unique = unique;
		});
		this.observe(this.#workspaceContext.email, (email) => {
			this._email = email;
		});
	}

	// Only for CRUD demonstration purposes
	#onChange = (e: Event) => {
		const input = e.target as HTMLInputElement;
		this.#workspaceContext!.updateData({ email: input.value });
	};

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.Member">
				<div>Unique: ${this._unique}</div>
				<!-- Only for CRUD demonstration purposes -->
				<input type="email" value=${ifDefined(this._email)} @change=${this.#onChange} />
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
		`,
	];
}

export default UmbMemberWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-editor': UmbMemberWorkspaceEditorElement;
	}
}
