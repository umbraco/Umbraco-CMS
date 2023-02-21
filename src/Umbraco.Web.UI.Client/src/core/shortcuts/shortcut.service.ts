export type Shortcut = {
	key: string;
	altKey?: boolean;
	ctrlKey?: boolean;
	shiftKey?: boolean;
	metaKey?: boolean;
	callback(): void;
};

export class UmbShortcutService {
	#shortcuts: Array<Shortcut> = [
		{
			key: 'k',
			metaKey: true,
			callback: () => console.log('Open search'),
		},
	];

	constructor() {
		addEventListener('keydown', (event: KeyboardEvent) => {
			if (!event.altKey && !event.ctrlKey && !event.shiftKey && !event.metaKey) return;
			if (event.key === 'Shift' || event.key === 'Control' || event.key === 'Alt' || event.key === 'Meta') return;

			const shortcut = this.#shortcuts.find((x) => {
				if (x.key !== event.key) return false;
				if ((x.altKey ? true : false) !== event.altKey) return false;
				if ((x.ctrlKey ? true : false) !== event.ctrlKey) return false;
				if ((x.shiftKey ? true : false) !== event.shiftKey) return false;
				if ((x.metaKey ? true : false) !== event.metaKey) return false;
				return true;
			});

			shortcut?.callback();
		});
	}
}

export default UmbShortcutService;

declare global {
	interface HTMLElementTagNameMap {
		'umb-shortcut': UmbShortcutService;
	}
}
