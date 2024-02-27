import type { UmbUserGroupDetailModel } from '../index.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../index.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from './user-group-workspace.context.js';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputSectionElement } from '@umbraco-cms/backoffice/components';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';

import './components/user-group-entity-user-permission-list.element.js';
import './components/user-group-granular-permission-list.element.js';

@customElement('umb-user-group-workspace-editor')
export class UmbUserGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _userGroup?: UmbUserGroupDetailModel;

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup), 'umbUserGroupObserver');
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
		this.#workspaceContext?.updateProperty('documentStartNode', { unique: target.selectedIds[0] });
	}

	#onMediaStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputMediaElement;
		this.#workspaceContext?.updateProperty('mediaStartNode', { unique: target.selectedIds[0] });
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
					<uui-icon name="icon-arrow-left"></uui-icon>
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
				<umb-property-layout
					label=${this.localize.term('main_sections')}
					description=${this.localize.term('user_sectionsHelp')}>
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections ?? []}
						@change=${this.#onSectionsChange}></umb-input-section>
				</umb-property-layout>
				<umb-property-layout
					label=${this.localize.term('defaultdialogs_selectContentStartNode')}
					description=${this.localize.term('user_startnodehelp')}>
					<umb-input-document
						slot="editor"
						max="1"
						.selectedIds=${this._userGroup.documentStartNode?.unique ? [this._userGroup.documentStartNode.unique] : []}
						@change=${this.#onDocumentStartNodeChange}></umb-input-document>
				</umb-property-layout>
				<umb-property-layout
					label=${this.localize.term('defaultdialogs_selectMediaStartNode')}
					description=${this.localize.term('user_mediastartnodehelp')}>
					<umb-input-media
						slot="editor"
						max="1"
						.selectedIds=${this._userGroup.mediaStartNode?.unique ? [this._userGroup.mediaStartNode.unique] : []}
						@change=${this.#onMediaStartNodeChange}></umb-input-media>
				</umb-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline"><umb-localize key="user_permissionsDefault"></umb-localize></div>

				<umb-property-layout label="Entity permissions" description="Assign permissions for an entity type">
					<umb-user-group-entity-user-permission-list slot="editor"></umb-user-group-entity-user-permission-list>
				</umb-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline"><umb-localize key="user_permissionsGranular"></umb-localize></div>
				<umb-user-group-granular-permission-list></umb-user-group-granular-permission-list>
			</uui-box>
		`;
	}

	#renderRightColumn() {
		return html` <uui-box headline="Actions">
			<umb-entity-action-list
				.entityType=${UMB_USER_GROUP_ENTITY_TYPE}
				.unique=${this._userGroup?.unique}></umb-entity-action-list
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
