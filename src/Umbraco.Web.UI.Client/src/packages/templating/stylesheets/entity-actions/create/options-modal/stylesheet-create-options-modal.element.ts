import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/index.js';
import type { UmbStylesheetCreateOptionsModalData } from './index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-stylesheet-create-options-modal')
export class UmbStylesheetCreateOptionsModalElement extends UmbModalBaseElement<
	UmbStylesheetCreateOptionsModalData,
	string
> {
	#createFolderAction?: UmbCreateFolderEntityAction<any>;

	connectedCallback(): void {
		super.connectedCallback();

		if (this.data?.parentUnique === undefined) throw new Error('A parent unique is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(
			this,
			UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			// TODO: allow null for entity actions. Some actions can be executed on the root item
			this.data.parentUnique,
			this.data.entityType,
		);
	}

	async #onCreateFolderClick(event: PointerEvent) {
		event.stopPropagation();

		try {
			await this.#createFolderAction?.execute();
			this._submitModal();
		} catch (error) {
			console.error(error);
		}
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Stylesheet">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/stylesheet/create/${this.data?.parentUnique || 'null'}/view/code`}
						label="New Stylesheet"
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-article"></uui-icon>}
					</uui-menu-item>

					<uui-menu-item
						href=${`section/settings/workspace/stylesheet/create/${
							this.data?.parentUnique || 'null'
						}/view/rich-text-editor`}
						label="New Rich Text Editor Stylesheet"
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-article"></uui-icon>}
					</uui-menu-item>

					<uui-menu-item @click=${this.#onCreateFolderClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>}
					</uui-menu-item>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbStylesheetCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-create-options-modal': UmbStylesheetCreateOptionsModalElement;
	}
}
