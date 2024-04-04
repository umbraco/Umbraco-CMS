import type { UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbRestoreFromRecycleBinModalData {
	unique: string;
	entityType: string;
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken<UmbTreePickerModalData<UmbUniqueTreeItemModel | UmbUniqueTreeRootModel>> | string;
}

export interface UmbRestoreFromRecycleBinModalValue {
	destination:
		| {
				unique: string | null;
				entityType: string;
		  }
		| undefined;
}

export const UMB_RESTORE_FROM_RECYCLE_BIN_MODAL = new UmbModalToken<
	UmbRestoreFromRecycleBinModalData,
	UmbRestoreFromRecycleBinModalValue
>('Umb.Modal.RecycleBin.Restore', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
