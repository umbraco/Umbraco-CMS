export interface UmbIconDefinition {
	name: string;
	path: string;
	legacy?: boolean;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;
