import type { UmbMarkedExtensionApi } from './marked-extension.extension.js';
import type { Marked, Tokens } from '@umbraco-cms/backoffice/external/marked';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUfmJsMarkedExtensionApi implements UmbMarkedExtensionApi {
	constructor(_host: UmbControllerHost, marked: Marked) {
		marked.use({
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
		});
	}

	destroy() {}
}

export default UmbUfmJsMarkedExtensionApi;
