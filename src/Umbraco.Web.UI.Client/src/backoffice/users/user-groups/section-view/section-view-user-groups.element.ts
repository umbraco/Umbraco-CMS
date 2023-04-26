import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import '../workspace/workspace-view-user-groups.element';

//TODO: rename to user-groups-section-view
@customElement('umb-section-view-user-groups')
export class UmbSectionViewUserGroupsElement extends LitElement {
	

	render() {
		return html`<umb-workspace-view-user-groups></umb-workspace-view-user-groups>`;
	}
	
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
			}

			#router-slot {
				height: calc(100% - var(--umb-header-layout-height) - var(--umb-footer-layout-height));
			}
		`,
	];
}

export default UmbSectionViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-user-groups': UmbSectionViewUserGroupsElement;
	}
}
