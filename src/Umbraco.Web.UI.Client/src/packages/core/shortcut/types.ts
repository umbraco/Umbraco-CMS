export interface UmbShortcut {
	unique: string | symbol;
	key: string;
	modifier: boolean;
	shift: boolean;
	alt: boolean;
	label?: string;
	weight?: number;
	action: () => void | Promise<void>;
	// TODO: Implement a global option, to make a shortcut be available despite children setting up their own inheritance scopes. [NL]
}
