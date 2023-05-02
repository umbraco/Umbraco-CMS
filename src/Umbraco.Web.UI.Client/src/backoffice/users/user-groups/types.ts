import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';

export interface UserGroupEntity extends UmbEntityBase {
	type: 'user-group';
	icon?: string;
}

export interface UserGroupDetails extends UserGroupEntity {
	sections: Array<string>;
	contentStartNode?: string;
	mediaStartNode?: string;
	permissions: Array<string>;
}
