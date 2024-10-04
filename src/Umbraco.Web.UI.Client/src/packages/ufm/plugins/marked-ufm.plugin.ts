import type { MarkedExtension, Tokens } from '@umbraco-cms/backoffice/external/marked';

export interface UfmPlugin {
	alias: string;
	marker: string;
	render?: (token: UfmToken) => string | undefined;
}

export interface UfmToken extends Tokens.Generic {
	text?: string;
}

/**
 *
 * @param {Array<UfmPlugin>} plugins - An array of UFM plugins.
 * @returns {MarkedExtension} A Marked extension object.
 */
export function ufm(plugins: Array<UfmPlugin> = []): MarkedExtension {
	return {
		extensions: plugins.map(({ alias, marker, render }) => {
			return {
				name: alias,
				level: 'inline',
				start: (src: string) => src.indexOf(`{${marker}`),
				tokenizer: (src: string) => {
					const pattern = `^\\{${marker}([^}]*)\\}`;
					const regex = new RegExp(pattern);
					const match = src.match(regex);

					if (match) {
						const [raw, content = ''] = match;
						return {
							type: alias,
							raw: raw,
							tokens: [],
							text: content.trim(),
						};
					}

					return undefined;
				},
				renderer: render,
			};
		}),
	};
}
