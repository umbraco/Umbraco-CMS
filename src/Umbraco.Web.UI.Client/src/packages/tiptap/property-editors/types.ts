export type UmbTiptapSortableViewModel<T> = { unique: string; data: T };

export type UmbTiptapStatusbarExtension = {
	alias: string;
	label: string;
	icon: string;
	dependencies?: Array<string>;
};

export type UmbTiptapStatusbarViewModel = UmbTiptapSortableViewModel<Array<string>>;

export type UmbTiptapToolbarExtension = UmbTiptapStatusbarExtension & {
	kind?: string;
};

export type UmbTiptapToolbarRowViewModel = UmbTiptapSortableViewModel<Array<UmbTiptapToolbarGroupViewModel>>;
export type UmbTiptapToolbarGroupViewModel = UmbTiptapSortableViewModel<Array<string>>;
