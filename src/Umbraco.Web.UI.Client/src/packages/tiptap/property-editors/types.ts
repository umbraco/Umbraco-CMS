export type UmbTiptapSortableViewModel<T> = { unique: string; data: T };

export type UmbTiptapExtensionBase = {
	kind?: string;
	alias: string;
	label: string;
	icon: string;
	dependencies?: Array<string>;
};

export type UmbTiptapStatusbarExtension = UmbTiptapExtensionBase;

export type UmbTiptapStatusbarViewModel = UmbTiptapSortableViewModel<Array<string>>;

export type UmbTiptapToolbarExtension = UmbTiptapExtensionBase;

export type UmbTiptapToolbarRowViewModel = UmbTiptapSortableViewModel<Array<UmbTiptapToolbarGroupViewModel>>;
export type UmbTiptapToolbarGroupViewModel = UmbTiptapSortableViewModel<Array<string>>;

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export type UmbTiptapToolbarSortableViewModel<T> = UmbTiptapSortableViewModel<T>;
