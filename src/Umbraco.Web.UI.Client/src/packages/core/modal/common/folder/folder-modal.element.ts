import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbFolderModalData, UmbFolderModalValue, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbCreateFolderModel,
	UmbFolderModel,
	UmbFolderRepository,
	UmbFolderScaffoldModel,
	UmbUpdateFolderModel,
} from '@umbraco-cms/backoffice/repository';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-folder-modal')
export class UmbFolderModalElement extends UmbModalBaseElement<UmbFolderModalData, UmbFolderModalValue> {
	@state()
	_folder?: UmbFolderModel;

	@state()
	_folderScaffold?: UmbFolderScaffoldModel;

	@state()
	_headline?: string;

	@state()
	_isNew = false;

	#folderRepository?: UmbFolderRepository;

	connectedCallback(): void {
		super.connectedCallback();
		this.#observeRepository();
	}

	#observeRepository() {
		if (!this.data?.folderRepositoryAlias) throw new Error('A folder repository alias is required');

		new UmbExtensionApiInitializer(
			this,
			umbExtensionsRegistry,
			this.data.folderRepositoryAlias,
			[this],
			(permitted, ctrl) => {
				this.#folderRepository = permitted ? (ctrl.api as UmbFolderRepository) : undefined;
				this.#init();
			},
		);
	}

	#init() {
		if (this.data?.unique) {
			this.#load();
		} else {
			this.#createScaffold();
		}
	}

	async #createScaffold() {
		if (!this.#folderRepository) throw new Error('A folder repository is required to create a folder');
		if (!this.data?.parentUnique) throw new Error('A parent unique is required to create folder');

		const { data } = await this.#folderRepository.createScaffold(this.data.parentUnique);

		if (data) {
			this._folderScaffold = data;
			this._isNew = true;
		}
	}

	async #load() {
		if (!this.#folderRepository) throw new Error('A folder repository is required to load a folder');
		if (!this.data?.unique) throw new Error('A unique is required to load folder');

		const { data } = await this.#folderRepository.request(this.data.unique);

		if (data) {
			this._folder = data;
			this._isNew = false;
		}
	}

	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		if (this._isNew) {
			if (!this._folderScaffold) throw new Error('A folder scaffold has not been loaded to create a folder');
			this.#create({ ...this._folderScaffold, name });
		} else {
			if (!this._folder) throw new Error('A folder has not been loaded to update');
			this.#update({ unique: this._folder.unique, name });
		}
	}

	async #create(data: UmbCreateFolderModel) {
		if (!this.#folderRepository) throw new Error('A folder repository is required to create a folder');
		const { error } = await this.#folderRepository.create(data);

		if (!error) {
			this._submitModal();
		}
	}

	async #update(data: UmbUpdateFolderModel) {
		if (!this.#folderRepository) throw new Error('A folder repository is required to update a folder');
		const { error } = await this.#folderRepository.update(data);

		if (!error) {
			this._submitModal();
		}
	}

	render() {
		return html`
			<umb-body-layout headline=${this._isNew ? 'Create Folder' : 'Update Folder'}>
				<uui-box>
					<uui-form>
						<form id="FolderForm" @submit="${this.#onSubmit}">
							<uui-form-layout-item>
								<uui-label id="nameLabel" for="name" slot="label" required>Folder name</uui-label>
								<uui-input
									type="text"
									id="name"
									name="name"
									placeholder="Enter folder name..."
									.value="${this._folder?.name}"
									required
									required-message="Folder name is required"></uui-input>
							</uui-form-layout-item>
						</form>
					</uui-form>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button
					form="FolderForm"
					type="submit"
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label=${this._isNew ? 'Create Folder' : 'Update Folder'}></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#name {
				width: 100%;
			}
		`,
	];
}

export default UmbFolderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-folder-modal': UmbFolderModalElement;
	}
}
