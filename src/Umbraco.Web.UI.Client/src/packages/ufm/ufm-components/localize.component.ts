import { UmbUfmComponentBase } from './ufm-component-base.js';
import type { Tokens } from '@umbraco-cms/backoffice/external/marked';

export class UmbUfmLocalizeComponent extends UmbUfmComponentBase {
	render(token: Tokens.Generic) {
		return `<umb-localize key="${token.text}"></umb-localize>`;
	}
}

export { UmbUfmLocalizeComponent as api };
