import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { ModelEntryType } from './sorter-group.js';

import './sorter-group.js';
@customElement('example-sorter-dashboard')
export class ExampleSorterDashboard extends UmbElementMixin(LitElement) {
	groupOneItems: ModelEntryType[] = [
		{
			name: 'Apple',
		},
		{
			name: 'Banana',
		},
		{
			name: 'Pear',
		},
		{
			name: 'Pineapple',
		},
		{
			name: 'Lemon',
		},
	];

	groupTwoItems: ModelEntryType[] = [
		{
			name: 'DXP',
		},
		{
			name: 'H5YR',
		},
		{
			name: 'UUI',
		},
	];

	override render() {
		return html`
			<uui-box class="uui-text">
				<div class="outer-wrapper">
					<example-sorter-group .items=${this.groupOneItems}></example-sorter-group>
					<example-sorter-group .items=${this.groupTwoItems}></example-sorter-group>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.outer-wrapper {
				display: flex;
				gap: var(--uui-size-layout-1);
			}
		`,
	];
}

export default ExampleSorterDashboard;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-dashboard': ExampleSorterDashboard;
	}
}
