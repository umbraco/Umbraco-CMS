import type { UmbDocumentTypeEntityType } from '../../entity.js';

export type UmbDocumentTypeItemModel = {
	entityType: UmbDocumentTypeEntityType;
	unique: string;
	name: string;
	isElement: boolean;
	icon?: string | null;
	description?: string | null;
};
