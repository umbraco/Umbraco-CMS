import { UmbUfmComponentBase } from '../ufm-component-base.js';
import type { UfmToken } from '../../plugins/types.js';

import './member-name.element.js';

export class UmbUfmMemberNameComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		if (!token.text) return;

		const attributes = super.getAttributes(token.text);
		return `<ufm-member-name ${attributes}></ufm-member-name>`;
	}
}

export { UmbUfmMemberNameComponent as api };
