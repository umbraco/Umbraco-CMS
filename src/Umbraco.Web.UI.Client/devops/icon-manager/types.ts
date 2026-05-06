export interface IconEntry {
	name: string;
	file: string;
	keywords?: string[];
	groups?: string[];
	related?: string[];
	legacy?: boolean;
	internal?: boolean;
}

export interface IconDictionary {
	lucide: IconEntry[];
	simpleIcons: IconEntry[];
	umbraco: IconEntry[];
	custom: IconEntry[];
}

export type IconNode = [string, Record<string, string>];

export type IconNodes = Record<string, IconNode[]>;

export type IconTags = Record<string, string[]>;

export type IconCategory = keyof IconDictionary;

export interface ManagedIcon {
	name: string;
	file: string;
	category: IconCategory;
	keywords: string[];
	groups: string[];
	related: string[];
	legacy: boolean;
	internal: boolean;
	svgMarkup: string;
	lucideTags: string[];
	isNew: boolean;
	isDirty: boolean;
}
