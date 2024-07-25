import type { MarkedExtension, Tokens } from '@umbraco-cms/backoffice/external/marked';

export interface UfmPlugin {
	alias: string;
	marker: string;
	render?: (token: UfmToken) => string | undefined;
}

export interface UfmToken extends Tokens.Generic {
	text?: string;
}

export function ufm(plugins: Array<UfmPlugin> = []): MarkedExtension {
	return {
		extensions: plugins.map(({ alias, marker, render }) => {
			return {
				name: alias,
				level: 'inline',
				start: (src: string) => {
					const regex = new RegExp(`\\{${marker}`);
					const match = src.match(regex);
					return match ? match.index : -1;
				},
				tokenizer(src: string): Tokens.Generic | undefined {
					const pattern = `^(?<!\\\\){{?${marker}((?:[a-zA-Z][\\w-]*|[\\{].*?[\\}]+|[\\[].*?[\\]])+)(?<!\\\\)}}?`;
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
