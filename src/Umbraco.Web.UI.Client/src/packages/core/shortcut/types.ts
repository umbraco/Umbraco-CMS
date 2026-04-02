export interface UmbShortcut {
	unique: string | symbol;
	key: string;
	modifier: boolean;
	shift: boolean;
	alt: boolean;
	label?: string;
	weight?: number;
	action: () => void | Promise<void>;
	// TODO: Consider implementing a global option, to make a shortcut be available despite children setting up their own inheritance scopes. [NL]
	// TODO: Addition thought, also a bit dangerous cause how do you know the interest of the children. [NL]
}
