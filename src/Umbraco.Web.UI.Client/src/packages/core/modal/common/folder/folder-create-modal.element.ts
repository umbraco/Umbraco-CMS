import { UmbFolderModalElementBase } from './folder-modal-element-base.js';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbFolderCreateModalData, UmbFolderCreateModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbFolderScaffoldModel } from '@umbraco-cms/backoffice/repository';

@customElement('umb-folder-create-modal')
export class UmbFolderCreateModalElement extends UmbFolderModalElementBase<
	UmbFolderCreateModalData,
	UmbFolderCreateModalValue
> {
	@state()
	_folderScaffold?: UmbFolderScaffoldModel;

	async init() {
		if (!this.folderRepository) throw new Error('A folder repository is required to create a folder');
		if (!this.data?.parentUnique) throw new Error('A parent unique is required to create folder');

		const { data } = await this.folderRepository.createScaffold(this.data.parentUnique);

		if (data) {
			this._folderScaffold = data;
		}
	}

	async onFormSubmit({ name }: { name: string }): Promise<void> {
		if (!this.folderRepository) throw new Error('A folder repository is required to create a folder');
		if (!this._folderScaffold) throw new Error('The folder scaffold was not initialized correctly');

		const data = {
			...this._folderScaffold,
			name,
		};

		const { data: createdFolder } = await this.folderRepository.create(data);

		if (createdFolder) {
			this.value = { folder: createdFolder };
			this._submitModal();
		}
	}
}

export default UmbFolderCreateModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-folder-create-modal': UmbFolderCreateModalElement;
	}
}
