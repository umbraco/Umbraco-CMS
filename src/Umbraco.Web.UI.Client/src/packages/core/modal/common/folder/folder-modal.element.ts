import { css, html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbFolderModalData, UmbFolderModalResult, UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbFolderRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionClass, ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { FolderReponseModel, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-folder-modal')
export class UmbFolderModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalContext<UmbFolderModalData, UmbFolderModalResult>;

	private _data?: UmbFolderModalData;
	@property({ type: Object, attribute: false })
	public get data() {
		return this._data;
	}
	public set data(value: UmbFolderModalData | undefined) {
		this._data = value;
		this.#unique = value?.unique || null;
		this.#repositoryAlias = value?.repositoryAlias;
		this.#observeRepository();
	}

	#repositoryAlias?: string;
	#unique: string | null = null;
	#repository?: UmbFolderRepository;
	#repositoryObserver?: UmbObserverController<ManifestBase | undefined>;

	@state()
	_folder?: FolderReponseModel;

	@state()
	_headline?: string;

	@state()
	_isNew = false;

	#observeRepository() {
		this.#repositoryObserver?.destroy();
		if (!this.#repositoryAlias) return;
		this.#repositoryObserver = this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('repository', this.#repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<UmbFolderRepository>(repositoryManifest, [this]);
					this.#repository = result;
					this.#init();
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + this.#repositoryAlias + '');
				}
			}
		);
	}

	// TODO: so I ended up building a full workspace in the end. We should look into building the real workspace folder editor
	// and see if we can use that in this modal instead of this custom logic.
	#init() {
		if (this.#unique) {
			this.#load();
		} else {
			this.#create();
		}
	}

	async #create() {
		if (!this.#repository) throw new Error('Repository is required to create folder');
		const { data } = await this.#repository.createFolderScaffold(this.#unique);
		this._folder = data;
		this._isNew = true;
	}

	async #load() {
		if (!this.#unique) throw new Error('Unique is required to load folder');
		if (!this.#repository) throw new Error('Repository is required to create folder');
		const { data } = await this.#repository.requestFolder(this.#unique);
		this._folder = data;
		this._isNew = false;
	}

	@query('#dataTypeFolderForm')
	private _formElement?: HTMLFormElement;

	#onCancel() {
		this.modalHandler?.reject();
	}

	#submitForm() {
		this._formElement?.requestSubmit();
	}

	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();
		if (!this._folder) throw new Error('Folder is not initialized correctly');
		if (!this.#repository) throw new Error('Repository is required to create folder');

		const isValid = this._formElement?.checkValidity();
		if (!isValid) return;

		let error: ProblemDetailsModel | undefined;

		const formData = new FormData(this._formElement);
		const folderName = formData.get('name') as string;

		this._folder = { ...this._folder, name: folderName };

		if (this._isNew) {
			const { error: createError } = await this.#repository.createFolder(this._folder);
			error = createError;
		} else {
			if (!this.#unique) throw new Error('Unique is required to update folder');
			const { error: updateError } = await this.#repository.updateFolder(this.#unique, this._folder);
			error = updateError;
		}

		if (!error) {
			this.modalHandler?.submit();
		}
	}

	render() {
		return html`
			<umb-body-layout headline=${this._isNew ? 'Create Folder' : 'Update Folder'}>
				<uui-box>
					<uui-form>
						<form id="dataTypeFolderForm" name="data" @submit="${this.#onSubmit}">
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

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this.#onCancel}"></uui-button>
				<uui-button
					type="submit"
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label=${this._isNew ? 'Create Folder' : 'Update Folder'}
					@click=${this.#submitForm}></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
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
