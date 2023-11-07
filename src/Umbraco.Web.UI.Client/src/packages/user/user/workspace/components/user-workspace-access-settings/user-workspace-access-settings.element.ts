import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context.js';
import { html, customElement, state, css, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { UmbUserGroupInputElement } from '@umbraco-cms/backoffice/user-group';

@customElement('umb-user-workspace-access-settings')
export class UmbUserWorkspaceAccessSettingsElement extends UmbLitElement {
	@state()
	private _user?: UserResponseModel;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(this.#userWorkspaceContext.data, (user) => (this._user = user), 'umbUserObserver');
		});
	}

	#onUserGroupsChange(event: UmbChangeEvent) {
		const target = event.target as UmbUserGroupInputElement;
		this.#userWorkspaceContext?.updateProperty('userGroupIds', target.selectedIds);
	}

	#onDocumentStartNodeChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputDocumentElement;
		this.#userWorkspaceContext?.updateProperty('contentStartNodeIds', target.selectedIds);
	}

	#onMediaStartNodeChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputMediaElement;
		this.#userWorkspaceContext?.updateProperty('mediaStartNodeIds', target.selectedIds);
	}

	render() {
		return html` <uui-box>
				<div slot="headline"><umb-localize key="user_assignAccess">Assign Access</umb-localize></div>
				<div id="assign-access">
					<umb-workspace-property-layout
						label="${this.localize.term('general_groups')}"
						description="${this.localize.term('user_groupsHelp')}">
						<umb-user-group-input
							slot="editor"
							.selectedIds=${this._user?.userGroupIds ?? []}
							@change=${this.#onUserGroupsChange}></umb-user-group-input>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label=${this.localize.term('user_startnodes')}
						description=${this.localize.term('user_startnodeshelp')}>
						<umb-input-document
							.selectedIds=${this._user?.contentStartNodeIds ?? []}
							@change=${this.#onDocumentStartNodeChange}
							slot="editor"></umb-input-document>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout
						label=${this.localize.term('user_mediastartnodes')}
						description=${this.localize.term('user_mediastartnodeshelp')}>
						<umb-input-media
							.selectedIds=${this._user?.mediaStartNodeIds ?? []}
							@change=${this.#onMediaStartNodeChange}
							slot="editor"></umb-input-media>
					</umb-workspace-property-layout>
				</div>
			</uui-box>

			<uui-box id="access" headline=${this.localize.term('user_access')}>
				<div slot="header" class="faded-text">
					<umb-localize key="user_accessHelp"
						>Based on the assigned groups and start nodes, the user has access to the following nodes</umb-localize
					>
				</div>

				${this.#renderDocumentStartNodes()}
				<hr />
				${this.#renderMediaStartNodes()}
			</uui-box>`;
	}

	#renderDocumentStartNodes() {
		return html` <b><umb-localize key="sections_content">Content</umb-localize></b>
			<umb-user-document-start-node .ids=${this._user?.contentStartNodeIds || []}></umb-user-document-start-node>`;
	}

	#renderMediaStartNodes() {
		return html` <b><umb-localize key="sections_media">Media</umb-localize></b>
			<umb-user-media-start-node .ids=${this._user?.mediaStartNodeIds || []}></umb-user-media-start-node>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#access {
				margin-top: var(--uui-size-space-4);
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			.faded-text {
				color: var(--uui-color-text-alt);
				font-size: 0.8rem;
			}
		`,
	];
}

export default UmbUserWorkspaceAccessSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-access-settings': UmbUserWorkspaceAccessSettingsElement;
	}
}
