import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbRestoreFromRecycleBinModalData {
	unique: string;
	entityType: string;
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken<UmbPickerModalData<any>, UmbPickerModalValue> | string;
}

export interface UmbRestoreFromRecycleBinModalValue {
	destination: UmbEntityModel | undefined;
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
