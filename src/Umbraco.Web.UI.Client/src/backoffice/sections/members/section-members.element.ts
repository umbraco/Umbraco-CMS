import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import '../shared/section-trees.element.ts';

@customElement('umb-section-members')
export class UmbSectionMembers extends LitElement {
	render() {
		return html`
			<umb-section-layout>
				<umb-section-sidebar>
					<umb-section-trees></umb-section-trees>
				</umb-section-sidebar>
				<umb-section-main></umb-section-main>
					<umb-section-dashboards></umb-section-dashboards>
				</umb-section-main>
			</umb-section-layout>
		`;
	}
}

export default UmbSectionMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-members': UmbSectionMembers;
	}
}
