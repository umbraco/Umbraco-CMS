import { UmbUserInputElement } from '@umbraco-cms/backoffice/user';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../index.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from './user-group-workspace.context.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import { UmbInputSectionElement } from '@umbraco-cms/backoffice/components';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/events';
import { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';

import './components/user-group-default-permission-list.element.js';
import './components/user-group-granular-permission-list.element.js';

@customElement('umb-user-group-workspace-editor')
export class UmbUserGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _userKeys?: Array<string>;

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup));
			this.observe(this.#workspaceContext.userIds, (userKeys) => (this._userKeys = userKeys));
		});
	}

	#onSectionsChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputSectionElement;
		this.#workspaceContext?.updateProperty('sections', target.value);
	}

	#onDocumentStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputDocumentElement;
		this.#workspaceContext?.updateProperty('documentStartNodeId', target.selectedIds[0]);
	}

	#onMediaStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputMediaElement;
		this.#workspaceContext?.updateProperty('mediaStartNodeId', target.selectedIds[0]);
	}

	#onUsersChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbUserInputElement;
		this.#workspaceContext?.updateUserKeys(target.selectedIds);
	}

	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.updateProperty('name', target.value);
			}
		}
	}

	render() {
		if (!this._userGroup) return nothing;

		return html`
			<umb-workspace-editor alias="Umb.Workspace.UserGroup" class="uui-text">
				${this.#renderHeader()}
				<div id="main">
					<div id="left-column">${this.#renderLeftColumn()}</div>
					<div id="right-column">${this.#renderRightColumn()}</div>
				</div>
			</umb-workspace-editor>
		`;
	}

	#renderHeader() {
		return html`
			<div id="header" slot="header">
				<a href="/section/users/view/user-groups">
					<uui-icon name="umb:arrow-left"></uui-icon>
				</a>
				<uui-input
					id="name"
					label=${this.localize.term('general_name')}
					.value=${this._userGroup?.name ?? ''}
					@input="${this.#onNameChange}"></uui-input>
			</div>
		`;
	}

	#renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html`
			<uui-box>
				<div slot="headline"><umb-localize key="user_assignAccess"></umb-localize></div>
				<umb-workspace-property-layout
					label=${this.localize.term('main_sections')}
					description=${this.localize.term('user_sectionsHelp')}>
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections ?? []}
						@change=${this.#onSectionsChange}></umb-input-section>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label=${this.localize.term('defaultdialogs_selectContentStartNode')}
					description=${this.localize.term('user_startnodehelp')}>
					<umb-input-document
						slot="editor"
						max="1"
						.selectedIds=${this._userGroup.documentStartNodeId ? [this._userGroup.documentStartNodeId] : []}
						@change=${this.#onDocumentStartNodeChange}></umb-input-document>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label=${this.localize.term('defaultdialogs_selectMediaStartNode')}
					description=${this.localize.term('user_mediastartnodehelp')}>
					<umb-input-media
						slot="editor"
						max="1"
						.selectedIds=${this._userGroup.mediaStartNodeId ? [this._userGroup.mediaStartNodeId] : []}
						@change=${this.#onMediaStartNodeChange}></umb-input-media>
				</umb-workspace-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline"><umb-localize key="user_permissionsDefault"></umb-localize></div>
				<umb-user-group-default-permission-list></umb-user-group-default-permission-list>
			</uui-box>

			<!-- Temp disabled because it is work in progress
			<uui-box>
				<div slot="headline"><umb-localize key="user_permissionsGranular"></umb-localize></div>
				<umb-user-group-granular-permission-list></umb-user-group-granular-permission-list>
			</uui-box>
	-->
		`;
	}

	#renderRightColumn() {
		return html`<uui-box>
				<div slot="headline"><umb-localize key="sections_users"></umb-localize></div>
				<umb-user-input @change=${this.#onUsersChange} .selectedIds=${this._userKeys ?? []}></umb-user-input>
			</uui-box>
			<uui-box headline="Actions">
				<umb-entity-action-list
					.entityType=${UMB_USER_GROUP_ENTITY_TYPE}
					.unique=${this._userGroup?.id}></umb-entity-action-list
			></uui-box>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
			#header {
				width: 100%;
				display: grid;
				grid-template-columns: var(--uui-size-layout-1) 1fr;
			}
			#main {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
			}
			#left-column,
			#right-column {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}
			#right-column > uui-box > div {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbUserGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-editor': UmbUserGroupWorkspaceEditorElement;
	}
}
