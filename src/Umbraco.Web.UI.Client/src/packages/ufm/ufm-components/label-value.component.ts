import { UmbUfmComponentBase } from './ufm-component-base.js';
import type { Tokens } from '@umbraco-cms/backoffice/external/marked';

import './label-value.element.js';

export class UmbUfmLabelValueComponent extends UmbUfmComponentBase {
	render(token: Tokens.Generic) {
		return `<ufm-label-value alias="${token.text}"></ufm-label-value>`;
	}
}

export { UmbUfmLabelValueComponent as api };
