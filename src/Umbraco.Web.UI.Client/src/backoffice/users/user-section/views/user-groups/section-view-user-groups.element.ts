import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import './workspace-view-user-groups.element';

@customElement('umb-section-view-user-groups')
export class UmbSectionViewUserGroupsElement extends LitElement {
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

	render() {
		return html`<umb-workspace-view-user-groups></umb-workspace-view-user-groups>`;
	}
}

export default UmbSectionViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-user-groups': UmbSectionViewUserGroupsElement;
	}
}
