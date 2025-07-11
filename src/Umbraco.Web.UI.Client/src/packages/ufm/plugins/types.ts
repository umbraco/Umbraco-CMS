import type { Tokens } from '@umbraco-cms/backoffice/external/marked';

export interface UfmPlugin {
	alias: string;
	marker?: string;
	render?: (token: UfmToken) => string | undefined;
}

export interface UfmToken extends Tokens.Generic {
	prefix: string;
	text?: string;
}
