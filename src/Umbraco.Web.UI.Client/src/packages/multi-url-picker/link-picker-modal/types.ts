export type UmbLinkPickerLinkType = 'document' | 'external' | 'media';

export interface UmbLinkPickerLink {
	icon?: string | null;
	name?: string | null;
	published?: boolean | null;
	queryString?: string | null;
	target?: string | null;
	trashed?: boolean | null;
	type?: UmbLinkPickerLinkType | null;
	unique?: string | null;
	url?: string | null;
}
