import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';

import './packages-created-item.element';

@customElement('umb-packages-created-overview')
export class UmbPackagesCreatedOverviewElement extends LitElement {
	// TODO: implement call to backend
	// TODO: add correct model for created packages
	@state()
	private _createdPackages: any[] = [
		{
			alias: 'my.package',
			key: '2a0181ec-244b-4068-a1d7-2f95ed7e6da6',
			name: 'A created package',
			plans: [],
			version: '1.0.0',
		},
	];

	render() {
		return html`<uui-box headline="Created packages">
			<uui-ref-list>
				${repeat(
					this._createdPackages,
					(item) => item.key,
					(item) => html`<umb-packages-created-item .package=${item}></umb-packages-created-item>`
				)}
			</uui-ref-list>
		</uui-box>`;
	}
}

export default UmbPackagesCreatedOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-created-overview': UmbPackagesCreatedOverviewElement;
	}
}
