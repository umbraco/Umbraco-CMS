import type { UfmToken } from '../../plugins/marked-ufm.plugin.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './link.element.js';

export class UmbUfmLinkComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		const attributes = super.getAttributes(token.text);
		return `<ufm-link ${attributes}></ufm-link>`;
	}
}

export { UmbUfmLinkComponent as api };
