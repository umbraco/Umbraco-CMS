export interface UmbShortcut {
	unique: string | symbol;
	key: string;
	label?: string;
	weight?: number;
	callback: () => void | Promise<void>;
}
