import { UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/index.js';
import { UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL } from '../snippet-modal/create-from-snippet-modal.token.js';
import type { UmbPartialViewCreateOptionsModalData } from './index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalManagerContext} from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-partial-view-create-options-modal')
export class UmbPartialViewCreateOptionsModalElement extends UmbModalBaseElement<
	UmbPartialViewCreateOptionsModalData,
	string
> {
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
			UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
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

	async #onCreateFromSnippetClick(event: PointerEvent) {
		event.stopPropagation();
		if (this.data?.parentUnique === undefined) throw new Error('A parent unique is required to create a folder');

		const modalContext = this.#modalManager?.open(UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL, {
			data: {
				parentUnique: this.data.parentUnique,
			},
		});

		modalContext?.onSubmit().then(() => this._submitModal());
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Partial View">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/partial-view/create/${this.data?.parentUnique || 'null'}`}
						label="New empty partial view"
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-article"></uui-icon>}
					</uui-menu-item>

					<uui-menu-item @click=${this.#onCreateFromSnippetClick} label="New partial view from snippet...">
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

export default UmbPartialViewCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-options-modal': UmbPartialViewCreateOptionsModalElement;
	}
}
