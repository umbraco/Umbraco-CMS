export interface UmbShortcut {
	unique: string | symbol;
	key: string;
	modifier: boolean;
	shift: boolean;
	alt: boolean;
	label?: string;
	weight?: number;
	callback: () => void | Promise<void>;
}
