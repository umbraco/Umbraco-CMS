import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbModalToken, UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export interface ManifestEntityActionRestoreFromRecycleBinKind
	extends ManifestEntityAction<MetaEntityActionRestoreFromRecycleBinKind> {
	type: 'entityAction';
	kind: 'restoreFromRecycleBin';
}

export interface MetaEntityActionRestoreFromRecycleBinKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken<UmbPickerModalData<any>, UmbPickerModalValue> | string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbRestoreFromRecycleBinEntityActionKind: ManifestEntityActionRestoreFromRecycleBinKind;
	}
}
