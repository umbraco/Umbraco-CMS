export interface UmbElementCurrentUserPermissionsModel {
	/** The unique of the element or element folder. */
	unique: string;
	/** The permission verbs the current user has at that element or element folder. */
	verbs: Array<string>;
}
