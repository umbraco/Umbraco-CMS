export interface UmbWorkspacePropertySetContextInterface {
	propertyValueByAlias(alias: string): void;
	getPropertyValue(alias: string): void;
	setPropertyValue(alias: string, value: unknown): void;
}
