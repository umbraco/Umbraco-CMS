import type { UfmToken } from '../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from './ufm-component-base.js';

import './label-value.element.js';

export class UmbUfmLabelValueComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;
		return `<ufm-label-value alias="${token.text}"></ufm-label-value>`;
	}
}

export { UmbUfmLabelValueComponent as api };
