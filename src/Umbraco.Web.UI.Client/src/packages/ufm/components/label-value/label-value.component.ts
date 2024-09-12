import type { UfmToken } from '../../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './label-value.element.js';

export class UmbUfmLabelValueComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		const attributes = super.getAttributes(token.text);
		return `<ufm-label-value ${attributes}></ufm-label-value>`;
	}
}

export { UmbUfmLabelValueComponent as api };
