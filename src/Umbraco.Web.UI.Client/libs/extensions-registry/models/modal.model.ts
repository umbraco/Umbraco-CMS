import type { ManifestElement } from '.';

export interface ManifestModal extends ManifestElement {
	type: 'modal';
}

export interface ManifestModalTreePickerKind extends ManifestModal {
	type: 'modal';
	kind: 'treePicker';
	meta: MetaModalTreePickerKind;
}

export interface MetaModalTreePickerKind {
	treeAlias: string;
}
