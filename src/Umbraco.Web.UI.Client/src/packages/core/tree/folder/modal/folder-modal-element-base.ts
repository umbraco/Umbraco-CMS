import { css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbFolderModel, UmbFolderRepository } from '@umbraco-cms/backoffice/tree';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbFolderModalElementBase<
	FolderModalDataType extends { folderRepositoryAlias: string },
	FolderModalValueType extends { folder: UmbFolderModel },
> extends UmbModalBaseElement<FolderModalDataType, FolderModalValueType> {
	@state()
	_isNew = false;

	folderRepository?: UmbFolderRepository;

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
				this.folderRepository = permitted ? (ctrl.api as UmbFolderRepository) : undefined;
				this.init();
			},
		);
	}

	abstract init(): void;
	abstract onFormSubmit({ name }: { name: string }): void;

	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		this.onFormSubmit({ name });
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
									.value="${this.value?.folder?.name || ''}"
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
