import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';

@customElement('umb-settings-section-tree')
class UmbSettingsSectionTree extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];

	render() {
		return html`
			<a href="${'/section/settings'}">
				<h3>Settings</h3>
			</a>

			<!-- TODO: hardcoded tree items. These should come the extensions -->
			<uui-menu-item label="Extensions" href="/section/settings/extensions"></uui-menu-item>
			<uui-menu-item label="Data Types" href="/section/settings/data-types"></uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-settings-section-tree': UmbSettingsSectionTree;
	}
}
