import type { UmbElementItemModel } from '../item/repository/types.js';

export interface UmbElementSearchItemModel extends UmbElementItemModel {
	// TODO: [v18] Temporarily added `name` field back in, as the `UmbSearchResultItemModel` (and `UmbNamedEntityModel`) require it. Mirrors `UmbDocumentSearchItemModel`.
	name: string;
	href: string;
	ancestors: Array<UmbElementSearchAncestorModel>;
}

export interface UmbElementSearchAncestorModel {
	unique: string;
	name: string;
}
