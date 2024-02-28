export interface UmbCurrentUserModel {
	unique: string;
	email: string;
	userName: string;
	name: string;
	languageIsoCode: string;
	documentStartNodeUniques: Array<string>;
	mediaStartNodeUniques: Array<string>;
	avatarUrls: Array<string>;
	languages: Array<string>;
	hasAccessToAllLanguages: boolean;
	permissions: Array<string>;
	sections: Array<string>;
}
