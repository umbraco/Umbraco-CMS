import type { UfmToken } from '../plugins/types.js';
import type { UmbUfmComponentApi } from '../extensions/ufm-component.extension.js';

export abstract class UmbUfmComponentBase implements UmbUfmComponentApi {
	protected getAttributes(text: string): string | null {
		if (!text) return null;

		const pipeIndex = text.indexOf('|');

		if (pipeIndex === -1) {
			return `alias="${text.trim()}"`;
		}

		const alias = text.substring(0, pipeIndex).trim();
		const filters = text.substring(pipeIndex + 1).trim();

		return Object.entries({ alias, filters })
			.map(([key, value]) => (value ? `${key}="${value.trim()}"` : null))
			.join(' ');
	}

	abstract render(token: UfmToken): string | undefined;

	destroy() {}
}
