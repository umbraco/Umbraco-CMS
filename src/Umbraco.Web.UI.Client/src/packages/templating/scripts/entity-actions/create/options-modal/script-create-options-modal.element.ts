import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/index.js';
import type { UmbScriptCreateOptionsModalData } from './index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalManagerContext} from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-script-create-options-modal')
export class UmbScriptCreateOptionsModalElement extends UmbModalBaseElement<UmbScriptCreateOptionsModalData, string> {
	#modalManager?: UmbModalManagerContext;
	#createFolderAction?: UmbCreateFolderEntityAction<any>;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManager = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		if (this.data?.parentUnique === undefined) throw new Error('A parent unique is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(
			this,
			UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
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
			<umb-body-layout headline="Create Script">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/script/create/${this.data?.parentUnique || 'null'}`}
						label="New Javascript file"
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

export default UmbScriptCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-create-options-modal': UmbScriptCreateOptionsModalElement;
	}
}
