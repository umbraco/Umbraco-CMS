import type { UfmToken } from '../../plugins/types.js';
import { UmbUfmComponentBase } from '../ufm-component-base.js';

import './localize.element.js';

export class UmbUfmLocalizeComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		const attributes = super.getAttributes(token.text);
		return `<ufm-localize ${attributes}></ufm-localize>`;
	}
}

export { UmbUfmLocalizeComponent as api };
