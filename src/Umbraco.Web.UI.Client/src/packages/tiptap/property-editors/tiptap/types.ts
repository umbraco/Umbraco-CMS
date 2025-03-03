export type UmbTiptapToolbarExtension = {
	alias: string;
	label: string;
	icon: string;
	dependencies?: Array<string>;
};

export type UmbTiptapToolbarSortableViewModel<T> = { unique: string; data: T };
export type UmbTiptapToolbarRowViewModel = UmbTiptapToolbarSortableViewModel<Array<UmbTiptapToolbarGroupViewModel>>;
export type UmbTiptapToolbarGroupViewModel = UmbTiptapToolbarSortableViewModel<Array<string>>;
