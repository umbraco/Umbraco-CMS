import type { UmbElementItemModel } from '../item/repository/types.js';

export interface UmbElementSearchItemModel extends UmbElementItemModel {
	href: string;
	ancestors: Array<UmbElementSearchAncestorModel>;
}

export interface UmbElementSearchAncestorModel {
	unique: string;
	name: string;
}
