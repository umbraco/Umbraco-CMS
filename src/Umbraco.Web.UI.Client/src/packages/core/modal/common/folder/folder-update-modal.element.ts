import { UmbFolderModalElementBase } from './folder-modal-element-base.js';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbFolderUpdateModalData, UmbFolderUpdateModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbFolderModel } from '@umbraco-cms/backoffice/repository';

@customElement('umb-folder-update-modal')
export class UmbFolderModalElement extends UmbFolderModalElementBase<
	UmbFolderUpdateModalData,
	UmbFolderUpdateModalValue
> {
	@state()
	_folder?: UmbFolderModel;

	async init() {
		if (!this.folderRepository) throw new Error('A folder repository is required to load a folder');
		if (!this.data?.unique) throw new Error('A unique is required to load folder');

		const { data } = await this.folderRepository.request(this.data.unique);

		if (data) {
			this._folder = data;
		}
	}

	async onFormSubmit({ name }: { name: string }) {
		if (!this.folderRepository) throw new Error('A folder repository is required to update a folder');
		if (this._folder === undefined) throw new Error('The folder was not initialized correctly');

		const { data } = await this.folderRepository.update({
			...this._folder,
			name,
		});

		if (data) {
			this.value = { folder: data };
			this._submitModal();
		}
	}
}

export default UmbFolderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-folder-update-modal': UmbFolderModalElement;
	}
}
