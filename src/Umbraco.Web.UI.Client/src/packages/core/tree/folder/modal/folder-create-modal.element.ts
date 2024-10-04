import { UmbFolderModalElementBase } from './folder-modal-element-base.js';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbFolderCreateModalData, UmbFolderCreateModalValue, UmbFolderModel } from '@umbraco-cms/backoffice/tree';

@customElement('umb-folder-create-modal')
export class UmbFolderCreateModalElement extends UmbFolderModalElementBase<
	UmbFolderCreateModalData,
	UmbFolderCreateModalValue
> {
	@state()
	_folderScaffold?: UmbFolderModel;

	constructor() {
		super();
		this._isNew = true;
	}

	async init() {
		if (!this.folderRepository) throw new Error('A folder repository is required to create a folder');

		const { data } = await this.folderRepository.createScaffold();

		if (data) {
			this._folderScaffold = data;
		}
	}

	async onFormSubmit({ name }: { name: string }): Promise<void> {
		if (!this.folderRepository) throw new Error('A folder repository is required to create a folder');
		if (!this._folderScaffold) throw new Error('The folder scaffold was not initialized correctly');
		if (!this.data?.parent) throw new Error('A parent is required to create folder');

		const folder: UmbFolderModel = {
			...this._folderScaffold,
			name,
		};

		const { data: createdFolder } = await this.folderRepository.create(folder, this.data.parent.unique);

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
