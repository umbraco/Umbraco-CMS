import { UmbFolderModalElementBase } from './folder-modal-element-base.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbFolderUpdateModalData, UmbFolderUpdateModalValue } from './folder-update-modal.token.js';

@customElement('umb-folder-update-modal')
export class UmbFolderModalElement extends UmbFolderModalElementBase<
	UmbFolderUpdateModalData,
	UmbFolderUpdateModalValue
> {
	async init() {
		if (!this.folderRepository) throw new Error('A folder repository is required to load a folder');
		if (!this.data?.unique) throw new Error('A unique is required to load folder');

		const { data } = await this.folderRepository.requestByUnique(this.data.unique);

		if (data) {
			this.value = { folder: data };
		}
	}

	async onFormSubmit({ name }: { name: string }) {
		if (!this.folderRepository) throw new Error('A folder repository is required to update a folder');
		if (this.value.folder === undefined) throw new Error('The folder was not initialized correctly');

		const { data } = await this.folderRepository.save({
			...this.value.folder,
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
