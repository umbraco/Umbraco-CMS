import type { MarkedExtension, Tokens } from '@umbraco-cms/backoffice/external/marked';

/**
 * @returns {MarkedExtension} A Marked extension object.
 */
export function ufmjs(): MarkedExtension {
	return {
		extensions: [
			{
				name: 'ufmjs',
				level: 'inline',
				start: (src: string) => src.search(/(?<!\\)\$\{/),
				tokenizer: (src: string) => {
					const pattern = /^\$\{((?:[^{}]|\{[^{}]*\})*)\}/;
					const regex = new RegExp(pattern);
					const match = src.match(regex);

					if (match) {
						const [raw, text] = match;
						return {
							type: 'ufmjs',
							raw: raw,
							tokens: [],
							text: text.trim(),
						};
					}

					return undefined;
				},
				renderer: (token: Tokens.Generic) => {
					return `<umb-ufm-js-expression>${token.text}</umb-ufm-js-expression>`;
				},
			},
		],
	};
}
