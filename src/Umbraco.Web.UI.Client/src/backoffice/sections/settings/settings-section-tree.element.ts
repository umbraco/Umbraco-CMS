import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { data } from '../../../mocks/data/data-type.data';

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

	// TODO: implement dynamic tree data
	@state()
	_dataTypes: Array<any> = data;

	render() {
		return html`
			<a href="${'/section/settings'}">
				<h3>Settings</h3>
			</a>

			<!-- TODO: hardcoded tree items. These should come the extensions -->
			<uui-menu-item label="Extensions" href="/section/settings/extensions"></uui-menu-item>
			<uui-menu-item label="Data Types" has-children>
				${this._dataTypes.map(
					(dataType) => html`
						<uui-menu-item label="${dataType.name}" href="/section/settings/data-type/${dataType.id}"></uui-menu-item>
					`
				)}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-settings-section-tree': UmbSettingsSectionTree;
	}
}
