import { UmbUserGroupWorkspaceContext } from './user-group-workspace.context.js';
import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
// TODO: import from package when available
//import { UmbUserInputElement } from '../../users/components/user-input/user-input.element.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_CONFIRM_MODAL, UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-user-group-workspace-editor')
export class UmbUserGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _userGroup?: UserGroupResponseModel;

	@state()
	private _userKeys?: Array<string>;

	#workspaceContext?: UmbUserGroupWorkspaceContext;
	#modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbUserGroupWorkspaceContext;
			this.observe(this.#workspaceContext.data, (userGroup) => (this._userGroup = userGroup));
			this.observe(this.#workspaceContext.userIds, (userKeys) => (this._userKeys = userKeys));
		});

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	#onUsersChange(userIds: Array<string>) {
		this.#workspaceContext?.updateUserKeys(userIds);
	}

	#onSectionsChange(value: string[]) {
		this.#workspaceContext?.updateProperty('sections', value);
	}

	async #onDelete() {
		if (!this.#modalContext || !this.#workspaceContext) return;

		const modalHandler = this.#modalContext.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Delete user group ${this._userGroup?.name}?`,
			content: html`Are you sure you want to delete <b>${this._userGroup?.name}</b> user group?`,
			confirmLabel: 'Delete',
		});

		await modalHandler.onSubmit();

		if (!this._userGroup || !this._userGroup.id) return;

		await this.#workspaceContext.delete(this._userGroup?.id);
		//TODO: should we check if it actually succeeded in deleting the user group?

		history.pushState(null, '', 'section/users/view/user-groups');
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
			<umb-workspace-editor alias="Umb.Workspace.UserGroup">
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
					label="name"
					.value=${this._userGroup?.name ?? ''}
					@input="${this.#onNameChange}"></uui-input>
			</div>
		`;
	}

	#renderLeftColumn() {
		if (!this._userGroup) return nothing;

		return html` <uui-box>
				<div slot="headline">Assign access</div>
				<umb-workspace-property-layout label="Sections" description="Add sections to give users access">
					<umb-input-section
						slot="editor"
						.value=${this._userGroup.sections ?? []}
						@change=${(e: any) => this.#onSectionsChange(e.target.value)}></umb-input-section>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Content start node"
					description="Limit the content tree to a specific start node">
					<b slot="editor">CONTENT START NODE PICKER NOT IMPLEMENTED YET</b>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Media start node"
					description="Limit the media library to a specific start node">
					<b slot="editor">MEDIA START NODE PICKER NOT IMPLEMENTED YET</b>
				</umb-workspace-property-layout>
			</uui-box>

			<uui-box>
				<div slot="headline">Default Permissions</div>
				<b>PERMISSIONS NOT IMPLEMENTED YET</b>
			</uui-box>

			<uui-box>
				<div slot="headline">Granular permissions</div>
				<b>PERMISSIONS NOT IMPLEMENTED YET</b>
			</uui-box>`;
	}

	#renderRightColumn() {
		return html`<uui-box>
				<div slot="headline">Users</div>
				<!-- change any to UmbUserInputElement when package is available -->
				<umb-user-input
					@change=${(e: Event) => this.#onUsersChange((e.target as any).selectedIds)}
					.selectedIds=${this._userKeys ?? []}></umb-user-input>
			</uui-box>
			<uui-box>
				<div slot="headline">Delete user group</div>
				<uui-button
					@click=${this.#onDelete}
					style="width: 100%"
					color="danger"
					look="secondary"
					label="Delete"></uui-button>
			</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
			#header {
				width: 100%;
				display: grid;
				grid-template-columns: var(--uui-size-layout-1) 1fr;
				padding: var(--uui-size-layout-1);
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
