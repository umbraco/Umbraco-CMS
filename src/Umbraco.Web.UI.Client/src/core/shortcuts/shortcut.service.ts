import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbSearchService } from 'src/backoffice/search/search.service';

export type Shortcut = {
	name: string;
	alias: string;
	action(): void;
	combinations: Array<{
		key: string;
		altKey?: boolean;
		ctrlKey?: boolean;
		shiftKey?: boolean;
		metaKey?: boolean;
	}>;
};

export class UmbShortcutService {
	#host: UmbControllerHostInterface;
	#shortcuts: Array<Shortcut> = [
		{
			name: 'Open search',
			alias: 'Shortcut.OpenSearch',
			action: () => UmbSearchService.Open(),
			combinations: [
				{
					key: 'k',
					metaKey: true,
				},
				{
					key: 'k',
					ctrlKey: true,
				},
			],
		},
	];

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextProviderController(host, UMB_SHORTCUT_CONTEXT_TOKEN, this);

		addEventListener('keydown', (event: KeyboardEvent) => {
			if (!event.altKey && !event.ctrlKey && !event.shiftKey && !event.metaKey) return;
			if (event.key === 'Shift' || event.key === 'Control' || event.key === 'Alt' || event.key === 'Meta') return;

			const shortcut = this.#shortcuts.find((x) => {
				return x.combinations.find((y) => {
					if (y.key !== event.key) return false;
					if ((y.altKey ? true : false) !== event.altKey) return false;
					if ((y.ctrlKey ? true : false) !== event.ctrlKey) return false;
					if ((y.shiftKey ? true : false) !== event.shiftKey) return false;
					if ((y.metaKey ? true : false) !== event.metaKey) return false;
					return true;
				});
			});

			shortcut?.action();
		});
	}
}

export const UMB_SHORTCUT_CONTEXT_TOKEN = new UmbContextToken<UmbShortcutService>(UmbShortcutService.name);
