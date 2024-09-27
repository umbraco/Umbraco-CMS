import { UMB_MEMBER_GROUP_ROOT_WORKSPACE_PATH } from '../../paths.js';
import { UMB_MEMBER_GROUP_WORKSPACE_CONTEXT } from './member-group-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-member-group-workspace-editor')
export class UmbMemberGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _unique?: string;

	#workspaceContext?: typeof UMB_MEMBER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEMBER_GROUP_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			if (!this.#workspaceContext) return;
			this.observe(this.#workspaceContext.unique, (unique) => (this._unique = unique ?? undefined));
		});
	}

	#renderActions() {
		// Actions only works if we have a valid unique.
		if (!this._unique || this.#workspaceContext?.getIsNew()) return nothing;

		return html`<umb-workspace-entity-action-menu slot="action-menu"></umb-workspace-entity-action-menu>`;
	}

	override render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.MemberGroup" back-path=${UMB_MEMBER_GROUP_ROOT_WORKSPACE_PATH}>
				<umb-workspace-name slot="header"></umb-workspace-name>
				${this.#renderActions()}
				<umb-workspace-entity-action-menu slot="action-menu"></umb-workspace-entity-action-menu>
			</umb-workspace-editor>
		`;
	}

	static override styles = [
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

export default UmbMemberGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace-editor': UmbMemberGroupWorkspaceEditorElement;
	}
}
